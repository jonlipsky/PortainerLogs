using System.CommandLine;
using System.CommandLine.Parsing;
using PortainerLogs.Config;

namespace PortainerLogs.Handlers;

public abstract class AbstractHandler
{
    protected AbstractHandler(ConfigStore configStore, ParseResult parseResult)
    {
        ConfigStore = configStore;
        ParseResult = parseResult;
    }

    protected ConfigStore ConfigStore { get; }
    protected ParseResult ParseResult { get; }

    protected T? GetOption<T>(Option<T> option) => ParseResult.GetValue(option);
    protected T GetArgument<T>(Argument<T> argument) => ParseResult.GetValue(argument)!;

    public abstract Task<int> InvokeAsync();
}
