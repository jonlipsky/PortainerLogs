using System.CommandLine;
using PortainerLogs.Config;
using PortainerLogs.Handlers.Containers;

namespace PortainerLogs.Commands.Containers;

public class ListContainers : AbstractCommand
{
    protected override string Name => "containers";
    protected override string Description => "List containers across an environment";

    public static readonly Option<bool> All = new("--all")
    {
        Description = "Include stopped containers (default: running only)"
    };

    protected override Command GenerateCommand(ConfigStore configStore)
    {
        var command = base.GenerateCommand(configStore);
        CommonOptions.AddDataOptions(command);
        command.Add(All);
        command.SetAction((parseResult, _) => new ListContainersHandler(configStore, parseResult).InvokeAsync());
        return command;
    }
}
