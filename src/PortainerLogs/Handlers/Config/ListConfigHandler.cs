using System.CommandLine;
using System.CommandLine.Parsing;
using PortainerLogs.Commands.Config;
using PortainerLogs.Config;
using PortainerLogs.Formatting;

namespace PortainerLogs.Handlers.Config;

public class ListConfigHandler : AbstractHandler
{
    public ListConfigHandler(ConfigStore configStore, ParseResult parseResult)
        : base(configStore, parseResult) { }

    public override Task<int> InvokeAsync()
    {
        var config = ConfigStore.Load();
        var formatResult = ParseResult.GetResult(ListConfig.FormatOpt);
        var format = formatResult is not null && !formatResult.Implicit
            ? GetOption(ListConfig.FormatOpt)! : config.Settings.DefaultFormat;

        if (format == "json")
        {
            var settings = new
            {
                fuzzyMatch = config.Settings.FuzzyMatch,
                defaultTail = config.Settings.DefaultTail,
                defaultFormat = config.Settings.DefaultFormat
            };
            Console.WriteLine(JsonFormatter.Format(settings));
        }
        else
        {
            Console.WriteLine(PlainFormatter.FormatSettingsList(config.Settings));
        }

        return Task.FromResult(0);
    }
}
