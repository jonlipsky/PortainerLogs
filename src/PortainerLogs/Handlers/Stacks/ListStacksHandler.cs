using System.CommandLine;
using System.CommandLine.Parsing;
using PortainerLogs.Client;
using PortainerLogs.Commands;
using PortainerLogs.Config;
using PortainerLogs.Formatting;

namespace PortainerLogs.Handlers.Stacks;

public class ListStacksHandler(ConfigStore configStore, ParseResult parseResult)
    : AbstractDataHandler(configStore, parseResult)
{
    public override async Task<int> InvokeAsync()
    {
        try
        {
            var format = ResolveFormat();
            var client = CreateClient();
            var envId = GetOption(CommonOptions.Env);
            var stacks = await client.GetStacksAsync(envId);

            // Fetch containers to compute per-stack running/total counts
            var containerCounts = new Dictionary<int, (int running, int total)>();
            var envIds = stacks.Select(s => s.EndpointId).Distinct();
            foreach (var eid in envIds)
            {
                try
                {
                    var containers = await client.GetContainersAsync(eid, all: true);
                    foreach (var stack in stacks.Where(s => s.EndpointId == eid))
                    {
                        var stackContainers = containers.Where(c =>
                            c.DisplayName.StartsWith(stack.Name + "-", StringComparison.OrdinalIgnoreCase) ||
                            c.DisplayName.StartsWith(stack.Name + "_", StringComparison.OrdinalIgnoreCase));
                        var total = stackContainers.Count();
                        var running = stackContainers.Count(c => c.State == "running");
                        containerCounts[stack.Id] = (running, total);
                    }
                }
                catch (HttpRequestException)
                {
                    // If we can't fetch containers for an env, skip counts
                }
            }

            if (format == "json")
            {
                var output = stacks.Select(s =>
                {
                    containerCounts.TryGetValue(s.Id, out var counts);
                    return new
                    {
                        id = s.Id,
                        name = s.Name,
                        status = counts.total > 0 && counts.running < counts.total && counts.running > 0
                            ? "partially-active" : s.StatusText,
                        environmentId = s.EndpointId,
                        containerCount = counts.total,
                        runningCount = counts.running,
                        updatedAt = s.UpdateDate > 0
                            ? DateTimeOffset.FromUnixTimeSeconds(s.UpdateDate).ToString("o")
                            : null,
                    };
                });
                Console.WriteLine(JsonFormatter.Format(output));
            }
            else
            {
                Console.WriteLine(PlainFormatter.FormatStacks(stacks, stackId =>
                    containerCounts.TryGetValue(stackId, out var c) ? c : null));
            }

            Environment.ExitCode = 0;
            return 0;
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
}
