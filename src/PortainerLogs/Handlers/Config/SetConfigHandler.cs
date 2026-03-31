using System.CommandLine;
using System.CommandLine.Parsing;
using PortainerLogs.Commands.Config;
using PortainerLogs.Config;

namespace PortainerLogs.Handlers.Config;

public class SetConfigHandler : AbstractHandler
{
    public SetConfigHandler(ConfigStore configStore, ParseResult parseResult)
        : base(configStore, parseResult) { }

    private string Key => GetArgument(SetConfig.Key)!;
    private string Value => GetArgument(SetConfig.Value)!;

    public override Task<int> InvokeAsync()
    {
        try
        {
            ConfigStore.SetSetting(Key, Value);
            Console.WriteLine($"Setting '{Key}' set to '{Value}'.");
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
