using System.CommandLine;
using System.CommandLine.Parsing;
using PortainerLogs.Commands.Instance;
using PortainerLogs.Config;

namespace PortainerLogs.Handlers.Instance;

public class SetDefaultHandler : AbstractHandler
{
    public SetDefaultHandler(ConfigStore configStore, ParseResult parseResult)
        : base(configStore, parseResult) { }

    private string Key => GetArgument(SetDefault.Key)!;

    public override Task<int> InvokeAsync()
    {
        try
        {
            ConfigStore.SetDefault(Key);
            Console.WriteLine($"Default instance set to '{Key}'.");
        }
        catch (InvalidOperationException ex)
        {
            Console.Error.WriteLine($"Error: {ex.Message}");
            Environment.ExitCode = 1;
            return Task.FromResult(1);
        }

        return Task.FromResult(0);
    }
}
