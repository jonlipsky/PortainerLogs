using System.CommandLine;

namespace PortainerLogs.Commands;

public static class CommonOptions
{
    public static readonly Option<string?> Instance = new("--instance")
    {
        Description = "Override the default Portainer instance for this invocation"
    };

    public static readonly Option<string> Format = new("--format")
    {
        Description = "Output format: plain or json",
        DefaultValueFactory = _ => "plain"
    };

    public static readonly Option<bool> Fuzzy = new("--fuzzy")
    {
        Description = "Enable fuzzy name resolution for this invocation"
    };

    public static readonly Option<bool> NoFuzzy = new("--no-fuzzy")
    {
        Description = "Require exact name match for this invocation"
    };

    public static readonly Option<int?> Env = new("--env")
    {
        Description = "Filter to a specific Portainer environment ID"
    };

    public static void AddDataOptions(Command command)
    {
        command.Add(Instance);
        command.Add(Format);
        command.Add(Env);
    }

    public static void AddResolverOptions(Command command)
    {
        command.Add(Fuzzy);
        command.Add(NoFuzzy);
    }
}
