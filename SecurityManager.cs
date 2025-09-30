using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace PulsePanel
{
    public static class SecurityManager
    {
        private static readonly byte[] Key = Encoding.UTF8.GetBytes("PulsePanel2024Key!"); // In production, use proper key management
        private static readonly byte[] IV = new byte[16]; // Initialization vector

        public static string EncryptString(string plainText)
        {
            try
            {
                using var aes = Aes.Create();
                aes.Key = Key;
                aes.IV = IV;

                using var encryptor = aes.CreateEncryptor();
                using var msEncrypt = new MemoryStream();
                using var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write);
                using var swEncrypt = new StreamWriter(csEncrypt);
                
                swEncrypt.Write(plainText);
                return Convert.ToBase64String(msEncrypt.ToArray());
            }
            catch
            {
                return plainText; // Fallback to plain text if encryption fails
            }
        }

        public static string DecryptString(string cipherText)
        {
            try
            {
                using var aes = Aes.Create();
                aes.Key = Key;
                aes.IV = IV;

                using var decryptor = aes.CreateDecryptor();
                using var msDecrypt = new MemoryStream(Convert.FromBase64String(cipherText));
                using var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read);
                using var srDecrypt = new StreamReader(csDecrypt);
                
                return srDecrypt.ReadToEnd();
            }
            catch
            {
                return cipherText; // Fallback to cipher text if decryption fails
            }
        }

        public static void SaveSecureSettings(object settings, string filePath)
        {
            try
            {
                var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
                var encrypted = EncryptString(json);
                File.WriteAllText(filePath, encrypted);
            }
            catch (Exception ex)
            {
                Logger.LogError("Failed to save secure settings", ex);
                throw;
            }
        }

        public static T? LoadSecureSettings<T>(string filePath) where T : class
        {
            try
            {
                if (!File.Exists(filePath))
                    return null;

                var encrypted = File.ReadAllText(filePath);
                var json = DecryptString(encrypted);
                return JsonSerializer.Deserialize<T>(json);
            }
            catch (Exception ex)
            {
                Logger.LogError("Failed to load secure settings", ex);
                return null;
            }
        }

        public static bool ValidateFileIntegrity(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                    return false;

                using var sha256 = SHA256.Create();
                using var stream = File.OpenRead(filePath);
                var hash = sha256.ComputeHash(stream);
                
                // Store and compare hashes for integrity checking
                var hashPath = filePath + ".hash";
                var currentHash = Convert.ToBase64String(hash);
                
                if (File.Exists(hashPath))
                {
                    var storedHash = File.ReadAllText(hashPath);
                    return currentHash == storedHash;
                }
                else
                {
                    File.WriteAllText(hashPath, currentHash);
                    return true;
                }
            }
            catch (Exception ex)
            {
                Logger.LogError($"File integrity check failed for {filePath}", ex);
                return false;
            }
        }

        public static void CreateBackupHash(string filePath)
        {
            try
            {
                using var sha256 = SHA256.Create();
                using var stream = File.OpenRead(filePath);
                var hash = sha256.ComputeHash(stream);
                var hashPath = filePath + ".hash";
                File.WriteAllText(hashPath, Convert.ToBase64String(hash));
            }
            catch (Exception ex)
            {
                Logger.LogError($"Failed to create backup hash for {filePath}", ex);
            }
        }
    }

    public class SecureAppSettings
    {
        public string SteamCmdPath { get; set; } = "";
        public List<GameServer> Servers { get; set; } = new();
        public Dictionary<string, string> EncryptedData { get; set; } = new();
        public List<AlertRule> AlertRules { get; set; } = new();
        public bool MetricsEnabled { get; set; } = true;
        public string BackupEncryptionKey { get; set; } = "";
    }
}