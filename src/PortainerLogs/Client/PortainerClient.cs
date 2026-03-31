using System.Text.Json;

namespace PortainerLogs.Client;

/// <summary>
/// Compile-time GET-only HTTP wrapper. Only exposes GET methods,
/// making it impossible for PortainerClient to issue non-GET requests.
/// </summary>
public sealed class GetOnlyHttpClient
{
    private readonly HttpClient _httpClient;

    public GetOnlyHttpClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public Task<HttpResponseMessage> GetAsync(string path) =>
        _httpClient.GetAsync(path);
}

public class PortainerClient
{
    private readonly GetOnlyHttpClient _httpClient;

    public PortainerClient(string baseUrl, string token)
        : this(CreateGetOnlyClient(baseUrl, token))
    {
    }

    public PortainerClient(GetOnlyHttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    /// <summary>
    /// Convenience constructor for tests that provide a raw HttpClient.
    /// </summary>
    public PortainerClient(HttpClient httpClient)
        : this(new GetOnlyHttpClient(httpClient))
    {
    }

    private static GetOnlyHttpClient CreateGetOnlyClient(string baseUrl, string token)
    {
        var client = new HttpClient { BaseAddress = new Uri(baseUrl.TrimEnd('/') + "/") };
        client.DefaultRequestHeaders.Add("X-API-Key", token);
        return new GetOnlyHttpClient(client);
    }

    public async Task<T> GetAsync<T>(string path)
    {
        using var response = await _httpClient.GetAsync(path);
        response.EnsureSuccessStatusCode();
        var stream = await response.Content.ReadAsStreamAsync();
        return await JsonSerializer.DeserializeAsync<T>(stream)
            ?? throw new InvalidOperationException($"Failed to deserialize response from {path}");
    }

    public async Task<string> GetStringAsync(string path)
    {
        using var response = await _httpClient.GetAsync(path);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }

    public async Task<JsonElement> GetJsonAsync(string path)
    {
        using var response = await _httpClient.GetAsync(path);
        response.EnsureSuccessStatusCode();
        var stream = await response.Content.ReadAsStreamAsync();
        var doc = await JsonDocument.ParseAsync(stream);
        return doc.RootElement.Clone();
    }

    // API methods — all GET-only

    public Task<PortainerStatus> GetStatusAsync() =>
        GetAsync<PortainerStatus>("api/status");

    public Task<List<PortainerEndpoint>> GetEndpointsAsync() =>
        GetAsync<List<PortainerEndpoint>>("api/endpoints");

    public Task<List<DockerContainer>> GetContainersAsync(int envId, bool all = false) =>
        GetAsync<List<DockerContainer>>($"api/endpoints/{envId}/docker/containers/json?all={(all ? "1" : "0")}");

    public Task<List<PortainerStack>> GetStacksAsync(int? envId = null)
    {
        var path = envId.HasValue
            ? $"api/stacks?endpointId={envId.Value}"
            : "api/stacks";
        return GetAsync<List<PortainerStack>>(path);
    }

    public Task<PortainerStackDetail> GetStackAsync(int stackId) =>
        GetAsync<PortainerStackDetail>($"api/stacks/{stackId}");

    public Task<string> GetContainerLogsAsync(int envId, string containerId, int? tail = null, long? since = null, bool timestamps = true)
    {
        var query = $"stdout=true&stderr=true&timestamps={timestamps.ToString().ToLowerInvariant()}";
        if (tail.HasValue) query += $"&tail={tail.Value}";
        if (since.HasValue) query += $"&since={since.Value}";
        return GetStringAsync($"api/endpoints/{envId}/docker/containers/{containerId}/logs?{query}");
    }

    public Task<JsonElement> GetContainerInspectAsync(int envId, string containerId) =>
        GetJsonAsync($"api/endpoints/{envId}/docker/containers/{containerId}/json");

    public Task<DockerInspectResponse> GetContainerInspectTypedAsync(int envId, string containerId) =>
        GetAsync<DockerInspectResponse>($"api/endpoints/{envId}/docker/containers/{containerId}/json");

    public Task<DockerStats> GetContainerStatsAsync(int envId, string containerId) =>
        GetAsync<DockerStats>($"api/endpoints/{envId}/docker/containers/{containerId}/stats?stream=false");

    public Task<string> GetEventsAsync(int envId, long? since = null, string? filters = null)
    {
        var query = "";
        var parts = new List<string>();
        if (since.HasValue) parts.Add($"since={since.Value}");
        if (filters != null) parts.Add($"filters={Uri.EscapeDataString(filters)}");
        if (parts.Count > 0) query = "?" + string.Join("&", parts);
        return GetStringAsync($"api/endpoints/{envId}/docker/events{query}");
    }
}
