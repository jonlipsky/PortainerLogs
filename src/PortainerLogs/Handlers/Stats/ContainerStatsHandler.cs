using System.CommandLine;
using System.CommandLine.Parsing;
using PortainerLogs.Client;
using PortainerLogs.Commands.Stats;
using PortainerLogs.Config;
using PortainerLogs.Formatting;
using PortainerLogs.Resolvers;

namespace PortainerLogs.Handlers.Stats;

public class ContainerStatsHandler(ConfigStore configStore, ParseResult parseResult)
    : AbstractDataHandler(configStore, parseResult)
{
    public override async Task<int> InvokeAsync()
    {
        try
        {
            var containerName = GetArgument(ContainerStats.Container);
            var format = ResolveFormat();
            var client = CreateClient();
            var fuzzyEnabled = ResolveFuzzy();

            var resolvedEnvId = await ResolveEnvId(client);
            var containers = await client.GetContainersAsync(resolvedEnvId, all: true);
            var result = ContainerResolver.Resolve(containers, containerName, fuzzyEnabled);

            if (result.WasFuzzy)
                Console.Error.WriteLine($"Resolved to container: {result.ResolvedName}");

            var stats = await client.GetContainerStatsAsync(resolvedEnvId, result.Value.Id);

            if (format == "json")
            {
                var cpuPercent = CalculateCpuPercent(stats);
                var (netRx, netTx) = CalculateNetworkBytes(stats);
                Console.WriteLine(JsonFormatter.Format(new
                {
                    cpuPercent,
                    memoryUsageBytes = stats.MemoryStats?.Usage ?? 0,
                    memoryLimitBytes = stats.MemoryStats?.Limit ?? 0,
                    memoryPercent = stats.MemoryStats?.Limit > 0
                        ? (double)(stats.MemoryStats?.Usage ?? 0) / stats.MemoryStats!.Limit * 100 : 0,
                    networkRxBytes = netRx,
                    networkTxBytes = netTx
                }));
            }
            else
            {
                Console.WriteLine(PlainFormatter.FormatStats(result.ResolvedName, stats));
            }

            Environment.ExitCode = 0;
            return 0;
        }
        catch (AmbiguousMatchException ex)
        {
            Console.Error.WriteLine($"Error: {ex.Message}");
            Console.Error.WriteLine("Candidates: " + string.Join(", ", ex.Candidates));
            Environment.ExitCode = 3;
            return 3;
        }
        catch (NoMatchException ex)
        {
            Console.Error.WriteLine($"Error: {ex.Message}");
            Console.Error.WriteLine("Known containers: " + string.Join(", ", ex.KnownNames));
            Environment.ExitCode = 4;
            return 4;
        }
        catch (InvalidOperationException ex)
        {
            Console.Error.WriteLine($"Error: {ex.Message}");
            Environment.ExitCode = 1;
            return 1;
        }
        catch (HttpRequestException ex)
        {
            Console.Error.WriteLine($"Error: Could not connect — {ex.Message}");
            Environment.ExitCode = 2;
            return 2;
        }
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
}
