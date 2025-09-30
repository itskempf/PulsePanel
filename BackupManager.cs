using System.IO;
using System.IO.Compression;

namespace PulsePanel
{
    public static class BackupManager
    {
        public static async Task<string> CreateFullBackup(GameServer server, string backupPath)
        {
            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var backupFileName = $"{server.Name}_Full_{timestamp}.zip";
            var fullBackupPath = Path.Combine(backupPath, backupFileName);

            await Task.Run(() =>
            {
                ZipFile.CreateFromDirectory(server.InstallPath, fullBackupPath, CompressionLevel.Optimal, false);
            });

            return backupFileName;
        }

        public static async Task<string> CreateIncrementalBackup(GameServer server, string backupPath, DateTime lastBackupTime)
        {
            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var backupFileName = $"{server.Name}_Incremental_{timestamp}.zip";
            var fullBackupPath = Path.Combine(backupPath, backupFileName);

            await Task.Run(() =>
            {
                using var archive = ZipFile.Open(fullBackupPath, ZipArchiveMode.Create);
                AddModifiedFiles(server.InstallPath, archive, lastBackupTime, "");
            });

            return backupFileName;
        }

        private static void AddModifiedFiles(string sourcePath, ZipArchive archive, DateTime lastBackupTime, string relativePath)
        {
            var directory = new DirectoryInfo(sourcePath);
            
            foreach (var file in directory.GetFiles())
            {
                if (file.LastWriteTime > lastBackupTime)
                {
                    var entryName = Path.Combine(relativePath, file.Name).Replace('\\', '/');
                    archive.CreateEntryFromFile(file.FullName, entryName);
                }
            }

            foreach (var subDir in directory.GetDirectories())
            {
                var newRelativePath = Path.Combine(relativePath, subDir.Name);
                AddModifiedFiles(subDir.FullName, archive, lastBackupTime, newRelativePath);
            }
        }

        public static async Task<bool> RestoreBackup(string backupFilePath, string restorePath)
        {
            try
            {
                if (Directory.Exists(restorePath))
                {
                    var backupDir = $"{restorePath}_backup_{DateTime.Now:yyyyMMdd_HHmmss}";
                    Directory.Move(restorePath, backupDir);
                }

                await Task.Run(() =>
                {
                    ZipFile.ExtractToDirectory(backupFilePath, restorePath);
                });

                return true;
            }
            catch
            {
                return false;
            }
        }

        public static List<BackupInfo> GetBackupList(string backupPath, string serverName)
        {
            var backups = new List<BackupInfo>();
            
            if (!Directory.Exists(backupPath))
                return backups;

            var backupFiles = Directory.GetFiles(backupPath, $"{serverName}_*.zip");
            
            foreach (var file in backupFiles)
            {
                var fileInfo = new FileInfo(file);
                var fileName = Path.GetFileNameWithoutExtension(file);
                var parts = fileName.Split('_');
                
                if (parts.Length >= 3)
                {
                    var isIncremental = parts.Contains("Incremental");
                    var dateStr = parts[^2] + parts[^1]; // Last two parts are date and time
                    
                    if (DateTime.TryParseExact(dateStr, "yyyyMMddHHmmss", null, System.Globalization.DateTimeStyles.None, out var backupDate))
                    {
                        backups.Add(new BackupInfo
                        {
                            FileName = fileInfo.Name,
                            FilePath = fileInfo.FullName,
                            BackupDate = backupDate,
                            Size = fileInfo.Length,
                            IsIncremental = isIncremental
                        });
                    }
                }
            }

            return backups.OrderByDescending(b => b.BackupDate).ToList();
        }

        public static Task CleanOldBackups(string backupPath, string serverName, int retentionDays)
        {
            return Task.Run(() =>
            {
                var cutoffDate = DateTime.Now.AddDays(-retentionDays);
                var backups = GetBackupList(backupPath, serverName);
                
                foreach (var backup in backups.Where(b => b.BackupDate < cutoffDate))
                {
                    try
                    {
                        File.Delete(backup.FilePath);
                    }
                    catch { }
                }
            });
        }
    }

    public class BackupInfo
    {
        public string FileName { get; set; } = "";
        public string FilePath { get; set; } = "";
        public DateTime BackupDate { get; set; }
        public long Size { get; set; }
        public bool IsIncremental { get; set; }
        
        public string SizeFormatted => FormatBytes(Size);
        public string TypeFormatted => IsIncremental ? "Incremental" : "Full";
        
        private static string FormatBytes(long bytes)
        {
            string[] suffixes = { "B", "KB", "MB", "GB", "TB" };
            int counter = 0;
            decimal number = bytes;
            while (Math.Round(number / 1024) >= 1)
            {
                number /= 1024;
                counter++;
            }
            return $"{number:n1} {suffixes[counter]}";
        }
    }
}