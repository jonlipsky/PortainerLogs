using System.CommandLine;
using PortainerLogs.Config;

namespace PortainerLogs.Commands.Stacks;

public class Stacks : AbstractCommand
{
    protected override string Name => "stacks";
    protected override string Description => "List and inspect deployed stacks";

    protected override Command GenerateCommand(ConfigStore configStore)
    {
        var command = base.GenerateCommand(configStore);
        command.Add(new ListStacks().Command(configStore));
        command.Add(new GetStack().Command(configStore));
        return command;
    }
}
