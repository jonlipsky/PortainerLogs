using System.CommandLine;
using PortainerLogs.Config;
using PortainerLogs.Handlers.Logs;

namespace PortainerLogs.Commands.Logs;

public class GetLogs : AbstractCommand
{
    protected override string Name => "logs";
    protected override string Description => "Fetch container log output";

    public static readonly Argument<string> Container = new("container")
    {
        Description = "Container name or ID"
    };

    public static readonly Option<int?> Tail = new("--tail")
    {
        Description = "Lines from end (default: default-tail setting)"
    };

    public static readonly Option<string?> Since = new("--since")
    {
        Description = "Time window, e.g. 1h, 30m, 2h30m"
    };

    public static readonly Option<string?> Level = new("--level")
    {
        Description = "Filter to lines containing Error or Critical"
    };

    public static readonly Option<string?> Grep = new("--grep")
    {
        Description = "Filter to lines matching a substring or regex"
    };

    public static readonly Option<bool> NoTimestamps = new("--no-timestamps")
    {
        Description = "Suppress the timestamp prefix on each line"
    };

    protected override Command GenerateCommand(ConfigStore configStore)
    {
        var command = base.GenerateCommand(configStore);
        command.Add(Container);
        command.Add(Tail);
        command.Add(Since);
        command.Add(Level);
        command.Add(Grep);
        command.Add(NoTimestamps);
        CommonOptions.AddDataOptions(command);
        CommonOptions.AddResolverOptions(command);
        command.SetAction((parseResult, _) => new GetLogsHandler(configStore, parseResult).InvokeAsync());
        return command;
    }
}
