using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using PulsePanel.App.Models;

namespace PulsePanel.App.Services
{
    public sealed class HttpAgentClient : IAgentClient
    {
        private readonly HttpClient _http;

        public HttpAgentClient(HttpClient http) { _http = http; }

        public async Task<NodeStatus> PingAsync(NodeInfo node, CancellationToken ct)
        {
            try
            {
                var resp = await _http.GetAsync($"{node.Url}/api/agent/ping", ct);
                return resp.IsSuccessStatusCode ? NodeStatus.Online : NodeStatus.Offline;
            }
            catch { return NodeStatus.Offline; }
        }

        public async Task ExecuteAsync(NodeInfo node, Blueprint blueprint, ExecutionActionType action, ExecutionOptions options, IProvenanceLogger log, CancellationToken ct)
        {
            var payload = new
            {
                blueprint = blueprint, // ensure JSON‑serializable
                action = action.ToString(),
                dryRun = options.DryRun,
                replaySessionId = options.ReplaySessionId
            };

            var req = new HttpRequestMessage(HttpMethod.Post, $"{node.Url}/api/agent/execute")
            {
                Content = JsonContent.Create(payload)
            };
            if (!string.IsNullOrEmpty(node.ApiKey))
                req.Headers.Add("X-API-Key", node.ApiKey);

            var resp = await _http.SendAsync(req, ct);
            if (!resp.IsSuccessStatusCode)
            {
                var body = await resp.Content.ReadAsStringAsync(ct);
                await log.LogError("RemoteExecution", node.Id, $"Remote execution failed ({node.Name}): {resp.StatusCode} {body}");
            }
            else
            {
                await log.LogAction("RemoteExecution", node.Id, $"Remote execution accepted by {node.Name}.");
            }
        }
    }
}