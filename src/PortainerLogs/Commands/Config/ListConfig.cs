using System.CommandLine;
using PortainerLogs.Config;
using PortainerLogs.Handlers.Config;

namespace PortainerLogs.Commands.Config;

public class ListConfig : AbstractCommand
{
    protected override string Name => "list";
    protected override string Description => "Display all current settings";

    public static readonly Option<string> FormatOpt = new("--format") { Description = "Output format: plain or json", DefaultValueFactory = _ => "plain" };

    protected override Command GenerateCommand(ConfigStore configStore)
    {
        var command = base.GenerateCommand(configStore);
        command.Add(FormatOpt);
        command.SetAction((parseResult, _) => new ListConfigHandler(configStore, parseResult).InvokeAsync());
        return command;
    }
}
