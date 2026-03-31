using System.CommandLine;
using System.CommandLine.Parsing;
using System.Text.Json;
using PortainerLogs.Client;
using PortainerLogs.Commands.Events;
using PortainerLogs.Config;
using PortainerLogs.Formatting;
using PortainerLogs.Resolvers;

namespace PortainerLogs.Handlers.Events;

public class ListEventsHandler(ConfigStore configStore, ParseResult parseResult)
    : AbstractDataHandler(configStore, parseResult)
{
    public override async Task<int> InvokeAsync()
    {
        try
        {
            var since = GetOption(ListEvents.Since) ?? "1h";
            var container = GetOption(ListEvents.Container);
            var type = GetOption(ListEvents.Type);

            var format = ResolveFormat();
            var client = CreateClient();
            var fuzzyEnabled = ResolveFuzzy();

            var resolvedEnvId = await ResolveEnvId(client);
            var sinceTimestamp = DurationParser.ToUnixTimestamp(since);

            // Build filters
            string? filters = null;
            var filterDict = new Dictionary<string, List<string>>();

            if (type != null)
                filterDict["type"] = [type];

            if (container != null)
            {
                var containers = await client.GetContainersAsync(resolvedEnvId, all: true);
                var result = ContainerResolver.Resolve(containers, container, fuzzyEnabled);

                if (result.WasFuzzy)
                    Console.Error.WriteLine($"Resolved to container: {result.ResolvedName}");

                filterDict["container"] = [result.ResolvedName];
            }

            if (filterDict.Count > 0)
                filters = JsonSerializer.Serialize(filterDict);

            var rawEvents = await client.GetEventsAsync(resolvedEnvId, sinceTimestamp, filters);

            var events = rawEvents
                .Split('\n', StringSplitOptions.RemoveEmptyEntries)
                .Select(line => JsonSerializer.Deserialize<DockerEvent>(line))
                .Where(e => e != null)
                .Cast<DockerEvent>()
                .ToList();

            if (format == "json")
            {
                var output = events.Select(e => new
                {
                    timestamp = DateTimeOffset.FromUnixTimeSeconds(e.Time).ToString("o"),
                    type = e.Type,
                    action = e.Action,
                    name = e.Actor?.Attributes.GetValueOrDefault("name", ""),
                    attributes = e.Actor?.Attributes ?? new Dictionary<string, string>()
                });
                Console.WriteLine(JsonFormatter.Format(output));
            }
            else
            {
                Console.WriteLine(PlainFormatter.FormatEvents(events));
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
        catch (FormatException ex)
        {
            Console.Error.WriteLine($"Error: {ex.Message}");
            Environment.ExitCode = 1;
            return 1;
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
