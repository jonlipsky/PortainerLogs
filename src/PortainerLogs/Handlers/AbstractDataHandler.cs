using System.CommandLine;
using System.CommandLine.Parsing;
using PortainerLogs.Client;
using PortainerLogs.Commands;
using PortainerLogs.Config;

namespace PortainerLogs.Handlers;

public abstract class AbstractDataHandler : AbstractHandler
{
    protected AbstractDataHandler(ConfigStore configStore, ParseResult parseResult)
        : base(configStore, parseResult) { }

    protected PortainerClient CreateClient()
    {
        var instanceKey = GetOption(CommonOptions.Instance);
        var instance = ConfigStore.GetInstance(instanceKey);
        return new PortainerClient(instance.Url, instance.Token);
    }

    protected async Task<int> ResolveEnvId(PortainerClient client)
    {
        var envId = GetOption(CommonOptions.Env);
        if (envId.HasValue) return envId.Value;
        var endpoints = await client.GetEndpointsAsync();
        if (endpoints.Count == 0)
            throw new InvalidOperationException("No environments found on this Portainer instance.");
        return endpoints[0].Id;
    }

    protected string ResolveFormat()
    {
        var config = ConfigStore.Load();
        var result = ParseResult.GetResult(CommonOptions.Format);
        if (result is not null && !result.Implicit)
            return GetOption(CommonOptions.Format)!;
        return config.Settings.DefaultFormat;
    }

    protected bool ResolveFuzzy()
    {
        var config = ConfigStore.Load();
        var fuzzy = GetOption(CommonOptions.Fuzzy);
        var noFuzzy = GetOption(CommonOptions.NoFuzzy);
        if (fuzzy) return true;
        if (noFuzzy) return false;
        return config.Settings.FuzzyMatch;
    }
}
