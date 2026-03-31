using System.CommandLine;
using PortainerLogs.Config;
using PortainerLogs.Handlers.Stacks;

namespace PortainerLogs.Commands.Stacks;

public class GetStack : AbstractCommand
{
    protected override string Name => "get";
    protected override string Description => "Show the compose file for a stack";

    public static readonly Argument<string> StackName = new("stack-name")
    {
        Description = "Name of the stack"
    };

    protected override Command GenerateCommand(ConfigStore configStore)
    {
        var command = base.GenerateCommand(configStore);
        command.Add(StackName);
        CommonOptions.AddDataOptions(command);
        CommonOptions.AddResolverOptions(command);
        command.SetAction((parseResult, _) => new GetStackHandler(configStore, parseResult).InvokeAsync());
        return command;
    }
}
