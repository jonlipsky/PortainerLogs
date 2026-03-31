using System.Text.Json;
using System.Text.Json.Serialization;

namespace PortainerLogs.Formatting;

public static class JsonFormatter
{
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public static string Format<T>(T data) =>
        JsonSerializer.Serialize(data, Options);

    public static string FormatRaw(JsonElement element) =>
        JsonSerializer.Serialize(element, Options);
}
