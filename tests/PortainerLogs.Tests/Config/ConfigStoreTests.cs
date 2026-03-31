using PortainerLogs.Config;

namespace PortainerLogs.Tests.Config;

[TestClass]
public class ConfigStoreTests
{
    private string _tempDir = null!;
    private ConfigStore _store = null!;

    [TestInitialize]
    public void Setup()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), "portainer-logs-tests-" + Guid.NewGuid().ToString("N"));
        _store = new ConfigStore(_tempDir);
    }

    [TestCleanup]
    public void Cleanup()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, recursive: true);
    }

    [TestMethod]
    public void Load_NoFile_ReturnsDefaultConfig()
    {
        var config = _store.Load();

        Assert.IsNotNull(config);
        Assert.IsNull(config.Default);
        Assert.AreEqual(0, config.Instances.Count);
        Assert.IsTrue(config.Settings.FuzzyMatch);
        Assert.AreEqual(200, config.Settings.DefaultTail);
        Assert.AreEqual("plain", config.Settings.DefaultFormat);
    }

    [TestMethod]
    public void AddInstance_FirstInstance_BecomesDefault()
    {
        _store.AddInstance("home", "http://localhost:9000", "token123", setDefault: false);

        var config = _store.Load();
        Assert.AreEqual("home", config.Default);
        Assert.AreEqual(1, config.Instances.Count);
        Assert.AreEqual("http://localhost:9000", config.Instances["home"].Url);
        Assert.AreEqual("token123", config.Instances["home"].Token);
    }

    [TestMethod]
    public void AddInstance_SecondInstance_DoesNotChangeDefault()
    {
        _store.AddInstance("home", "http://localhost:9000", "token1", setDefault: false);
        _store.AddInstance("staging", "http://staging:9000", "token2", setDefault: false);

        var config = _store.Load();
        Assert.AreEqual("home", config.Default);
        Assert.AreEqual(2, config.Instances.Count);
    }

    [TestMethod]
    public void AddInstance_WithSetDefault_OverridesDefault()
    {
        _store.AddInstance("home", "http://localhost:9000", "token1", setDefault: false);
        _store.AddInstance("staging", "http://staging:9000", "token2", setDefault: true);

        var config = _store.Load();
        Assert.AreEqual("staging", config.Default);
    }

    [TestMethod]
    public void AddInstance_DuplicateKey_Throws()
    {
        _store.AddInstance("home", "http://localhost:9000", "token1", setDefault: false);

        var ex = Assert.ThrowsExactly<InvalidOperationException>(
            () => _store.AddInstance("home", "http://other:9000", "token2", setDefault: false));

        StringAssert.Contains(ex.Message, "already exists");
    }

    [TestMethod]
    public void RemoveInstance_ExistingKey_Removes()
    {
        _store.AddInstance("home", "http://localhost:9000", "token1", setDefault: false);
        _store.AddInstance("staging", "http://staging:9000", "token2", setDefault: false);

        _store.RemoveInstance("staging");

        var config = _store.Load();
        Assert.AreEqual(1, config.Instances.Count);
        Assert.IsFalse(config.Instances.ContainsKey("staging"));
    }

    [TestMethod]
    public void RemoveInstance_DefaultInstance_ClearsDefault()
    {
        _store.AddInstance("home", "http://localhost:9000", "token1", setDefault: false);

        _store.RemoveInstance("home");

        var config = _store.Load();
        Assert.IsNull(config.Default);
        Assert.AreEqual(0, config.Instances.Count);
    }

    [TestMethod]
    public void RemoveInstance_NonExistentKey_Throws()
    {
        var ex = Assert.ThrowsExactly<InvalidOperationException>(
            () => _store.RemoveInstance("nope"));

        StringAssert.Contains(ex.Message, "does not exist");
    }

    [TestMethod]
    public void SetDefault_ExistingKey_SetsDefault()
    {
        _store.AddInstance("home", "http://localhost:9000", "token1", setDefault: false);
        _store.AddInstance("staging", "http://staging:9000", "token2", setDefault: false);

        _store.SetDefault("staging");

        var config = _store.Load();
        Assert.AreEqual("staging", config.Default);
    }

    [TestMethod]
    public void SetDefault_NonExistentKey_Throws()
    {
        var ex = Assert.ThrowsExactly<InvalidOperationException>(
            () => _store.SetDefault("nope"));

        StringAssert.Contains(ex.Message, "does not exist");
    }

    [TestMethod]
    public void SetSetting_FuzzyMatch_ValidValues()
    {
        _store.Save(new ToolConfig());

        _store.SetSetting("fuzzy-match", "false");
        Assert.IsFalse(_store.Load().Settings.FuzzyMatch);

        _store.SetSetting("fuzzy-match", "true");
        Assert.IsTrue(_store.Load().Settings.FuzzyMatch);
    }

    [TestMethod]
    public void SetSetting_FuzzyMatch_InvalidValue_Throws()
    {
        _store.Save(new ToolConfig());

        var ex = Assert.ThrowsExactly<InvalidOperationException>(
            () => _store.SetSetting("fuzzy-match", "maybe"));

        StringAssert.Contains(ex.Message, "Invalid value");
    }

    [TestMethod]
    public void SetSetting_DefaultTail_ValidValue()
    {
        _store.Save(new ToolConfig());

        _store.SetSetting("default-tail", "500");

        Assert.AreEqual(500, _store.Load().Settings.DefaultTail);
    }

    [TestMethod]
    public void SetSetting_DefaultTail_InvalidValues_Throw()
    {
        _store.Save(new ToolConfig());

        Assert.ThrowsExactly<InvalidOperationException>(
            () => _store.SetSetting("default-tail", "abc"));

        Assert.ThrowsExactly<InvalidOperationException>(
            () => _store.SetSetting("default-tail", "0"));

        Assert.ThrowsExactly<InvalidOperationException>(
            () => _store.SetSetting("default-tail", "-5"));
    }

    [TestMethod]
    public void SetSetting_DefaultFormat_ValidValues()
    {
        _store.Save(new ToolConfig());

        _store.SetSetting("default-format", "json");
        Assert.AreEqual("json", _store.Load().Settings.DefaultFormat);

        _store.SetSetting("default-format", "plain");
        Assert.AreEqual("plain", _store.Load().Settings.DefaultFormat);
    }

    [TestMethod]
    public void SetSetting_DefaultFormat_InvalidValue_Throws()
    {
        _store.Save(new ToolConfig());

        var ex = Assert.ThrowsExactly<InvalidOperationException>(
            () => _store.SetSetting("default-format", "xml"));

        StringAssert.Contains(ex.Message, "Invalid value");
    }

    [TestMethod]
    public void SetSetting_UnrecognisedKey_Throws()
    {
        _store.Save(new ToolConfig());

        var ex = Assert.ThrowsExactly<InvalidOperationException>(
            () => _store.SetSetting("unknown-key", "value"));

        StringAssert.Contains(ex.Message, "Unrecognised setting");
    }

    [TestMethod]
    public void GetInstance_WithKey_ReturnsInstance()
    {
        _store.AddInstance("home", "http://localhost:9000", "token1", setDefault: false);

        var instance = _store.GetInstance("home");

        Assert.AreEqual("http://localhost:9000", instance.Url);
        Assert.AreEqual("token1", instance.Token);
    }

    [TestMethod]
    public void GetInstance_NullKey_ReturnsDefault()
    {
        _store.AddInstance("home", "http://localhost:9000", "token1", setDefault: false);

        var instance = _store.GetInstance(null);

        Assert.AreEqual("http://localhost:9000", instance.Url);
    }

    [TestMethod]
    public void GetInstance_NullKey_NoDefault_Throws()
    {
        var ex = Assert.ThrowsExactly<InvalidOperationException>(
            () => _store.GetInstance(null));

        StringAssert.Contains(ex.Message, "No instance specified");
    }

    [TestMethod]
    public void GetInstance_NonExistentKey_Throws()
    {
        var ex = Assert.ThrowsExactly<InvalidOperationException>(
            () => _store.GetInstance("nope"));

        StringAssert.Contains(ex.Message, "not found");
    }

    [TestMethod]
    public void Save_CreatesDirectoryAndFile()
    {
        _store.Save(new ToolConfig());

        Assert.IsTrue(File.Exists(Path.Combine(_tempDir, "config.json")));
    }

    [TestMethod]
    public void Save_RoundTripsConfig()
    {
        var original = new ToolConfig
        {
            Default = "prod",
            Settings = new ToolSettings
            {
                FuzzyMatch = false,
                DefaultTail = 100,
                DefaultFormat = "json"
            },
            Instances =
            {
                ["prod"] = new InstanceEntry { Url = "http://prod:9000", Token = "secret" }
            }
        };

        _store.Save(original);
        var loaded = _store.Load();

        Assert.AreEqual("prod", loaded.Default);
        Assert.IsFalse(loaded.Settings.FuzzyMatch);
        Assert.AreEqual(100, loaded.Settings.DefaultTail);
        Assert.AreEqual("json", loaded.Settings.DefaultFormat);
        Assert.AreEqual("http://prod:9000", loaded.Instances["prod"].Url);
        Assert.AreEqual("secret", loaded.Instances["prod"].Token);
    }
}
