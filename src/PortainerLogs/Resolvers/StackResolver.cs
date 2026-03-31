using PortainerLogs.Client;

namespace PortainerLogs.Resolvers;

public class StackResolver
{
    public static ResolveResult<PortainerStack> Resolve(
        IReadOnlyList<PortainerStack> stacks,
        string name,
        bool fuzzyEnabled)
    {
        // 1. Exact match on name or id
        var exact = stacks.FirstOrDefault(s =>
            s.Name.Equals(name, StringComparison.OrdinalIgnoreCase) ||
            s.Id.ToString().Equals(name, StringComparison.OrdinalIgnoreCase));

        if (exact != null)
            return new ResolveResult<PortainerStack>(exact, false, exact.Name);

        if (!fuzzyEnabled)
            throw new NoMatchException(
                $"No stack found with exact name '{name}'.",
                stacks.Select(s => s.Name).ToList());

        // 2. Fuzzy: case-insensitive substring
        var matches = stacks
            .Where(s => s.Name.Contains(name, StringComparison.OrdinalIgnoreCase))
            .ToList();

        return matches.Count switch
        {
            1 => new ResolveResult<PortainerStack>(matches[0], true, matches[0].Name),
            0 => throw new NoMatchException(
                $"No stack found matching '{name}'.",
                stacks.Select(s => s.Name).ToList()),
            _ => throw new AmbiguousMatchException(
                $"Multiple stacks match '{name}'.",
                matches.Select(s => s.Name).ToList())
        };
    }
}
