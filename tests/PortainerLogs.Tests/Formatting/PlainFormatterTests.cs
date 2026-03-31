using PortainerLogs.Client;
using PortainerLogs.Config;
using PortainerLogs.Formatting;

namespace PortainerLogs.Tests.Formatting;

[TestClass]
public class PlainFormatterTests
{
    [TestMethod]
    public void FormatInstanceList_ShowsKeysAndUrls()
    {
        var config = new ToolConfig
        {
            Default = "home",
            Instances =
            {
                ["home"] = new InstanceEntry { Url = "http://mac-mini:9000", Token = "secret" },
                ["staging"] = new InstanceEntry { Url = "http://staging:9000", Token = "secret2" }
            }
        };

        var output = PlainFormatter.FormatInstanceList(config);

        StringAssert.Contains(output, "KEY");
        StringAssert.Contains(output, "URL");
        StringAssert.Contains(output, "DEFAULT");
        StringAssert.Contains(output, "home");
        StringAssert.Contains(output, "http://mac-mini:9000");
        StringAssert.Contains(output, "*"); // default marker
        StringAssert.Contains(output, "staging");
        // Tokens must never appear
        Assert.IsFalse(output.Contains("secret"));
    }

    [TestMethod]
    public void FormatSettingsList_ShowsAllSettings()
    {
        var settings = new ToolSettings
        {
            FuzzyMatch = true,
            DefaultTail = 200,
            DefaultFormat = "plain"
        };

        var output = PlainFormatter.FormatSettingsList(settings);

        StringAssert.Contains(output, "fuzzy-match");
        StringAssert.Contains(output, "true");
        StringAssert.Contains(output, "default-tail");
        StringAssert.Contains(output, "200");
        StringAssert.Contains(output, "default-format");
        StringAssert.Contains(output, "plain");
    }

    [TestMethod]
    public void FormatContainers_ShowsNameStatusImageCreated()
    {
        var containers = new List<DockerContainer>
        {
            new()
            {
                Id = "abc123",
                Names = ["/myapp-api-1"],
                Image = "myapp/api:1.0",
                State = "running",
                Created = DateTimeOffset.UtcNow.AddHours(-2).ToUnixTimeSeconds()
            }
        };

        var output = PlainFormatter.FormatContainers(containers);

        StringAssert.Contains(output, "NAME");
        StringAssert.Contains(output, "STATUS");
        StringAssert.Contains(output, "IMAGE");
        StringAssert.Contains(output, "CREATED");
        StringAssert.Contains(output, "myapp-api-1");
        StringAssert.Contains(output, "running");
        StringAssert.Contains(output, "myapp/api:1.0");
        StringAssert.Contains(output, "2h ago");
    }

    [TestMethod]
    public void FormatStacks_ShowsNameStatusEnv()
    {
        var stacks = new List<PortainerStack>
        {
            new() { Id = 1, Name = "myapp", Status = 1, EndpointId = 1 },
            new() { Id = 2, Name = "monitoring", Status = 2, EndpointId = 1 }
        };

        var output = PlainFormatter.FormatStacks(stacks);

        StringAssert.Contains(output, "myapp");
        StringAssert.Contains(output, "active");
        StringAssert.Contains(output, "monitoring");
        StringAssert.Contains(output, "inactive");
    }

    [TestMethod]
    public void FormatStacks_ShowsContainerCountsAndUpdated()
    {
        var now = DateTimeOffset.UtcNow;
        var stacks = new List<PortainerStack>
        {
            new() { Id = 1, Name = "myapp", Status = 1, EndpointId = 1, UpdateDate = now.AddHours(-1).ToUnixTimeSeconds() },
            new() { Id = 2, Name = "monitoring", Status = 1, EndpointId = 1, UpdateDate = now.AddDays(-3).ToUnixTimeSeconds() }
        };

        var output = PlainFormatter.FormatStacks(stacks, stackId => stackId switch
        {
            1 => (3, 3),
            2 => (1, 2),
            _ => null
        });

        StringAssert.Contains(output, "CONTAINERS");
        StringAssert.Contains(output, "UPDATED");
        StringAssert.Contains(output, "3 / 3");
        StringAssert.Contains(output, "1 / 2");
        StringAssert.Contains(output, "1h ago");
        StringAssert.Contains(output, "3d ago");
        // Stack with running < total should show partially-active
        StringAssert.Contains(output, "partially-active");
    }

