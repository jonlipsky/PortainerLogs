using System.Text;
using PortainerLogs.Client;
using PortainerLogs.Config;

namespace PortainerLogs.Formatting;

public static class PlainFormatter
{
    private static bool UseColor => !Console.IsOutputRedirected;

    public static string FormatInstanceList(ToolConfig config)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"{"KEY",-12}{"URL",-34}{"DEFAULT"}");
        sb.AppendLine(new string('\u2500', 56));

        foreach (var (key, instance) in config.Instances)
        {
            var isDefault = key == config.Default ? "*" : "";
            sb.AppendLine($"{key,-12}{instance.Url,-34}{isDefault}");
        }

        return sb.ToString().TrimEnd();
    }

    public static string FormatInstanceStatus(string key, string url, bool reachable, string? version)
    {
        var status = reachable ? "OK" : "UNREACHABLE";
        var ver = version ?? "";
        return $"{key,-12}{url,-34}{status,-13}{ver}";
    }

    public static string FormatInstanceStatusHeader()
    {
        var sb = new StringBuilder();
        sb.AppendLine($"{"KEY",-12}{"URL",-34}{"STATUS",-13}{"VERSION"}");
        sb.AppendLine(new string('\u2500', 65));
        return sb.ToString();
    }

    public static string FormatSettingsList(ToolSettings settings)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"{"SETTING",-17}{"VALUE"}");
        sb.AppendLine(new string('\u2500', 30));
        sb.AppendLine($"{"fuzzy-match",-17}{settings.FuzzyMatch.ToString().ToLowerInvariant()}");
        sb.AppendLine($"{"default-tail",-17}{settings.DefaultTail}");
        sb.AppendLine($"{"default-format",-17}{settings.DefaultFormat}");
        return sb.ToString().TrimEnd();
    }

    public static string FormatContainers(IReadOnlyList<DockerContainer> containers)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"{"NAME",-25}{"STATUS",-11}{"IMAGE",-34}{"CREATED"}");
        sb.AppendLine(new string('\u2500', 77));

        foreach (var c in containers)
        {
            var created = FormatTimestamp(c.Created);
            sb.AppendLine($"{c.DisplayName,-25}{c.State,-11}{c.Image,-34}{created}");
        }

        return sb.ToString().TrimEnd();
    }

    public static string FormatStacks(IReadOnlyList<PortainerStack> stacks, Func<int, (int running, int total)?>? containerCounts = null)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"{"NAME",-17}{"STATUS",-18}{"ENV",-7}{"CONTAINERS",-13}{"UPDATED"}");
        sb.AppendLine(new string('\u2500', 69));

        foreach (var s in stacks)
        {
            var counts = containerCounts?.Invoke(s.Id);
            var containerStr = counts.HasValue ? $"{counts.Value.running} / {counts.Value.total}" : "";
            var statusText = counts.HasValue && counts.Value.total > 0 && counts.Value.running < counts.Value.total && counts.Value.running > 0
                ? "partially-active" : s.StatusText;
            var updatedStr = s.UpdateDate > 0 ? FormatTimestamp(s.UpdateDate) : "";
            sb.AppendLine($"{s.Name,-17}{statusText,-18}{s.EndpointId,-7}{containerStr,-13}{updatedStr}");
        }

        return sb.ToString().TrimEnd();
    }

    public static string FormatInspect(DockerInspectResponse inspect)
    {
        var sb = new StringBuilder();

        // Image, entrypoint, command
        sb.AppendLine("IMAGE & COMMAND");
        sb.AppendLine(new string('\u2500', 40));
        if (inspect.Config != null)
        {
            sb.AppendLine($"  Image:       {inspect.Config.Image}");
            if (inspect.Config.Entrypoint != null)
                sb.AppendLine($"  Entrypoint:  {string.Join(" ", inspect.Config.Entrypoint)}");
            if (inspect.Config.Cmd != null)
                sb.AppendLine($"  Command:     {string.Join(" ", inspect.Config.Cmd)}");
        }
        sb.AppendLine();

        // Environment variables
        if (inspect.Config?.Env.Count > 0)
        {
            sb.AppendLine("ENVIRONMENT VARIABLES");
            sb.AppendLine(new string('\u2500', 40));
            foreach (var env in inspect.Config.Env)
                sb.AppendLine($"  {env}");
            sb.AppendLine();
        }

        // Port bindings
        if (inspect.HostConfig?.PortBindings?.Count > 0)
        {
            sb.AppendLine("PORT BINDINGS");
            sb.AppendLine(new string('\u2500', 40));
            foreach (var (containerPort, bindings) in inspect.HostConfig.PortBindings)
            {
                foreach (var b in bindings)
                    sb.AppendLine($"  {b.HostIp}:{b.HostPort} \u2192 {containerPort}");
            }
            sb.AppendLine();
        }

        // Mounts
        if (inspect.Mounts.Count > 0)
        {
            sb.AppendLine("MOUNTS");
            sb.AppendLine(new string('\u2500', 40));
            foreach (var m in inspect.Mounts)
                sb.AppendLine($"  [{m.Type}] {m.Source} \u2192 {m.Destination} ({(m.ReadWrite ? "rw" : "ro")})");
            sb.AppendLine();
        }

        // Restart policy
        if (inspect.HostConfig?.RestartPolicy != null)
        {
            sb.AppendLine("RESTART POLICY");
            sb.AppendLine(new string('\u2500', 40));
            sb.AppendLine($"  Policy:        {inspect.HostConfig.RestartPolicy.Name}");
            sb.AppendLine($"  Max retries:   {inspect.HostConfig.RestartPolicy.MaximumRetryCount}");
            sb.AppendLine($"  Restart count: {inspect.RestartCount}");
            sb.AppendLine();
        }

        // Health check
        if (inspect.Config?.Healthcheck != null)
        {
            sb.AppendLine("HEALTH CHECK");
            sb.AppendLine(new string('\u2500', 40));
            sb.AppendLine($"  Test:     {string.Join(" ", inspect.Config.Healthcheck.Test)}");
            sb.AppendLine($"  Interval: {TimeSpan.FromTicks(inspect.Config.Healthcheck.Interval / 100)}");
            sb.AppendLine($"  Timeout:  {TimeSpan.FromTicks(inspect.Config.Healthcheck.Timeout / 100)}");
            sb.AppendLine($"  Retries:  {inspect.Config.Healthcheck.Retries}");
        }

        if (inspect.State?.Health != null)
        {
            sb.AppendLine($"  Status:   {inspect.State.Health.Status}");
            if (inspect.State.Health.Log.Count > 0)
            {
                sb.AppendLine("  Recent results:");
                foreach (var log in inspect.State.Health.Log.TakeLast(3))
                    sb.AppendLine($"    [{log.Start}] exit={log.ExitCode} {log.Output.Trim()}");
            }
            sb.AppendLine();
        }

        // Network settings
        if (inspect.NetworkSettings?.Networks?.Count > 0)
        {
            sb.AppendLine("NETWORKS");
            sb.AppendLine(new string('\u2500', 40));
            foreach (var (name, net) in inspect.NetworkSettings.Networks)
                sb.AppendLine($"  {name}: IP={net.IpAddress} GW={net.Gateway}");
        }

        return sb.ToString().TrimEnd();
    }

    public static string FormatStats(string containerName, DockerStats stats)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"{"CONTAINER",-19}{"CPU %",-9}{"MEM USAGE / LIMIT",-24}{"MEM %",-9}{"NET I/O"}");
        sb.AppendLine(new string('\u2500', 75));

        var cpuPercent = CalculateCpuPercent(stats);
        var memUsage = stats.MemoryStats?.Usage ?? 0;
        var memLimit = stats.MemoryStats?.Limit ?? 0;
        var memPercent = memLimit > 0 ? (double)memUsage / memLimit * 100 : 0;

        var (netRx, netTx) = CalculateNetworkBytes(stats);

        sb.AppendLine($"{containerName,-19}{cpuPercent,5:F1}%   {FormatBytes(memUsage)} / {FormatBytes(memLimit),-12}{memPercent,5:F1}%   {FormatBytes(netRx)} / {FormatBytes(netTx)}");

        return sb.ToString().TrimEnd();
    }

    public static string FormatEvents(IReadOnlyList<DockerEvent> events)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"{"TIMESTAMP",-22}{"TYPE",-12}{"ACTION",-11}{"NAME",-21}{"DETAIL"}");
        sb.AppendLine(new string('\u2500', 85));

        foreach (var e in events)
        {
            var ts = DateTimeOffset.FromUnixTimeSeconds(e.Time).ToString("yyyy-MM-ddTHH:mm:ssZ");
            var name = e.Actor?.Attributes.GetValueOrDefault("name", "") ?? "";
            var detail = "";
            if (e.Actor?.Attributes.TryGetValue("exitCode", out var exitCode) == true)
                detail = $"exitCode={exitCode}";

            sb.AppendLine($"{ts,-22}{e.Type,-12}{e.Action,-11}{name,-21}{detail}");
        }

        return sb.ToString().TrimEnd();
    }

    private static double CalculateCpuPercent(DockerStats stats)
    {
        if (stats.CpuStats?.CpuUsage == null || stats.PreCpuStats?.CpuUsage == null)
            return 0;

        var cpuDelta = stats.CpuStats.CpuUsage.TotalUsage - stats.PreCpuStats.CpuUsage.TotalUsage;
        var systemDelta = stats.CpuStats.SystemCpuUsage - stats.PreCpuStats.SystemCpuUsage;

        if (systemDelta <= 0 || cpuDelta < 0)
            return 0;

        return (double)cpuDelta / systemDelta * stats.CpuStats.OnlineCpus * 100;
    }

    private static (long rx, long tx) CalculateNetworkBytes(DockerStats stats)
    {
        if (stats.Networks == null) return (0, 0);

        long rx = 0, tx = 0;
        foreach (var net in stats.Networks.Values)
        {
            rx += net.RxBytes;
            tx += net.TxBytes;
        }
        return (rx, tx);
    }

    private static string FormatBytes(long bytes)
    {
        return bytes switch
        {
            >= 1L << 30 => $"{bytes / (double)(1L << 30):F1} GiB",
            >= 1L << 20 => $"{bytes / (double)(1L << 20):F0} MiB",
            >= 1L << 10 => $"{bytes / (double)(1L << 10):F0} KiB",
            _ => $"{bytes} B"
        };
    }

    private static string FormatTimestamp(long unixSeconds)
    {
        var created = DateTimeOffset.FromUnixTimeSeconds(unixSeconds);
        var elapsed = DateTimeOffset.UtcNow - created;

        return elapsed.TotalMinutes switch
        {
            < 1 => "just now",
            < 60 => $"{(int)elapsed.TotalMinutes}m ago",
            < 1440 => $"{(int)elapsed.TotalHours}h ago",
            _ => $"{(int)elapsed.TotalDays}d ago"
        };
    }
}
