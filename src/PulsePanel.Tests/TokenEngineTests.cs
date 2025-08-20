using PulsePanel.Blueprints;
using Xunit;
using System.Collections.Generic;

namespace PulsePanel.Tests;

public class TokenEngineTests
{
    private readonly Dictionary<string, object> _testValues = new()
    {
        { "motd", "Hello World" },
        { "server", new Dictionary<string, object>
            {
                { "name", "My Test Server" },
                { "port", 8080 },
                { "whitelist", true }
            }
        }
    };

    [Fact]
    public void SimpleToken_Should_Be_Replaced()
    {
        var replacer = new TokenReplacer(_testValues);
        var result = replacer.Replace("Welcome to {{motd}}");
        Assert.Equal("Welcome to Hello World", result);
    }

    [Fact]
    public void NestedToken_Should_Be_Replaced()
    {
        var replacer = new TokenReplacer(_testValues);
        var result = replacer.Replace("Server name is {{server.name}}");
        Assert.Equal("Server name is My Test Server", result);
    }

    [Fact]
    public void DefaultFilter_Should_Work_When_Token_Is_Missing()
    {
        var replacer = new TokenReplacer(_testValues);
        var result = replacer.Replace("Game port is {{game.port | default: 7777}}");
        Assert.Equal("Game port is 7777", result);
    }

    [Fact]
    public void DefaultFilter_Should_Not_Be_Used_When_Token_Exists()
    {
        var replacer = new TokenReplacer(_testValues);
        var result = replacer.Replace("Server port is {{server.port | default: 25565}}");
        Assert.Equal("Server port is 8080", result);
    }

    [Fact]
    public void IfBlock_Should_Render_When_Condition_Is_True()
    {
        var replacer = new TokenReplacer(_testValues);
        var result = replacer.Replace("Whitelist is {{#if server.whitelist}}enabled{{/if}}");
        Assert.Equal("Whitelist is enabled", result);
    }

    [Fact]
    public void IfBlock_Should_Not_Render_When_Condition_Is_False()
    {
        var replacer = new TokenReplacer(_testValues);
        var result = replacer.Replace("Creative mode is {{#if server.creative}}enabled{{/if}}");
        Assert.Equal("Creative mode is ", result);
    }
}
