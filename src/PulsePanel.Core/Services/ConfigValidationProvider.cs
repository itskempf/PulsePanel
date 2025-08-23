using System;
using System.IO;
using System.Text.Json;
using YamlDotNet.Serialization;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;

namespace PulsePanel.Core.Services
{
    public class ConfigValidationProvider : IConfigValidationProvider
    {
        private readonly IProvenanceLogger _logger;

        public ConfigValidationProvider(IProvenanceLogger logger)
        {
            _logger = logger;
        }

        public ValidationResult Validate(string filePath)
        {
            var ext = Path.GetExtension(filePath).ToLowerInvariant();
            try
            {
                switch (ext)
                {
                    case ".json":
                        JsonDocument.Parse(File.ReadAllText(filePath));
                        break;

                    case ".yml":
                    case ".yaml":
                        var deserializer = new DeserializerBuilder().Build();
                        deserializer.Deserialize<object>(File.ReadAllText(filePath));
                        break;

                    case ".ini":
                        // TODO: Implement INI parsing/validation
                        break;

                    case ".xml":
                        var doc = new System.Xml.XmlDocument();
                        doc.Load(filePath);
                        break;

                    default:
                        return ValidationResult.Warning("Unknown file type");
                }

                _logger.Log(new ProvenanceEvent
                {
                    Action = "ConfigValidated",
                    Timestamp = DateTime.UtcNow,
                    Metadata = new { File = filePath, Type = ext, Result = "Success" }
                });

                return ValidationResult.Success();
            }
            catch (Exception ex)
            {
                _logger.Log(new ProvenanceEvent
                {
                    Action = "ConfigValidationFailed",
                    Timestamp = DateTime.UtcNow,
                    Metadata = new { File = filePath, Type = ext, Error = ex.Message }
                });

                return ValidationResult.Error($"Validation failed: {ex.Message}");
            }
        }
    }
}
