using System.CommandLine;
using System.CommandLine.Parsing;
using PortainerLogs.Commands.Instance;
using PortainerLogs.Config;
using PortainerLogs.Formatting;

namespace PortainerLogs.Handlers.Instance;

public class ListInstancesHandler : AbstractHandler
{
    public ListInstancesHandler(ConfigStore configStore, ParseResult parseResult)
        : base(configStore, parseResult) { }

    public override Task<int> InvokeAsync()
    {
        var config = ConfigStore.Load();
        var formatResult = ParseResult.GetResult(ListInstances.FormatOpt);
        var format = formatResult is not null && !formatResult.Implicit
            ? GetOption(ListInstances.FormatOpt)! : config.Settings.DefaultFormat;

        if (format == "json")
        {
            var instances = config.Instances.Select(i => new
            {
                key = i.Key,
                url = i.Value.Url,
                isDefault = i.Key == config.Default
            }).ToList();
            Console.WriteLine(JsonFormatter.Format(instances));
        }
        else
        {
            Console.WriteLine(PlainFormatter.FormatInstanceList(config));
        }

        return Task.FromResult(0);
    }
}
