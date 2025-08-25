using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using PulsePanel.Core.Models;

namespace PulsePanel.Core.Services
{
    public sealed class BlueprintService : IBlueprintService
    {
        private readonly string _dir;
        private const string TemplateVarPattern = @"\{\{(?<v>[A-Z0-9_]+)\}\}";
        public BlueprintService()
        {
            _dir = Path.Combine(AppContext.BaseDirectory, "Data", "Blueprints");
            Directory.CreateDirectory(_dir);
        }
        public async Task<IReadOnlyList<Blueprint>> GetBlueprintsAsync()
        {
            var list = new List<Blueprint>();
            foreach (var bpDir in Directory.EnumerateDirectories(_dir))
            {
                var file = Path.Combine(bpDir, "blueprint.json");
                if (File.Exists(file))
                {
                    try
                    {
                        var json = await File.ReadAllTextAsync(file);
                        var bp = JsonSerializer.Deserialize<Blueprint>(json, new JsonSerializerOptions(JsonSerializerDefaults.Web)) ?? new Blueprint();
                        list.Add(bp);
                    }
                    catch { }
                }
            }
            return list.OrderBy(b=>b.Name).ToList();
        }
        public async Task<string[]> GetTemplateVariablesAsync(Blueprint blueprint)
        {
            var vars = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (Match m in Regex.Matches(blueprint.Arguments ?? string.Empty, TemplateVarPattern))
                vars.Add(m.Groups["v"].Value);
            string folder = Path.Combine(_dir, blueprint.Id);
            if (!Directory.Exists(folder)) folder = Path.Combine(_dir, blueprint.Name.Replace(':','_'));
            string templates = Path.Combine(folder, "templates");
            if (Directory.Exists(templates))
            {
                foreach (var f in Directory.EnumerateFiles(templates, "*.template", SearchOption.AllDirectories))
                {
                    try
                    {
                        var text = await File.ReadAllTextAsync(f);
                        foreach (Match m in Regex.Matches(text, TemplateVarPattern))
                            vars.Add(m.Groups["v"].Value);
                    }
                    catch { }
                }
            }
            return vars.ToArray();
        }
    }
}
