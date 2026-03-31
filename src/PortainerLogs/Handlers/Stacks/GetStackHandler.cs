using System.CommandLine;
using System.CommandLine.Parsing;
using PortainerLogs.Client;
using PortainerLogs.Commands;
using PortainerLogs.Commands.Stacks;
using PortainerLogs.Config;
using PortainerLogs.Formatting;
using PortainerLogs.Resolvers;

namespace PortainerLogs.Handlers.Stacks;

public class GetStackHandler(ConfigStore configStore, ParseResult parseResult)
    : AbstractDataHandler(configStore, parseResult)
{
    public override async Task<int> InvokeAsync()
    {
        try
        {
            var name = GetArgument(GetStack.StackName);
            var format = ResolveFormat();
            var client = CreateClient();
            var fuzzyEnabled = ResolveFuzzy();
            var envId = GetOption(CommonOptions.Env);

            var stacks = await client.GetStacksAsync(envId);
            var result = StackResolver.Resolve(stacks, name, fuzzyEnabled);

            if (result.WasFuzzy)
                Console.Error.WriteLine($"Resolved to stack: {result.ResolvedName}");

            var detail = await client.GetStackAsync(result.Value.Id);

            if (format == "json")
            {
                Console.WriteLine(JsonFormatter.Format(new { stackFileContent = detail.StackFileContent }));
            }
            else
            {
                Console.WriteLine(detail.StackFileContent);
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
            Console.Error.WriteLine("Known stacks: " + string.Join(", ", ex.KnownNames));
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
