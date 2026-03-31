using System.Text.Json.Serialization;

namespace PortainerLogs.Client;

// GET /api/status
public record PortainerStatus
{
    [JsonPropertyName("Version")]
    public string Version { get; init; } = string.Empty;
}

// GET /api/endpoints
public record PortainerEndpoint
{
    [JsonPropertyName("Id")]
    public int Id { get; init; }

    [JsonPropertyName("Name")]
    public string Name { get; init; } = string.Empty;

    [JsonPropertyName("URL")]
    public string Url { get; init; } = string.Empty;
}

// GET /api/endpoints/{envId}/docker/containers/json
public record DockerContainer
{
    [JsonPropertyName("Id")]
    public string Id { get; init; } = string.Empty;

    [JsonPropertyName("Names")]
    public List<string> Names { get; init; } = [];

    [JsonPropertyName("Image")]
    public string Image { get; init; } = string.Empty;

    [JsonPropertyName("State")]
    public string State { get; init; } = string.Empty;

    [JsonPropertyName("Status")]
    public string Status { get; init; } = string.Empty;

    [JsonPropertyName("Created")]
    public long Created { get; init; }

    public string DisplayName => Names.Count > 0
        ? Names[0].TrimStart('/')
        : Id[..12];
}

// GET /api/stacks
public record PortainerStack
{
    [JsonPropertyName("Id")]
    public int Id { get; init; }

    [JsonPropertyName("Name")]
    public string Name { get; init; } = string.Empty;

    [JsonPropertyName("Status")]
    public int Status { get; init; }

    [JsonPropertyName("EndpointId")]
    public int EndpointId { get; init; }

    [JsonPropertyName("UpdateDate")]
    public long UpdateDate { get; init; }

    [JsonPropertyName("Env")]
    public List<StackEnvVar> Env { get; init; } = [];

    public string StatusText => Status switch
    {
        1 => "active",
        2 => "inactive",
        _ => "unknown"
    };
}

public record StackEnvVar
{
    [JsonPropertyName("name")]
    public string Name { get; init; } = string.Empty;

    [JsonPropertyName("value")]
    public string Value { get; init; } = string.Empty;
}

// GET /api/stacks/{id} — includes StackFileContent
public record PortainerStackDetail : PortainerStack
{
    [JsonPropertyName("StackFileContent")]
    public string StackFileContent { get; init; } = string.Empty;
}

// GET /api/endpoints/{envId}/docker/containers/{id}/json
// Full Docker inspect payload — we use JsonElement for passthrough in JSON mode
// and extract specific fields for plain mode
public record DockerInspectResponse
{
    [JsonPropertyName("Id")]
    public string Id { get; init; } = string.Empty;

    [JsonPropertyName("Name")]
    public string Name { get; init; } = string.Empty;

    [JsonPropertyName("Config")]
    public InspectConfig? Config { get; init; }

    [JsonPropertyName("HostConfig")]
    public InspectHostConfig? HostConfig { get; init; }

    [JsonPropertyName("NetworkSettings")]
    public InspectNetworkSettings? NetworkSettings { get; init; }

    [JsonPropertyName("State")]
    public InspectState? State { get; init; }

    [JsonPropertyName("RestartCount")]
    public int RestartCount { get; init; }

    [JsonPropertyName("Mounts")]
    public List<InspectMount> Mounts { get; init; } = [];
}

public record InspectConfig
{
    [JsonPropertyName("Image")]
    public string Image { get; init; } = string.Empty;

    [JsonPropertyName("Entrypoint")]
    public List<string>? Entrypoint { get; init; }

    [JsonPropertyName("Cmd")]
    public List<string>? Cmd { get; init; }

    [JsonPropertyName("Env")]
    public List<string> Env { get; init; } = [];

    [JsonPropertyName("ExposedPorts")]
    public Dictionary<string, object>? ExposedPorts { get; init; }

    [JsonPropertyName("Healthcheck")]
    public InspectHealthcheck? Healthcheck { get; init; }
}

public record InspectHealthcheck
{
    [JsonPropertyName("Test")]
    public List<string> Test { get; init; } = [];

    [JsonPropertyName("Interval")]
    public long Interval { get; init; }

    [JsonPropertyName("Timeout")]
    public long Timeout { get; init; }

    [JsonPropertyName("Retries")]
    public int Retries { get; init; }
}

public record InspectHostConfig
{
    [JsonPropertyName("RestartPolicy")]
    public RestartPolicy? RestartPolicy { get; init; }

