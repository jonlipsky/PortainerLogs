using System.CommandLine;
using System.CommandLine.Parsing;
using PortainerLogs.Client;
using PortainerLogs.Commands.Containers;
using PortainerLogs.Config;
using PortainerLogs.Formatting;

namespace PortainerLogs.Handlers.Containers;

public class ListContainersHandler(ConfigStore configStore, ParseResult parseResult)
    : AbstractDataHandler(configStore, parseResult)
{
    public override async Task<int> InvokeAsync()
    {
        try
        {
            var all = GetOption(ListContainers.All);
            var format = ResolveFormat();
            var client = CreateClient();
            var resolvedEnvId = await ResolveEnvId(client);
            var containers = await client.GetContainersAsync(resolvedEnvId, all);

            if (format == "json")
            {
                var output = containers.Select(c => new
                {
                    id = c.Id,
                    name = c.DisplayName,
                    status = c.State,
                    image = c.Image,
                    createdAt = DateTimeOffset.FromUnixTimeSeconds(c.Created).ToString("o")
                });
                Console.WriteLine(JsonFormatter.Format(output));
            }
            else
            {
                Console.WriteLine(PlainFormatter.FormatContainers(containers));
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
