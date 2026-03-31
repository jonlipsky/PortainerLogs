using PortainerLogs.Client;
using PortainerLogs.Resolvers;

namespace PortainerLogs.Tests.Resolvers;

[TestClass]
public class StackResolverTests
{
    private static readonly List<PortainerStack> TestStacks =
    [
        new() { Id = 1, Name = "myapp", Status = 1, EndpointId = 1 },
        new() { Id = 2, Name = "monitoring", Status = 1, EndpointId = 1 },
        new() { Id = 3, Name = "myapp-staging", Status = 2, EndpointId = 1 },
    ];

    [TestMethod]
    public void Resolve_ExactNameMatch_ReturnsStack()
    {
        var result = StackResolver.Resolve(TestStacks, "myapp", fuzzyEnabled: true);

        Assert.AreEqual("myapp", result.ResolvedName);
        Assert.IsFalse(result.WasFuzzy);
        Assert.AreEqual(1, result.Value.Id);
    }

    [TestMethod]
    public void Resolve_ExactNameMatch_CaseInsensitive()
    {
        var result = StackResolver.Resolve(TestStacks, "MONITORING", fuzzyEnabled: true);

        Assert.AreEqual("monitoring", result.ResolvedName);
        Assert.IsFalse(result.WasFuzzy);
    }

    [TestMethod]
    public void Resolve_ExactIdMatch_ReturnsStack()
    {
        var result = StackResolver.Resolve(TestStacks, "2", fuzzyEnabled: true);

        Assert.AreEqual("monitoring", result.ResolvedName);
        Assert.IsFalse(result.WasFuzzy);
    }

    [TestMethod]
    public void Resolve_FuzzyEnabled_SingleMatch_ReturnsStack()
    {
        var result = StackResolver.Resolve(TestStacks, "monitor", fuzzyEnabled: true);

        Assert.AreEqual("monitoring", result.ResolvedName);
        Assert.IsTrue(result.WasFuzzy);
    }

    [TestMethod]
    public void Resolve_FuzzyEnabled_AmbiguousMatch_Throws()
    {
        // Use a name that doesn't exact-match but fuzzy-matches multiple
        var stacks = new List<PortainerStack>
        {
            new() { Id = 1, Name = "app-prod", Status = 1, EndpointId = 1 },
            new() { Id = 2, Name = "app-staging", Status = 1, EndpointId = 1 },
        };

        var ex = Assert.ThrowsExactly<AmbiguousMatchException>(
            () => StackResolver.Resolve(stacks, "app", fuzzyEnabled: true));

        Assert.AreEqual(3, ex.ExitCode);
        Assert.AreEqual(2, ex.Candidates.Count);
    }

    [TestMethod]
    public void Resolve_FuzzyEnabled_NoMatch_Throws()
    {
        var ex = Assert.ThrowsExactly<NoMatchException>(
            () => StackResolver.Resolve(TestStacks, "nonexistent", fuzzyEnabled: true));

        Assert.AreEqual(4, ex.ExitCode);
        Assert.AreEqual(3, ex.KnownNames.Count);
    }

    [TestMethod]
    public void Resolve_FuzzyDisabled_ExactMatch_Works()
    {
        var result = StackResolver.Resolve(TestStacks, "myapp", fuzzyEnabled: false);

        Assert.AreEqual("myapp", result.ResolvedName);
        Assert.IsFalse(result.WasFuzzy);
    }

    [TestMethod]
    public void Resolve_FuzzyDisabled_SubstringMatch_Throws()
    {
        var ex = Assert.ThrowsExactly<NoMatchException>(
            () => StackResolver.Resolve(TestStacks, "monitor", fuzzyEnabled: false));

        Assert.AreEqual(4, ex.ExitCode);
    }

    [TestMethod]
    public void Resolve_ExactMatchTakesPrecedenceOverFuzzy()
    {
        var result = StackResolver.Resolve(TestStacks, "myapp", fuzzyEnabled: true);

        Assert.IsFalse(result.WasFuzzy);
        Assert.AreEqual(1, result.Value.Id);
    }
}
