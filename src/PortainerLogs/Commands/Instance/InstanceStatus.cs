using System.CommandLine;
using PortainerLogs.Config;
using PortainerLogs.Handlers.Instance;

namespace PortainerLogs.Commands.Instance;

public class InstanceStatus : AbstractCommand
{
    protected override string Name => "status";
    protected override string Description => "Check reachability of configured instances";

    public static readonly Option<string?> InstanceOpt = new("--instance") { Description = "Check only the named instance" };
    public static readonly Option<string> FormatOpt = new("--format") { Description = "Output format: plain or json", DefaultValueFactory = _ => "plain" };

    protected override Command GenerateCommand(ConfigStore configStore)
    {
        var command = base.GenerateCommand(configStore);
        command.Add(InstanceOpt);
        command.Add(FormatOpt);
        command.SetAction((parseResult, _) => new InstanceStatusHandler(configStore, parseResult).InvokeAsync());
        return command;
    }
}