    [JsonPropertyName("PortBindings")]
    public Dictionary<string, List<PortBinding>>? PortBindings { get; init; }
}

public record RestartPolicy
{
    [JsonPropertyName("Name")]
    public string Name { get; init; } = string.Empty;

    [JsonPropertyName("MaximumRetryCount")]
    public int MaximumRetryCount { get; init; }
}

public record PortBinding
{
    [JsonPropertyName("HostIp")]
    public string HostIp { get; init; } = string.Empty;

    [JsonPropertyName("HostPort")]
    public string HostPort { get; init; } = string.Empty;
}

public record InspectNetworkSettings
{
    [JsonPropertyName("Networks")]
    public Dictionary<string, NetworkEntry>? Networks { get; init; }
}

public record NetworkEntry
{
    [JsonPropertyName("IPAddress")]
    public string IpAddress { get; init; } = string.Empty;

    [JsonPropertyName("Gateway")]
    public string Gateway { get; init; } = string.Empty;

    [JsonPropertyName("MacAddress")]
    public string MacAddress { get; init; } = string.Empty;
}

public record InspectState
{
    [JsonPropertyName("Status")]
    public string Status { get; init; } = string.Empty;

    [JsonPropertyName("Health")]
    public HealthState? Health { get; init; }
}

public record HealthState
{
    [JsonPropertyName("Status")]
    public string Status { get; init; } = string.Empty;

    [JsonPropertyName("Log")]
    public List<HealthLog> Log { get; init; } = [];
}

public record HealthLog
{
    [JsonPropertyName("Start")]
    public string Start { get; init; } = string.Empty;

    [JsonPropertyName("End")]
    public string End { get; init; } = string.Empty;

    [JsonPropertyName("ExitCode")]
    public int ExitCode { get; init; }

    [JsonPropertyName("Output")]
    public string Output { get; init; } = string.Empty;
}

public record InspectMount
{
    [JsonPropertyName("Type")]
    public string Type { get; init; } = string.Empty;

    [JsonPropertyName("Source")]
    public string Source { get; init; } = string.Empty;

    [JsonPropertyName("Destination")]
    public string Destination { get; init; } = string.Empty;

    [JsonPropertyName("RW")]
    public bool ReadWrite { get; init; }
}

// GET /api/endpoints/{envId}/docker/containers/{id}/stats?stream=false
public record DockerStats
{
    [JsonPropertyName("cpu_stats")]
    public CpuStats? CpuStats { get; init; }

    [JsonPropertyName("precpu_stats")]
    public CpuStats? PreCpuStats { get; init; }

    [JsonPropertyName("memory_stats")]
    public MemoryStats? MemoryStats { get; init; }

    [JsonPropertyName("networks")]
    public Dictionary<string, NetworkStats>? Networks { get; init; }
}

public record CpuStats
{
    [JsonPropertyName("cpu_usage")]
    public CpuUsage? CpuUsage { get; init; }

    [JsonPropertyName("system_cpu_usage")]
    public long SystemCpuUsage { get; init; }

    [JsonPropertyName("online_cpus")]
    public int OnlineCpus { get; init; }
}

public record CpuUsage
{
    [JsonPropertyName("total_usage")]
    public long TotalUsage { get; init; }
}

public record MemoryStats
{
    [JsonPropertyName("usage")]
    public long Usage { get; init; }

    [JsonPropertyName("limit")]
    public long Limit { get; init; }
}

public record NetworkStats
{
    [JsonPropertyName("rx_bytes")]
    public long RxBytes { get; init; }

    [JsonPropertyName("tx_bytes")]
    public long TxBytes { get; init; }
}

// GET /api/endpoints/{envId}/docker/events
public record DockerEvent
{
    [JsonPropertyName("Type")]
    public string Type { get; init; } = string.Empty;

    [JsonPropertyName("Action")]
    public string Action { get; init; } = string.Empty;

    [JsonPropertyName("Actor")]
    public EventActor? Actor { get; init; }

    [JsonPropertyName("time")]
    public long Time { get; init; }

    [JsonPropertyName("timeNano")]
    public long TimeNano { get; init; }
}

public record EventActor
{
    [JsonPropertyName("ID")]
    public string Id { get; init; } = string.Empty;

    [JsonPropertyName("Attributes")]
    public Dictionary<string, string> Attributes { get; init; } = new();
}
