using System.CommandLine;
using PortainerLogs.Config;
using PortainerLogs.Handlers.Instance;

namespace PortainerLogs.Commands.Instance;

public class RemoveInstance : AbstractCommand
{
    protected override string Name => "remove";
    protected override string Description => "Remove a configured instance";

    public static readonly Argument<string> Key = new("key") { Description = "Key of the instance to remove" };

    protected override Command GenerateCommand(ConfigStore configStore)
    {
        var command = base.GenerateCommand(configStore);
        command.Add(Key);
        command.SetAction((parseResult, _) => new RemoveInstanceHandler(configStore, parseResult).InvokeAsync());
        return command;
    }
}
