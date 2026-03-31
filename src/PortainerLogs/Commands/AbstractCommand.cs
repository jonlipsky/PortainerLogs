using System.CommandLine;
using PortainerLogs.Config;

namespace PortainerLogs.Commands;

public abstract class AbstractCommand
{
    public Command Command(ConfigStore configStore) => GenerateCommand(configStore);

    protected virtual Command GenerateCommand(ConfigStore configStore)
    {
        return new Command(Name, Description);
    }

    protected abstract string Name { get; }
    protected abstract string Description { get; }
}
