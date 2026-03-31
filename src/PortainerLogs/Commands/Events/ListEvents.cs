using System.CommandLine;
using PortainerLogs.Config;
using PortainerLogs.Handlers.Events;

namespace PortainerLogs.Commands.Events;

public class ListEvents : AbstractCommand
{
    protected override string Name => "events";
    protected override string Description => "Show recent Docker events";

    public static readonly Option<string?> Since = new("--since")
    {
        Description = "Time window (default: 1h)"
    };

    public static readonly Option<string?> Container = new("--container")
    {
        Description = "Filter to a specific container"
    };

    public static readonly Option<string?> Type = new("--type")
    {
        Description = "Filter by event type: container, network, volume, image"
    };

    protected override Command GenerateCommand(ConfigStore configStore)
    {
        var command = base.GenerateCommand(configStore);
        command.Add(Since);
        command.Add(Container);
        command.Add(Type);
        CommonOptions.AddDataOptions(command);
        CommonOptions.AddResolverOptions(command);
        command.SetAction((parseResult, _) => new ListEventsHandler(configStore, parseResult).InvokeAsync());
        return command;
    }
}
