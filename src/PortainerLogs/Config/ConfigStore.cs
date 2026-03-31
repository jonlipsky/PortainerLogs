using System.Runtime.InteropServices;
using System.Text.Json;

namespace PortainerLogs.Config;

public class ConfigStore
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    private readonly string _configDir;
    private readonly string _configPath;

    public ConfigStore()
        : this(Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            ".portainer-logs"))
    {
    }

    public ConfigStore(string configDir)
    {
        _configDir = configDir;
        _configPath = Path.Combine(_configDir, "config.json");
    }

    public ToolConfig Load()
    {
        if (!File.Exists(_configPath))
            return new ToolConfig();

        var json = File.ReadAllText(_configPath);
        return JsonSerializer.Deserialize<ToolConfig>(json, JsonOptions) ?? new ToolConfig();
    }

    public void Save(ToolConfig config)
    {
        Directory.CreateDirectory(_configDir);

        var json = JsonSerializer.Serialize(config, JsonOptions);
        File.WriteAllText(_configPath, json);

        SetFilePermissions();
    }

    public void AddInstance(string key, string url, string token, bool setDefault)
    {
        var config = Load();

        if (config.Instances.ContainsKey(key))
            throw new InvalidOperationException($"Instance '{key}' already exists.");

        config.Instances[key] = new InstanceEntry { Url = url, Token = token };

        if (setDefault || config.Instances.Count == 1)
            config.Default = key;

        Save(config);
    }

    public void RemoveInstance(string key)
    {
        var config = Load();

        if (!config.Instances.Remove(key))
            throw new InvalidOperationException($"Instance '{key}' does not exist.");

        if (config.Default == key)
            config.Default = null;

        Save(config);
    }

    public void SetDefault(string key)
    {
        var config = Load();

        if (!config.Instances.ContainsKey(key))
            throw new InvalidOperationException($"Instance '{key}' does not exist.");

        config.Default = key;
        Save(config);
    }

    public void SetSetting(string key, string value)
    {
        var config = Load();

        switch (key)
        {
            case "fuzzy-match":
                if (!bool.TryParse(value, out var fuzzy))
                    throw new InvalidOperationException($"Invalid value '{value}' for fuzzy-match. Expected true or false.");
                config.Settings.FuzzyMatch = fuzzy;
                break;
            case "default-tail":
                if (!int.TryParse(value, out var tail) || tail <= 0)
                    throw new InvalidOperationException($"Invalid value '{value}' for default-tail. Expected a positive integer.");
                config.Settings.DefaultTail = tail;
                break;
            case "default-format":
                if (value is not ("plain" or "json"))
                    throw new InvalidOperationException($"Invalid value '{value}' for default-format. Expected 'plain' or 'json'.");
                config.Settings.DefaultFormat = value;
                break;
            default:
                throw new InvalidOperationException($"Unrecognised setting '{key}'. Valid keys: fuzzy-match, default-tail, default-format.");
        }

        Save(config);
    }

    public InstanceEntry GetInstance(string? key)
    {
        var config = Load();

        var resolvedKey = key ?? config.Default
            ?? throw new InvalidOperationException("No instance specified and no default configured. Run 'instance set-default <key>' first.");

        if (!config.Instances.TryGetValue(resolvedKey, out var instance))
            throw new InvalidOperationException($"Instance '{resolvedKey}' not found.");

        return instance;
    }

    private void SetFilePermissions()
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            File.SetUnixFileMode(_configPath,
                UnixFileMode.UserRead | UnixFileMode.UserWrite);
        }
    }
}
