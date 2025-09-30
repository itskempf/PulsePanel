using System.Diagnostics;

namespace PulsePanel
{
    public static class FirewallManager
    {
        public static async Task<bool> OpenFirewallPort(int port, string ruleName, bool isUdp = true)
        {
            try
            {
                var protocol = isUdp ? "UDP" : "TCP";
                var direction = "in";
                
                // Create inbound rule
                var inboundArgs = $"advfirewall firewall add rule name=\"{ruleName} - Inbound {protocol}\" dir={direction} action=allow protocol={protocol} localport={port}";
                var inboundResult = await RunNetshCommand(inboundArgs);
                
                // Create outbound rule
                direction = "out";
                var outboundArgs = $"advfirewall firewall add rule name=\"{ruleName} - Outbound {protocol}\" dir={direction} action=allow protocol={protocol} localport={port}";
                var outboundResult = await RunNetshCommand(outboundArgs);
                
                return inboundResult && outboundResult;
            }
            catch
            {
                return false;
            }
        }

        public static async Task<bool> CloseFirewallPort(string ruleName)
        {
            try
            {
                var inboundArgs = $"advfirewall firewall delete rule name=\"{ruleName} - Inbound UDP\"";
                var outboundArgs = $"advfirewall firewall delete rule name=\"{ruleName} - Outbound UDP\"";
                var inboundTcpArgs = $"advfirewall firewall delete rule name=\"{ruleName} - Inbound TCP\"";
                var outboundTcpArgs = $"advfirewall firewall delete rule name=\"{ruleName} - Outbound TCP\"";
                
                await RunNetshCommand(inboundArgs);
                await RunNetshCommand(outboundArgs);
                await RunNetshCommand(inboundTcpArgs);
                await RunNetshCommand(outboundTcpArgs);
                
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static async Task<bool> CheckFirewallRule(string ruleName)
        {
            try
            {
                var args = $"advfirewall firewall show rule name=\"{ruleName} - Inbound UDP\"";
                var result = await RunNetshCommand(args);
                return result;
            }
            catch
            {
                return false;
            }
        }

        public static async Task<bool> IsPortInUse(int port)
        {
            try
            {
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "netstat",
                        Arguments = "-an",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        CreateNoWindow = true
                    }
                };

                process.Start();
                var output = await process.StandardOutput.ReadToEndAsync();
                await process.WaitForExitAsync();
                
                return output.Contains($":{port} ") || output.Contains($":{port}\t");
            }
            catch
            {
                return false;
            }
        }

        public static async Task<List<int>> GetUsedPorts()
        {
            var usedPorts = new List<int>();
            
            try
            {
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "netstat",
                        Arguments = "-an",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        CreateNoWindow = true
                    }
                };

                process.Start();
                var output = await process.StandardOutput.ReadToEndAsync();
                await process.WaitForExitAsync();
                
                var lines = output.Split('\n');
                foreach (var line in lines)
                {
                    if (line.Contains(":") && (line.Contains("LISTENING") || line.Contains("UDP")))
                    {
                        var parts = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                        if (parts.Length > 1)
                        {
                            var address = parts[1];
                            var portIndex = address.LastIndexOf(':');
                            if (portIndex > 0 && int.TryParse(address.Substring(portIndex + 1), out int port))
                            {
                                usedPorts.Add(port);
                            }
                        }
                    }
                }
            }
            catch { }
            
            return usedPorts.Distinct().ToList();
        }

        private static async Task<bool> RunNetshCommand(string arguments)
        {
            try
            {
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "netsh",
                        Arguments = arguments,
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true,
                        Verb = "runas" // Run as administrator
                    }
                };

                process.Start();
                await process.WaitForExitAsync();
                
                return process.ExitCode == 0;
            }
            catch
            {
                return false;
            }
        }
    }
}