    [TestMethod]
    public void FormatStats_ShowsCpuMemNet()
    {
        var stats = new DockerStats
        {
            CpuStats = new CpuStats
            {
                CpuUsage = new CpuUsage { TotalUsage = 5000000 },
                SystemCpuUsage = 100000000,
                OnlineCpus = 4
            },
            PreCpuStats = new CpuStats
            {
                CpuUsage = new CpuUsage { TotalUsage = 4000000 },
                SystemCpuUsage = 90000000,
                OnlineCpus = 4
            },
            MemoryStats = new MemoryStats
            {
                Usage = 327155712, // ~312 MiB
                Limit = 1073741824 // 1 GiB
            },
            Networks = new Dictionary<string, NetworkStats>
            {
                ["eth0"] = new() { RxBytes = 47185920, TxBytes = 12582912 }
            }
        };

        var output = PlainFormatter.FormatStats("myapp-api-1", stats);

        StringAssert.Contains(output, "CONTAINER");
        StringAssert.Contains(output, "CPU %");
        StringAssert.Contains(output, "MEM USAGE / LIMIT");
        StringAssert.Contains(output, "myapp-api-1");
        StringAssert.Contains(output, "MiB");
        StringAssert.Contains(output, "GiB");
    }

    [TestMethod]
    public void FormatEvents_ShowsTimestampTypeAction()
    {
        var events = new List<DockerEvent>
        {
            new()
            {
                Type = "container",
                Action = "die",
                Time = 1711875251,
                Actor = new EventActor
                {
                    Id = "abc123",
                    Attributes = new Dictionary<string, string>
                    {
                        ["name"] = "myapp-api-1",
                        ["exitCode"] = "137"
                    }
                }
            }
        };

        var output = PlainFormatter.FormatEvents(events);

        StringAssert.Contains(output, "TIMESTAMP");
        StringAssert.Contains(output, "TYPE");
        StringAssert.Contains(output, "ACTION");
        StringAssert.Contains(output, "container");
        StringAssert.Contains(output, "die");
        StringAssert.Contains(output, "myapp-api-1");
        StringAssert.Contains(output, "exitCode=137");
    }

    [TestMethod]
    public void FormatInstanceStatus_ShowsStatusAndVersion()
    {
        var header = PlainFormatter.FormatInstanceStatusHeader();
        var row = PlainFormatter.FormatInstanceStatus("home", "http://mac-mini:9000", true, "2.31.2");

        StringAssert.Contains(header, "KEY");
        StringAssert.Contains(header, "STATUS");
        StringAssert.Contains(header, "VERSION");
        StringAssert.Contains(row, "OK");
        StringAssert.Contains(row, "2.31.2");

        var unreachable = PlainFormatter.FormatInstanceStatus("staging", "http://staging:9000", false, null);
        StringAssert.Contains(unreachable, "UNREACHABLE");
    }

    [TestMethod]
    public void FormatInspect_ShowsSections()
    {
        var inspect = new DockerInspectResponse
        {
            Id = "abc123",
            Name = "/myapp-api-1",
            Config = new InspectConfig
            {
                Image = "myapp/api:1.0",
                Entrypoint = ["dotnet"],
                Cmd = ["MyApp.dll"],
                Env = ["ASPNETCORE_URLS=http://+:8080", "DOTNET_ENVIRONMENT=Production"]
            },
            HostConfig = new InspectHostConfig
            {
                RestartPolicy = new RestartPolicy { Name = "unless-stopped", MaximumRetryCount = 0 },
                PortBindings = new Dictionary<string, List<PortBinding>>
                {
                    ["8080/tcp"] = [new PortBinding { HostIp = "0.0.0.0", HostPort = "8080" }]
                }
            },
            Mounts =
            [
                new InspectMount { Type = "bind", Source = "/data", Destination = "/app/data", ReadWrite = true }
            ],
            NetworkSettings = new InspectNetworkSettings
            {
                Networks = new Dictionary<string, NetworkEntry>
                {
                    ["bridge"] = new() { IpAddress = "172.17.0.2", Gateway = "172.17.0.1" }
                }
            },
            RestartCount = 3
        };

        var output = PlainFormatter.FormatInspect(inspect);

        StringAssert.Contains(output, "IMAGE & COMMAND");
        StringAssert.Contains(output, "myapp/api:1.0");
        StringAssert.Contains(output, "dotnet");
        StringAssert.Contains(output, "ENVIRONMENT VARIABLES");
        StringAssert.Contains(output, "ASPNETCORE_URLS");
        StringAssert.Contains(output, "PORT BINDINGS");
        StringAssert.Contains(output, "8080");
        StringAssert.Contains(output, "MOUNTS");
        StringAssert.Contains(output, "/data");
        StringAssert.Contains(output, "RESTART POLICY");
        StringAssert.Contains(output, "unless-stopped");
        StringAssert.Contains(output, "NETWORKS");
        StringAssert.Contains(output, "172.17.0.2");
    }
}
