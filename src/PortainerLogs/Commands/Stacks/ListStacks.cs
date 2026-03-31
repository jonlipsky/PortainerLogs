using System.CommandLine;
using PortainerLogs.Config;
using PortainerLogs.Handlers.Stacks;

namespace PortainerLogs.Commands.Stacks;

public class ListStacks : AbstractCommand
{
    protected override string Name => "list";
    protected override string Description => "List all stacks";

    protected override Command GenerateCommand(ConfigStore configStore)
    {
        var command = base.GenerateCommand(configStore);
        CommonOptions.AddDataOptions(command);
        command.SetAction((parseResult, _) => new ListStacksHandler(configStore, parseResult).InvokeAsync());
        return command;
    }
}
