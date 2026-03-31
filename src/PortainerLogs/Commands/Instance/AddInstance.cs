using System.CommandLine;
using PortainerLogs.Config;
using PortainerLogs.Handlers.Instance;

namespace PortainerLogs.Commands.Instance;

public class AddInstance : AbstractCommand
{
    protected override string Name => "add";
    protected override string Description => "Add a new Portainer instance";

    public static readonly Argument<string> Key = new("key") { Description = "Short key for this instance" };
    public static readonly Option<string> Url = new("--url") { Description = "Base URL of the Portainer instance", Required = true };
    public static readonly Option<string> Token = new("--token") { Description = "Portainer API access token", Required = true };
    public static readonly Option<bool> SetDefaultOpt = new("--set-default") { Description = "Also mark this instance as the default" };

    protected override Command GenerateCommand(ConfigStore configStore)
    {
        var command = base.GenerateCommand(configStore);
        command.Add(Key);
        command.Add(Url);
        command.Add(Token);
        command.Add(SetDefaultOpt);
        command.SetAction((parseResult, _) => new AddInstanceHandler(configStore, parseResult).InvokeAsync());
        return command;
    }
}
