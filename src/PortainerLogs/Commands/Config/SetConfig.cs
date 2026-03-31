using System.CommandLine;
using PortainerLogs.Config;
using PortainerLogs.Handlers.Config;

namespace PortainerLogs.Commands.Config;

public class SetConfig : AbstractCommand
{
    protected override string Name => "set";
    protected override string Description => "Set a global option";

    public static readonly Argument<string> Key = new("key") { Description = "Setting key (fuzzy-match, default-tail, default-format)" };
    public static readonly Argument<string> Value = new("value") { Description = "Setting value" };

    protected override Command GenerateCommand(ConfigStore configStore)
    {
        var command = base.GenerateCommand(configStore);
        command.Add(Key);
        command.Add(Value);
        command.SetAction((parseResult, _) => new SetConfigHandler(configStore, parseResult).InvokeAsync());
        return command;
    }
}
