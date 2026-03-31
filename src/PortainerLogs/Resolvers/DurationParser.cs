using System.Text.RegularExpressions;

namespace PortainerLogs.Resolvers;

public static partial class DurationParser
{
    [GeneratedRegex(@"^(?:(\d+)h)?(?:(\d+)m)?$", RegexOptions.IgnoreCase)]
    private static partial Regex DurationRegex();

    public static TimeSpan Parse(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            throw new FormatException("Duration cannot be empty.");

        // Try ISO-8601 timestamp first (e.g. 2026-03-31T09:00:00Z)
        if (DateTimeOffset.TryParse(input, out var timestamp))
        {
            var elapsed = DateTimeOffset.UtcNow - timestamp;
            if (elapsed < TimeSpan.Zero)
                throw new FormatException($"Timestamp '{input}' is in the future.");
            return elapsed;
        }

        // Try Nh, Nm, NhNm pattern
        var match = DurationRegex().Match(input);
        if (!match.Success || (match.Groups[1].Length == 0 && match.Groups[2].Length == 0))
            throw new FormatException($"Invalid duration format: '{input}'. Expected Nh, Nm, NhNm, or an ISO-8601 timestamp.");

        var hours = match.Groups[1].Success ? int.Parse(match.Groups[1].Value) : 0;
        var minutes = match.Groups[2].Success ? int.Parse(match.Groups[2].Value) : 0;

        return new TimeSpan(hours, minutes, 0);
    }

    public static long ToUnixTimestamp(string input)
    {
        var duration = Parse(input);
        var since = DateTimeOffset.UtcNow - duration;
        return since.ToUnixTimeSeconds();
    }
}
