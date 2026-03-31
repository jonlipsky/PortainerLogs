using System.CommandLine;
using PortainerLogs.Config;
using PortainerLogs.Handlers.Inspect;

namespace PortainerLogs.Commands.Inspect;

public class InspectContainer : AbstractCommand
{
    protected override string Name => "inspect";
    protected override string Description => "Show full container configuration detail";

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
        command.SetAction((parseResult, _) => new InspectContainerHandler(configStore, parseResult).InvokeAsync());
        return command;
    }
}
