using System.Text.Json.Serialization;

namespace PortainerLogs.Config;

public record ToolConfig
{
    [JsonPropertyName("default")]
    public string? Default { get; set; }

    [JsonPropertyName("settings")]
    public ToolSettings Settings { get; set; } = new();

    [JsonPropertyName("instances")]
    public Dictionary<string, InstanceEntry> Instances { get; set; } = new();
}

public record ToolSettings
{
    [JsonPropertyName("fuzzyMatch")]
    public bool FuzzyMatch { get; set; } = true;

    [JsonPropertyName("defaultTail")]
    public int DefaultTail { get; set; } = 200;

    [JsonPropertyName("defaultFormat")]
    public string DefaultFormat { get; set; } = "plain";
}

public record InstanceEntry
{
    [JsonPropertyName("url")]
    public string Url { get; set; } = string.Empty;

    [JsonPropertyName("token")]
    public string Token { get; set; } = string.Empty;
}
