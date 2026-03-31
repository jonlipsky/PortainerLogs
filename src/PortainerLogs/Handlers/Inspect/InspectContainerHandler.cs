using System.CommandLine;
using System.CommandLine.Parsing;
using PortainerLogs.Client;
using PortainerLogs.Commands.Inspect;
using PortainerLogs.Config;
using PortainerLogs.Formatting;
using PortainerLogs.Resolvers;

namespace PortainerLogs.Handlers.Inspect;

public class InspectContainerHandler(ConfigStore configStore, ParseResult parseResult)
    : AbstractDataHandler(configStore, parseResult)
{
    public override async Task<int> InvokeAsync()
    {
        try
        {
            var containerName = GetArgument(InspectContainer.Container);
            var format = ResolveFormat();
            var client = CreateClient();
            var fuzzyEnabled = ResolveFuzzy();

            var resolvedEnvId = await ResolveEnvId(client);
            var containers = await client.GetContainersAsync(resolvedEnvId, all: true);
            var result = ContainerResolver.Resolve(containers, containerName, fuzzyEnabled);

            if (result.WasFuzzy)
                Console.Error.WriteLine($"Resolved to container: {result.ResolvedName}");

            if (format == "json")
            {
                var json = await client.GetContainerInspectAsync(resolvedEnvId, result.Value.Id);
                Console.WriteLine(JsonFormatter.FormatRaw(json));
            }
            else
            {
                var inspect = await client.GetContainerInspectTypedAsync(resolvedEnvId, result.Value.Id);
                Console.WriteLine(PlainFormatter.FormatInspect(inspect));
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
}
