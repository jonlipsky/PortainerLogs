using System.CommandLine;
using System.Reflection;
using PortainerLogs.Commands.Config;
using PortainerLogs.Commands.Containers;
using PortainerLogs.Commands.Events;
using PortainerLogs.Commands.Inspect;
using PortainerLogs.Commands.Instance;
using PortainerLogs.Commands.Logs;
using PortainerLogs.Commands.Stacks;
using PortainerLogs.Commands.Stats;
using PortainerLogs.Config;

// Handle --version before System.CommandLine parsing to avoid interfering with help/subcommand dispatch
if (args.Length == 1 && args[0] == "--version")
{
    var version = Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
        ?? Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "unknown";
    Console.WriteLine(version);
    return 0;
}

var configStore = new ConfigStore();
var rootCommand = new RootCommand("Read-only CLI tool for inspecting containerised infrastructure across Portainer instances");

rootCommand.Add(new Instance().Command(configStore));
rootCommand.Add(new Config().Command(configStore));
rootCommand.Add(new ListContainers().Command(configStore));
rootCommand.Add(new Stacks().Command(configStore));
rootCommand.Add(new GetLogs().Command(configStore));
rootCommand.Add(new InspectContainer().Command(configStore));
rootCommand.Add(new ContainerStats().Command(configStore));
rootCommand.Add(new ListEvents().Command(configStore));

return await rootCommand.Parse(args).InvokeAsync();
