using PortainerLogs.Client;
using PortainerLogs.Resolvers;

namespace PortainerLogs.Tests.Resolvers;

[TestClass]
public class ContainerResolverTests
{
    private static readonly List<DockerContainer> TestContainers =
    [
        new() { Id = "abc123def456", Names = ["/myapp-api-1"], Image = "myapp/api:1.0", State = "running" },
        new() { Id = "def456ghi789", Names = ["/myapp-web-1"], Image = "myapp/web:1.0", State = "running" },
        new() { Id = "ghi789jkl012", Names = ["/myapp-worker-1"], Image = "myapp/worker:1.0", State = "running" },
    ];

    [TestMethod]
    public void Resolve_ExactNameMatch_ReturnsContainer()
    {
        var result = ContainerResolver.Resolve(TestContainers, "myapp-api-1", fuzzyEnabled: true);

        Assert.AreEqual("myapp-api-1", result.ResolvedName);
        Assert.IsFalse(result.WasFuzzy);
        Assert.AreEqual("abc123def456", result.Value.Id);
    }

    [TestMethod]
    public void Resolve_ExactNameMatch_CaseInsensitive()
    {
        var result = ContainerResolver.Resolve(TestContainers, "MYAPP-API-1", fuzzyEnabled: true);

        Assert.AreEqual("myapp-api-1", result.ResolvedName);
        Assert.IsFalse(result.WasFuzzy);
    }

    [TestMethod]
    public void Resolve_ExactIdMatch_ReturnsContainer()
    {
        var result = ContainerResolver.Resolve(TestContainers, "abc123def456", fuzzyEnabled: true);

        Assert.AreEqual("myapp-api-1", result.ResolvedName);
        Assert.IsFalse(result.WasFuzzy);
    }

    [TestMethod]
    public void Resolve_FuzzyEnabled_SingleMatch_ReturnsContainer()
    {
        var result = ContainerResolver.Resolve(TestContainers, "api", fuzzyEnabled: true);

        Assert.AreEqual("myapp-api-1", result.ResolvedName);
        Assert.IsTrue(result.WasFuzzy);
    }

    [TestMethod]
    public void Resolve_FuzzyEnabled_AmbiguousMatch_Throws()
    {
        var ex = Assert.ThrowsExactly<AmbiguousMatchException>(
            () => ContainerResolver.Resolve(TestContainers, "myapp", fuzzyEnabled: true));

        Assert.AreEqual(3, ex.ExitCode);
        Assert.AreEqual(3, ex.Candidates.Count);
    }

    [TestMethod]
    public void Resolve_FuzzyEnabled_NoMatch_Throws()
    {
        var ex = Assert.ThrowsExactly<NoMatchException>(
            () => ContainerResolver.Resolve(TestContainers, "nonexistent", fuzzyEnabled: true));

        Assert.AreEqual(4, ex.ExitCode);
        Assert.AreEqual(3, ex.KnownNames.Count);
    }

    [TestMethod]
    public void Resolve_FuzzyDisabled_ExactMatch_Works()
    {
        var result = ContainerResolver.Resolve(TestContainers, "myapp-api-1", fuzzyEnabled: false);

        Assert.AreEqual("myapp-api-1", result.ResolvedName);
        Assert.IsFalse(result.WasFuzzy);
    }

    [TestMethod]
    public void Resolve_FuzzyDisabled_SubstringMatch_Throws()
    {
        var ex = Assert.ThrowsExactly<NoMatchException>(
            () => ContainerResolver.Resolve(TestContainers, "api", fuzzyEnabled: false));

        Assert.AreEqual(4, ex.ExitCode);
    }

    [TestMethod]
    public void Resolve_FuzzyEnabled_SingleSubstringMatch_IsFuzzy()
    {
        var result = ContainerResolver.Resolve(TestContainers, "worker", fuzzyEnabled: true);

        Assert.IsTrue(result.WasFuzzy);
        Assert.AreEqual("myapp-worker-1", result.ResolvedName);
    }

    [TestMethod]
    public void Resolve_ExactMatchTakesPrecedenceOverFuzzy()
    {
        // Even if "myapp-api-1" would fuzzy match multiple, exact match wins
        var result = ContainerResolver.Resolve(TestContainers, "myapp-api-1", fuzzyEnabled: true);

        Assert.IsFalse(result.WasFuzzy);
        Assert.AreEqual("myapp-api-1", result.ResolvedName);
    }
}
