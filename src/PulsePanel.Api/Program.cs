using System.Text.Json;
using System.Text.RegularExpressions;
using PulsePanel.Api.Models;
using PulsePanel.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(k => {
    var portEnv = Environment.GetEnvironmentVariable("PULSEPANEL_PORT");
    int port = 8070;
    if (int.TryParse(portEnv, out var p)) port = p;
    k.Listen(System.Net.IPAddress.Parse(Environment.GetEnvironmentVariable("PULSEPANEL_BIND") ?? "127.0.0.1"), port);
});

builder.Services.AddSingleton<ServerRegistry>();
builder.Services.AddSingleton<ServerStore>();
builder.Services.AddSingleton<SteamCmdService>();
builder.Services.AddSingleton<ServerProcessService>();

builder.Services.AddRouting();
builder.Services.AddDirectoryBrowser();

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

app.MapGet("/api/games", (ServerRegistry reg) =>
{
    var games = reg.Games.Select(g => new {
        id = g.Id, label = g.Label, defaultPorts = g.DefaultPorts, tokens = g.Tokens
    });
    return Results.Json(games);
});

app.MapGet("/api/servers", (ServerStore store) =>
{
    var list = store.Load();
    return Results.Json(list.Select(s => new {
        s.Id, s.Name, s.Game, s.Status, s.Pid, s.CreatedAt, s.Ports, s.Tokens, s.InstallDir
    }));
});

app.MapPost("/api/servers", async (HttpContext ctx, ServerRegistry reg, ServerStore store, ServerProcessService procs) =>
{
    using var doc = await JsonDocument.ParseAsync(ctx.Request.Body);
    var root = doc.RootElement;

    var name = root.GetProperty("name").GetString() ?? "";
    var gameId = root.GetProperty("game").GetString() ?? "";
    var overrides = root.TryGetProperty("overrides", out var ovr) ? ovr : default;

    if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(gameId))
        return Results.BadRequest(new { error = "name and game are required" });

    var def = reg.Get(gameId);
    if (def is null) return Results.BadRequest(new { error = "unknown game id" });

    var id = Guid.NewGuid().ToString("N")[..8];
    var s = new ServerEntry {
        Id = id,
        Name = name,
        Game = def.Id,
        Status = "created",
        Ports = new(def.DefaultPorts),
        Tokens = new(def.Tokens),
        InstallDir = "" // filled below
    };

    // Apply overrides
    if (overrides.ValueKind != JsonValueKind.Undefined && overrides.ValueKind != JsonValueKind.Null) {
        if (overrides.TryGetProperty("ports", out var p)) {
            foreach (var kv in p.EnumerateObject()) s.Ports[kv.Name] = kv.Value.GetInt32();
        }
        if (overrides.TryGetProperty("tokens", out var t)) {
            foreach (var kv in t.EnumerateObject()) s.Tokens[kv.Name] = kv.Value.ValueKind switch {
                JsonValueKind.Number => kv.Value.TryGetInt32(out var i) ? i : (object)kv.Value.GetDouble(),
                JsonValueKind.True => true,
                JsonValueKind.False => false,
                _ => kv.Value.GetString() ?? ""
            };
        }
    }

    // Layout
    var env = app.Environment;
    var rootDir = Path.GetFullPath(Path.Combine(env.ContentRootPath, "..", ".."));
    var serverDir = Path.Combine(rootDir, "servers", id);
    var filesDir = Path.Combine(serverDir, "files");
    var configDir = Path.Combine(serverDir, "config");
    Directory.CreateDirectory(filesDir);
    Directory.CreateDirectory(configDir);
    s.InstallDir = filesDir;

    // Persist
    var list = store.Load();
    list.Insert(0, s);
    store.Save(list);

    return Results.Json(s);
});

app.MapPost("/api/servers/{id}/install", async (string id, ServerStore store, ServerRegistry reg, SteamCmdService steam) =>
{
    var list = store.Load();
    var s = list.FirstOrDefault(x => x.Id == id);
    if (s is null) return Results.NotFound();

    var def = reg.Get(s.Game);
    if (def is null) return Results.BadRequest(new { error = "unknown game id" });

    s.Status = "installing";
    store.Save(list);

    int exit = await steam.AppUpdateAsync(def.Install.AppId, s.InstallDir, validate: true);
    s.Status = (exit == 0) ? "stopped" : "error";
    store.Save(list);

    return Results.Json(new { ok = exit == 0, exitCode = exit });
});

string ReplaceTokens(string input, Dictionary<string, object> tokens) {
    return Regex.Replace(input, @"\{\{(\w+)\}\}", m => {
        var key = m.Groups[1].Value;
        return tokens.TryGetValue(key, out var val) ? Convert.ToString(val) ?? "" : m.Value;
    });
}

