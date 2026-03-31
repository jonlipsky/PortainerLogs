using System.CommandLine;
using PortainerLogs.Config;

namespace PortainerLogs.Commands.Instance;

public class Instance : AbstractCommand
{
    protected override string Name => "instance";
    protected override string Description => "Manage configured Portainer servers";

    protected override Command GenerateCommand(ConfigStore configStore)
    {
        var command = base.GenerateCommand(configStore);
        command.Add(new AddInstance().Command(configStore));
        command.Add(new RemoveInstance().Command(configStore));
        command.Add(new SetDefault().Command(configStore));
        command.Add(new ListInstances().Command(configStore));
        command.Add(new InstanceStatus().Command(configStore));
        return command;
    }
}
