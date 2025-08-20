using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace PulsePanel.Blueprints;

public class TokenReplacer
{
    private readonly Dictionary<string, object> _values;
    private static readonly Regex SimpleTokenRegex = new(@"\{\{([a-zA-Z0-9_.-]+?)(?:\s*\|\s*default:\s*(.+?))?\s*\}\}", RegexOptions.Compiled);
    private static readonly Regex BlockTokenRegex = new(@"\{\{([#/])(if|else)\s*(.*?)\}\}", RegexOptions.Compiled);

    public TokenReplacer(Dictionary<string, object> values)
    {
        _values = values;
    }

    public string Replace(string templateContent)
    {
        // Normalize line endings
        templateContent = templateContent.Replace("\r\n", "\n");
        return ProcessBlock(new StringScanner(templateContent));
    }

    private string ProcessBlock(StringScanner scanner, bool isMainBlock = true)
    {
        var output = new StringBuilder();
        while (!scanner.IsDone)
        {
            // 1. Scan for normal text until a block `{{` is found
            output.Append(scanner.ScanUntil(BlockTokenRegex));
            if (scanner.IsDone) break;

            var match = scanner.Match(BlockTokenRegex);
            scanner.Advance(match.Length);

            var type = match.Groups[1].Value; // # or /
            var command = match.Groups[2].Value; // if, else
            var conditionKey = match.Groups[3].Value.Trim();

            if (type == "#" && command == "if")
            {
                bool conditionResult = EvaluateCondition(conditionKey);
                var innerContent = ProcessBlock(scanner, false);
                if (conditionResult)
                {
                    output.Append(innerContent);
                }
            }
            else if (type == "/" && command == "if")
            {
                if (!isMainBlock) return output.ToString();
            }
            // Note: A full implementation would need to handle {{else}} correctly,
            // which requires more complex state management. This is a simplified version.
        }

        // Finally, replace simple tokens in the processed block
        return SimpleTokenRegex.Replace(output.ToString(), SimpleMatchEvaluator);
    }

    private string SimpleMatchEvaluator(Match match)
    {
        var key = match.Groups[1].Value;
        var defaultValue = match.Groups[2].Success ? match.Groups[2].Value : null;

        var resolvedValue = ResolvePath(key);
        if (resolvedValue != null)
        {
            return resolvedValue;
        }
        if (defaultValue != null)
        {
            return defaultValue;
        }
        return match.Value; // Return original token if not found and no default
    }

    private bool EvaluateCondition(string key)
    {
        var valueStr = ResolvePath(key);
        if (valueStr == null) return false;

        // Bools are true if "true", case-insensitive
        if (bool.TryParse(valueStr, out bool val)) return val;

        // Numbers are true if not 0
        if (double.TryParse(valueStr, out double num)) return num != 0;

        // Strings are true if not empty
        return !string.IsNullOrEmpty(valueStr);
    }

    private string? ResolvePath(string key)
    {
        var path = key.Split('.');
        object? currentValue = _values;
        foreach (var part in path)
        {
            if (currentValue is Dictionary<string, object> dict && dict.TryGetValue(part, out var nextValue))
            {
                currentValue = nextValue;
            }
            else if (currentValue is JsonElement element && element.ValueKind == JsonValueKind.Object)
            {
                if (element.TryGetProperty(part, out var property))
                {
                    currentValue = property;
                }
                else { currentValue = null; break; }
            }
            else { currentValue = null; break; }
        }

        if (currentValue is JsonElement finalElement)
        {
            return finalElement.ValueKind switch
            {
                JsonValueKind.String => finalElement.GetString(),
                JsonValueKind.Number => finalElement.GetRawText(),
                JsonValueKind.True => "true",
                JsonValueKind.False => "false",
                _ => null
            };
        }

        return currentValue?.ToString();
    }
}

// A helper class to make scanning through the template string easier.
internal class StringScanner
{
    private readonly string _source;
    public int Position { get; private set; }
    public bool IsDone => Position >= _source.Length;

    public StringScanner(string source)
    {
        _source = source;
        Position = 0;
    }

    public string ScanUntil(Regex regex)
    {
        var match = regex.Match(_source, Position);
        if (!match.Success)
        {
            var remaining = _source.Substring(Position);
            Position = _source.Length;
            return remaining;
        }

        var text = _source.Substring(Position, match.Index - Position);
        Position = match.Index;
        return text;
    }

    public Match Match(Regex regex) => regex.Match(_source, Position);
    public void Advance(int count) => Position += count;
}
