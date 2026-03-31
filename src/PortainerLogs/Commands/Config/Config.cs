using System.CommandLine;
using PortainerLogs.Config;

namespace PortainerLogs.Commands.Config;

public class Config : AbstractCommand
{
    protected override string Name => "config";
    protected override string Description => "Manage global tool settings";

    protected override Command GenerateCommand(ConfigStore configStore)
    {
        var command = base.GenerateCommand(configStore);
        command.Add(new SetConfig().Command(configStore));
        command.Add(new ListConfig().Command(configStore));
        return command;
    }
}