app.MapPost("/api/servers/{id}/start", (string id, ServerStore store, ServerRegistry reg, ServerProcessService procs) =>
{
    var list = store.Load();
    var s = list.FirstOrDefault(x => x.Id == id);
    if (s is null) return Results.NotFound();

    var def = reg.Get(s.Game);
    if (def is null) return Results.BadRequest(new { error = "unknown game id" });

    string args = ReplaceTokens(def.Start.Args, s.Tokens);
    var (proc, err) = procs.Start(s, def.Start.Exec, args);
    if (err != null || proc == null) {
        s.Status = "error";
        store.Save(list);
        return Results.Json(new { ok = false, error = err?.Message ?? "failed to start" }, statusCode: 500);
    }
    s.Pid = proc.Id;
    s.Status = "running";
    store.Save(list);
    return Results.Json(new { ok = true, pid = s.Pid });
});

app.MapPost("/api/servers/{id}/stop", (string id, ServerStore store, ServerProcessService procs) =>
{
    var list = store.Load();
    var s = list.FirstOrDefault(x => x.Id == id);
    if (s is null) return Results.NotFound();

    if (s.Pid is int pid) {
        procs.Stop(pid);
        s.Pid = null;
    }
    s.Status = "stopped";
    store.Save(list);
    return Results.Json(new { ok = true });
});

app.MapPost("/api/servers/{id}/restart", (string id, ServerStore store, ServerRegistry reg, ServerProcessService procs) =>
{
    var list = store.Load();
    var s = list.FirstOrDefault(x => x.Id == id);
    if (s is null) return Results.NotFound();

    if (s.Pid is int pid) procs.Stop(pid);
    s.Status = "stopped";
    store.Save(list);

    var def = reg.Get(s.Game)!;
    string args = ReplaceTokens(def.Start.Args, s.Tokens);
    var (proc, err) = procs.Start(s, def.Start.Exec, args);
    if (err != null || proc == null) {
        s.Status = "error";
        store.Save(list);
        return Results.Json(new { ok = false }, statusCode: 500);
    }
    s.Pid = proc.Id; s.Status = "running"; store.Save(list);
    return Results.Json(new { ok = true, pid = s.Pid });
});

app.MapGet("/api/servers/{id}/logs", (string id, int tail, string? stream, ServerStore store, ServerProcessService procs) =>
{
    var list = store.Load();
    var s = list.FirstOrDefault(x => x.Id == id);
    if (s is null) return Results.NotFound();

    var logPath = Path.Combine(procs.GetServerDir(id), "logs", (stream == "err" ? "server.err.log" : "server.out.log"));
    if (!System.IO.File.Exists(logPath)) return Results.Text("");
    var lines = System.IO.File.ReadLines(logPath);
    if (tail > 0) lines = lines.Reverse().Take(tail).Reverse();
    return Results.Text(string.Join(Environment.NewLine, lines), "text/plain");
});

app.MapGet("/api/servers/{id}/config", (string id, ServerStore store) =>
{
    var list = store.Load();
    var s = list.FirstOrDefault(x => x.Id == id);
    if (s is null) return Results.NotFound();
    return Results.Json(new { tokens = s.Tokens, ports = s.Ports });
});

app.MapPut("/api/servers/{id}/config", async (string id, ServerStore store, HttpContext ctx) =>
{
    var list = store.Load();
    var s = list.FirstOrDefault(x => x.Id == id);
    if (s is null) return Results.NotFound();

    using var doc = await System.Text.Json.JsonDocument.ParseAsync(ctx.Request.Body);
    if (doc.RootElement.TryGetProperty("tokens", out var t)) {
        foreach (var kv in t.EnumerateObject()) {
            s.Tokens[kv.Name] = kv.Value.ValueKind switch {
                System.Text.Json.JsonValueKind.Number => kv.Value.TryGetInt32(out var i) ? i : (object)kv.Value.GetDouble(),
                System.Text.Json.JsonValueKind.True => true,
                System.Text.Json.JsonValueKind.False => false,
                _ => kv.Value.GetString() ?? ""
            };
        }
    }
    if (doc.RootElement.TryGetProperty("ports", out var p)) {
        foreach (var kv in p.EnumerateObject()) s.Ports[kv.Name] = kv.Value.GetInt32();
    }
    store.Save(list);
    return Results.Json(new { ok = true });
});

app.MapDelete("/api/servers/{id}", (string id, ServerStore store, ServerProcessService procs) =>
{
    var list = store.Load();
    var s = list.FirstOrDefault(x => x.Id == id);
    if (s is null) return Results.NotFound();

    if (s.Pid is int pid) procs.Stop(pid);
    var dir = procs.GetServerDir(id);
    try { Directory.Delete(dir, true); } catch { }
    list.RemoveAll(x => x.Id == id);
    store.Save(list);
    return Results.Json(new { ok = true });
});

app.Run();
