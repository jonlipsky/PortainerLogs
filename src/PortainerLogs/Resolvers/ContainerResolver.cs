using PortainerLogs.Client;

namespace PortainerLogs.Resolvers;

public record ResolveResult<T>(T Value, bool WasFuzzy, string ResolvedName);

public class ContainerResolver
{
    public static ResolveResult<DockerContainer> Resolve(
        IReadOnlyList<DockerContainer> containers,
        string name,
        bool fuzzyEnabled)
    {
        // 1. Exact match on name or id
        var exact = containers.FirstOrDefault(c =>
            c.DisplayName.Equals(name, StringComparison.OrdinalIgnoreCase) ||
            c.Id.Equals(name, StringComparison.OrdinalIgnoreCase));

        if (exact != null)
            return new ResolveResult<DockerContainer>(exact, false, exact.DisplayName);

        if (!fuzzyEnabled)
            throw new NoMatchException(
                $"No container found with exact name '{name}'.",
                containers.Select(c => c.DisplayName).ToList());

        // 2. Fuzzy: case-insensitive substring
        var matches = containers
            .Where(c => c.DisplayName.Contains(name, StringComparison.OrdinalIgnoreCase))
            .ToList();

        return matches.Count switch
        {
            1 => new ResolveResult<DockerContainer>(matches[0], true, matches[0].DisplayName),
            0 => throw new NoMatchException(
                $"No container found matching '{name}'.",
                containers.Select(c => c.DisplayName).ToList()),
            _ => throw new AmbiguousMatchException(
                $"Multiple containers match '{name}'.",
                matches.Select(c => c.DisplayName).ToList())
        };
    }
}

public class AmbiguousMatchException : Exception
{
    public IReadOnlyList<string> Candidates { get; }
    public int ExitCode => 3;

    public AmbiguousMatchException(string message, IReadOnlyList<string> candidates)
        : base(message)
    {
        Candidates = candidates;
    }
}

public class NoMatchException : Exception
{
    public IReadOnlyList<string> KnownNames { get; }
    public int ExitCode => 4;

    public NoMatchException(string message, IReadOnlyList<string> knownNames)
        : base(message)
    {
        KnownNames = knownNames;
    }
}
