using System.CommandLine;
using PortainerLogs.Config;
using PortainerLogs.Handlers.Instance;

namespace PortainerLogs.Commands.Instance;

public class SetDefault : AbstractCommand
{
    protected override string Name => "set-default";
    protected override string Description => "Set the default instance";

    public static readonly Argument<string> Key = new("key") { Description = "Key of the instance to set as default" };

    protected override Command GenerateCommand(ConfigStore configStore)
    {
        var command = base.GenerateCommand(configStore);
        command.Add(Key);
        command.SetAction((parseResult, _) => new SetDefaultHandler(configStore, parseResult).InvokeAsync());
        return command;
    }
}
