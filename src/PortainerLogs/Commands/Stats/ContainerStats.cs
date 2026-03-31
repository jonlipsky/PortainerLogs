using System.CommandLine;
using PortainerLogs.Config;
using PortainerLogs.Handlers.Stats;

namespace PortainerLogs.Commands.Stats;

public class ContainerStats : AbstractCommand
{
    protected override string Name => "stats";
    protected override string Description => "Show live resource usage snapshot";

    public static readonly Argument<string> Container = new("container")
    {
        Description = "Container name or ID"
    };

    protected override Command GenerateCommand(ConfigStore configStore)
    {
        var command = base.GenerateCommand(configStore);
        command.Add(Container);
        CommonOptions.AddDataOptions(command);
        CommonOptions.AddResolverOptions(command);
        command.SetAction((parseResult, _) => new ContainerStatsHandler(configStore, parseResult).InvokeAsync());
        return command;
    }
}
