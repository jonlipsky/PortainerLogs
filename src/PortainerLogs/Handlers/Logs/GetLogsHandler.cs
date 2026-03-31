using System.CommandLine;
using System.CommandLine.Parsing;
using System.Text.RegularExpressions;
using PortainerLogs.Client;
using PortainerLogs.Commands.Logs;
using PortainerLogs.Config;
using PortainerLogs.Formatting;
using PortainerLogs.Resolvers;

namespace PortainerLogs.Handlers.Logs;

public class GetLogsHandler(ConfigStore configStore, ParseResult parseResult)
    : AbstractDataHandler(configStore, parseResult)
{
    public override async Task<int> InvokeAsync()
    {
        try
        {
            var containerName = GetArgument(GetLogs.Container);
            var tail = GetOption(GetLogs.Tail);
            var since = GetOption(GetLogs.Since);
            var level = GetOption(GetLogs.Level);
            var grep = GetOption(GetLogs.Grep);
            var noTimestamps = GetOption(GetLogs.NoTimestamps);

            var format = ResolveFormat();
            var client = CreateClient();
            var fuzzyEnabled = ResolveFuzzy();

            var resolvedEnvId = await ResolveEnvId(client);
            var containers = await client.GetContainersAsync(resolvedEnvId, all: true);
            var result = ContainerResolver.Resolve(containers, containerName, fuzzyEnabled);

            if (result.WasFuzzy)
                Console.Error.WriteLine($"Resolved to container: {result.ResolvedName}");

            var config = ConfigStore.Load();
            var resolvedTail = tail ?? config.Settings.DefaultTail;
            long? sinceTimestamp = since != null ? DurationParser.ToUnixTimestamp(since) : null;

            var rawLogs = await client.GetContainerLogsAsync(
                resolvedEnvId, result.Value.Id, resolvedTail, sinceTimestamp, !noTimestamps);

            var rawLines = rawLogs.Split('\n', StringSplitOptions.RemoveEmptyEntries);

            // Strip Docker stream header bytes (8-byte prefix per line), preserving stream type
            var parsed = rawLines.Select(l => DetectStreamAndStrip(l)).ToArray();
            var lines = parsed.Select(p => p.Line).ToArray();

            // Apply filters
            if (level?.Equals("error", StringComparison.OrdinalIgnoreCase) == true)
                lines = lines.Where(l =>
                    l.Contains("Error", StringComparison.OrdinalIgnoreCase) ||
                    l.Contains("Critical", StringComparison.OrdinalIgnoreCase) ||
                    l.Contains("fail:", StringComparison.OrdinalIgnoreCase)).ToArray();

            if (grep != null)
            {
                try
                {
                    var regex = new Regex(grep, RegexOptions.IgnoreCase);
                    lines = lines.Where(l => regex.IsMatch(l)).ToArray();
                }
                catch (RegexParseException)
                {
                    lines = lines.Where(l => l.Contains(grep, StringComparison.OrdinalIgnoreCase)).ToArray();
                }
            }

            if (format == "json")
            {
                var filteredLineSet = new HashSet<string>(lines);
                var filteredParsed = parsed.Where(p => filteredLineSet.Contains(p.Line));
                var logEntries = filteredParsed.Select(p => ParseLogLine(p.Line, p.Stream)).ToList();
                Console.WriteLine(JsonFormatter.Format(logEntries));
            }
            else
            {
                foreach (var line in lines)
                    Console.WriteLine(line);
            }

            Environment.ExitCode = 0;
            return 0;
        }
        catch (AmbiguousMatchException ex)
        {
            Console.Error.WriteLine($"Error: {ex.Message}");
            Console.Error.WriteLine("Candidates: " + string.Join(", ", ex.Candidates));
            Environment.ExitCode = 3;
            return 3;
        }
        catch (NoMatchException ex)
        {
            Console.Error.WriteLine($"Error: {ex.Message}");
            Console.Error.WriteLine("Known containers: " + string.Join(", ", ex.KnownNames));
            Environment.ExitCode = 4;
            return 4;
        }
        catch (FormatException ex)
        {
            Console.Error.WriteLine($"Error: {ex.Message}");
            Environment.ExitCode = 1;
            return 1;
        }
        catch (InvalidOperationException ex)
        {
            Console.Error.WriteLine($"Error: {ex.Message}");
            Environment.ExitCode = 1;
            return 1;
        }
        catch (HttpRequestException ex)
        {
            Console.Error.WriteLine($"Error: Could not connect — {ex.Message}");
            Environment.ExitCode = 2;
            return 2;
        }
    }

    private static (string Line, string Stream) DetectStreamAndStrip(string line)
    {
        if (line.Length > 8 && line[0] is '\x01' or '\x02')
        {
            var stream = line[0] == '\x02' ? "stderr" : "stdout";
            return (line[8..], stream);
        }
        return (line, "stdout");
    }

    private static object ParseLogLine(string line, string stream)
    {
        var spaceIdx = line.IndexOf(' ');
        if (spaceIdx > 0 && DateTimeOffset.TryParse(line[..spaceIdx], out var ts))
        {
            return new
            {
                timestamp = ts.ToString("o"),
                stream,
                line = line[(spaceIdx + 1)..]
            };
        }

        return new
        {
            timestamp = (string?)null,
            stream,
            line
        };
    }
}
