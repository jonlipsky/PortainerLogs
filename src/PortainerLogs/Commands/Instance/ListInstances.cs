using System.CommandLine;
using PortainerLogs.Config;
using PortainerLogs.Handlers.Instance;

namespace PortainerLogs.Commands.Instance;

public class ListInstances : AbstractCommand
{
    protected override string Name => "list";
    protected override string Description => "List all configured instances";

    public static readonly Option<string> FormatOpt = new("--format") { Description = "Output format: plain or json", DefaultValueFactory = _ => "plain" };

    protected override Command GenerateCommand(ConfigStore configStore)
    {
        var command = base.GenerateCommand(configStore);
        command.Add(FormatOpt);
        command.SetAction((parseResult, _) => new ListInstancesHandler(configStore, parseResult).InvokeAsync());
        return command;
    }
}
