using System.CommandLine;
using System.CommandLine.Parsing;
using PortainerLogs.Client;
using PortainerLogs.Commands.Instance;
using PortainerLogs.Config;

namespace PortainerLogs.Handlers.Instance;

public class AddInstanceHandler : AbstractHandler
{
    public AddInstanceHandler(ConfigStore configStore, ParseResult parseResult)
        : base(configStore, parseResult) { }

    private string Key => GetArgument(AddInstance.Key)!;
    private string Url => GetOption(AddInstance.Url)!;
    private string Token => GetOption(AddInstance.Token)!;
    private bool SetDefault => GetOption(AddInstance.SetDefaultOpt);

    public override async Task<int> InvokeAsync()
    {
        try
        {
            ConfigStore.AddInstance(Key, Url, Token, SetDefault);
            Console.WriteLine($"Instance '{Key}' added.");
            try
            {
                var client = new PortainerClient(Url, Token);
                var status = await client.GetStatusAsync();
                Console.WriteLine($"Connectivity OK — Portainer {status.Version}");
            }
            catch
            {
                Console.Error.WriteLine($"Warning: Could not reach {Url}. Instance saved but may be unreachable.");
            }
        }
        catch (InvalidOperationException ex)
        {
            Console.Error.WriteLine($"Error: {ex.Message}");
            Environment.ExitCode = 1;
            return 1;
        }

        return 0;
    }
}
