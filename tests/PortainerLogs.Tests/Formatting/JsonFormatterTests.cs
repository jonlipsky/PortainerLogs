using System.Text.Json;
using PortainerLogs.Client;
using PortainerLogs.Formatting;

namespace PortainerLogs.Tests.Formatting;

[TestClass]
public class JsonFormatterTests
{
    [TestMethod]
    public void Format_Containers_UsesCamelCase()
    {
        var containers = new[]
        {
            new DockerContainer
            {
                Id = "abc123",
                Names = ["/myapp-api-1"],
                Image = "myapp/api:1.0",
                State = "running",
                Status = "Up 2 hours",
                Created = 1711875292
            }
        };

        var json = JsonFormatter.Format(containers);
        var doc = JsonDocument.Parse(json);
        var first = doc.RootElement[0];

        // JsonPropertyName attributes produce exact casing from API
        Assert.IsTrue(first.TryGetProperty("Id", out _));
        Assert.IsTrue(first.TryGetProperty("Names", out _));
        Assert.IsTrue(first.TryGetProperty("Image", out _));
        Assert.IsTrue(first.TryGetProperty("State", out _));
        // DisplayName has no JsonPropertyName, so camelCase policy applies
        Assert.IsTrue(first.TryGetProperty("displayName", out _));
    }

    [TestMethod]
    public void Format_Stacks_UsesCamelCase()
    {
        var stacks = new[]
        {
            new PortainerStack { Id = 1, Name = "myapp", Status = 1, EndpointId = 1 }
        };

        var json = JsonFormatter.Format(stacks);
        var doc = JsonDocument.Parse(json);
        var first = doc.RootElement[0];

        // JsonPropertyName attributes produce exact casing from API
        Assert.IsTrue(first.TryGetProperty("Id", out _));
        Assert.IsTrue(first.TryGetProperty("Name", out _));
        Assert.IsTrue(first.TryGetProperty("EndpointId", out _));
        // StatusText has no JsonPropertyName, so camelCase policy applies
        Assert.IsTrue(first.TryGetProperty("statusText", out _));
    }

    [TestMethod]
    public void Format_ProducesIndentedJson()
    {
        var data = new { Name = "test", Value = 42 };
        var json = JsonFormatter.Format(data);

        Assert.IsTrue(json.Contains('\n'));
        Assert.IsTrue(json.Contains("  ")); // indented
    }

    [TestMethod]
    public void FormatRaw_PassesThroughJsonElement()
    {
        var doc = JsonDocument.Parse("""{"foo":"bar","nested":{"a":1}}""");
        var json = JsonFormatter.FormatRaw(doc.RootElement);

        Assert.IsTrue(json.Contains("foo"));
        Assert.IsTrue(json.Contains("bar"));
        Assert.IsTrue(json.Contains("nested"));
    }

    [TestMethod]
    public void Format_NullProperties_AreOmitted()
    {
        var data = new { Name = "test", Nullable = (string?)null };
        var json = JsonFormatter.Format(data);

        Assert.IsFalse(json.Contains("nullable"));
    }
}
