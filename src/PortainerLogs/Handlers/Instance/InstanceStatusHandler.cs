using System.CommandLine;
using System.CommandLine.Parsing;
using PortainerLogs.Client;
using PortainerLogs.Commands.Instance;
using PortainerLogs.Config;
using PortainerLogs.Formatting;

namespace PortainerLogs.Handlers.Instance;

public class InstanceStatusHandler : AbstractHandler
{
    public InstanceStatusHandler(ConfigStore configStore, ParseResult parseResult)
        : base(configStore, parseResult) { }

    public override async Task<int> InvokeAsync()
    {
        var instanceKey = GetOption(InstanceStatus.InstanceOpt);
        var config = ConfigStore.Load();
        var formatResult = ParseResult.GetResult(InstanceStatus.FormatOpt);
        var format = formatResult is not null && !formatResult.Implicit
            ? GetOption(InstanceStatus.FormatOpt)! : config.Settings.DefaultFormat;

        var instancesToCheck = instanceKey != null
            ? config.Instances.Where(i => i.Key == instanceKey)
            : config.Instances;

        var results = new List<object>();

        foreach (var (key, instance) in instancesToCheck)
        {
            try
            {
                var client = new PortainerClient(instance.Url, instance.Token);
                var status = await client.GetStatusAsync();
                results.Add(new { key, url = instance.Url, status = "OK", version = status.Version });
            }
            catch
            {
                results.Add(new { key, url = instance.Url, status = "UNREACHABLE", version = (string?)null });
            }
        }

        if (format == "json")
        {
            Console.WriteLine(JsonFormatter.Format(results));
        }
        else
        {
            Console.Write(PlainFormatter.FormatInstanceStatusHeader());
            foreach (dynamic r in results)
            {
                Console.WriteLine(PlainFormatter.FormatInstanceStatus(r.key, r.url, r.status == "OK", r.version));
            }
        }

        return 0;
    }
}
