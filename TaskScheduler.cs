using System.Diagnostics;

namespace PulsePanel
{
    public static class TaskScheduler
    {
        public static async Task<bool> CreateScheduledTask(string taskName, string schedule, string action, string arguments = "")
        {
            try
            {
                var scheduleType = schedule switch
                {
                    "Daily at 3:00 AM" => "DAILY /ST 03:00",
                    "Weekly (Monday 3:00 AM)" => "WEEKLY /D MON /ST 03:00",
                    "Weekly (Sunday 3:00 AM)" => "WEEKLY /D SUN /ST 03:00",
                    "Every 6 hours" => "DAILY /ST 00:00 /RI 360",
                    "Every 12 hours" => "DAILY /ST 00:00 /RI 720",
                    "Every 24 hours" => "DAILY /ST 00:00",
                    _ => "DAILY /ST 03:00"
                };

                var command = $"schtasks /CREATE /TN \"{taskName}\" /TR \"{action} {arguments}\" /SC {scheduleType} /F";
                
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "cmd.exe",
                        Arguments = $"/C {command}",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true
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

        public static async Task<bool> DeleteScheduledTask(string taskName)
        {
            try
            {
                var command = $"schtasks /DELETE /TN \"{taskName}\" /F";
                
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "cmd.exe",
                        Arguments = $"/C {command}",
                        UseShellExecute = false,
                        CreateNoWindow = true
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

        public static async Task<bool> TaskExists(string taskName)
        {
            try
            {
                var command = $"schtasks /QUERY /TN \"{taskName}\"";
                
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "cmd.exe",
                        Arguments = $"/C {command}",
                        UseShellExecute = false,
                        CreateNoWindow = true
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