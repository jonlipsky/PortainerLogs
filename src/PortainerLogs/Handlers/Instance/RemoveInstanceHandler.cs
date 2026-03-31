using System.CommandLine;
using System.CommandLine.Parsing;
using PortainerLogs.Commands.Instance;
using PortainerLogs.Config;

namespace PortainerLogs.Handlers.Instance;

public class RemoveInstanceHandler : AbstractHandler
{
    public RemoveInstanceHandler(ConfigStore configStore, ParseResult parseResult)
        : base(configStore, parseResult) { }

    private string Key => GetArgument(RemoveInstance.Key)!;

    public override Task<int> InvokeAsync()
    {
        ConfigStore.RemoveInstance(Key);
        Console.WriteLine($"Instance '{Key}' removed.");

        var config = ConfigStore.Load();
        if (config.Default == null && config.Instances.Count > 0)
            Console.Error.WriteLine("Warning: No default instance set. Run 'instance set-default <key>' to set one.");

        return Task.FromResult(0);
    }
}
