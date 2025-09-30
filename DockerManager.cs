using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace PulsePanel
{
    public class DockerManager
    {
        public DockerManager()
        {
        }

        public async Task<bool> IsDockerAvailable()
        {
            try
            {
                var result = await RunDockerCommand("--version");
                return result.Success;
            }
            catch
            {
                return false;
            }
        }

        public async Task<List<DockerContainer>> GetContainers()
        {
            var result = await RunDockerCommand("ps -a --format \"{{.ID}}|{{.Names}}|{{.Status}}|{{.Ports}}\"");
            if (!result.Success) return new List<DockerContainer>();

            return result.Output.Split('\n', StringSplitOptions.RemoveEmptyEntries)
                .Select(line => line.Split('|'))
                .Where(parts => parts.Length >= 4)
                .Select(parts => new DockerContainer
                {
                    Id = parts[0],
                    Name = parts[1],
                    Status = parts[2],
                    Ports = parts[3]
                }).ToList();
        }

        public async Task<bool> CreateContainer(string name, string image, Dictionary<string, string> ports, Dictionary<string, string> volumes, Dictionary<string, string> environment)
        {
            var portArgs = string.Join(" ", ports.Select(p => $"-p {p.Key}:{p.Value}"));
            var volumeArgs = string.Join(" ", volumes.Select(v => $"-v \"{v.Key}:{v.Value}\""));
            var envArgs = string.Join(" ", environment.Select(e => $"-e {e.Key}={e.Value}"));

            var command = $"run -d --name {name} {portArgs} {volumeArgs} {envArgs} {image}";
            var result = await RunDockerCommand(command);
            
            if (result.Success)
                Logger.LogInfo($"Docker container '{name}' created successfully");
            else
                Logger.LogError($"Failed to create container '{name}': {result.Error}");

            return result.Success;
        }

        public async Task<bool> StartContainer(string name)
        {
            var result = await RunDockerCommand($"start {name}");
            if (result.Success)
                Logger.LogInfo($"Docker container '{name}' started");
            return result.Success;
        }

        public async Task<bool> StopContainer(string name)
        {
            var result = await RunDockerCommand($"stop {name}");
            if (result.Success)
                Logger.LogInfo($"Docker container '{name}' stopped");
            return result.Success;
        }

        public async Task<bool> RemoveContainer(string name)
        {
            var result = await RunDockerCommand($"rm -f {name}");
            if (result.Success)
                Logger.LogInfo($"Docker container '{name}' removed");
            return result.Success;
        }

        public async Task<string> GetContainerLogs(string name, int lines = 100)
        {
            var result = await RunDockerCommand($"logs --tail {lines} {name}");
            return result.Success ? result.Output : string.Empty;
        }

        private async Task<DockerResult> RunDockerCommand(string arguments)
        {
            try
            {
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "docker",
                        Arguments = arguments,
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true
                    }
                };

                process.Start();
                var output = await process.StandardOutput.ReadToEndAsync();
                var error = await process.StandardError.ReadToEndAsync();
                await process.WaitForExitAsync();

                return new DockerResult
                {
                    Success = process.ExitCode == 0,
                    Output = output.Trim(),
                    Error = error.Trim()
                };
            }
            catch (Exception ex)
            {
                return new DockerResult
                {
                    Success = false,
                    Error = ex.Message
                };
            }
        }
    }

    public class DockerContainer
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Status { get; set; }
        public string Ports { get; set; }
        public bool IsRunning => Status.Contains("Up");
    }

    public class DockerResult
    {
        public bool Success { get; set; }
        public string Output { get; set; }
        public string Error { get; set; }
    }
}