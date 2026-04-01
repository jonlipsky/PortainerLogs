# portainer-logs

A read-only CLI tool for inspecting containerised infrastructure across one or more [Portainer](https://www.portainer.io/) instances. Designed for developers diagnosing production issues from a terminal and for use as a subprocess by AI coding assistants like Claude Code.

All operations are reads — no mutations, no stack management writes, no user changes.

## Installation

**NuGet (.NET global tool — requires .NET 10):**

```bash
dotnet tool install --global PortainerLogs
```

**Standalone binaries** are available on the [Releases](https://github.com/jonlipsky/PortainerLogs/releases) page for Windows, Linux, and macOS (x64 and ARM64).

## Quick Start

```bash
# Add a Portainer instance
portainer-logs instance add home --url http://my-server:9000 --token ptr_abc123

# List running containers
portainer-logs containers

# Fetch recent logs
portainer-logs logs myapp-api

# Filter to errors in the last hour
portainer-logs logs myapp-api --since 1h --level error

# Check container resource usage
portainer-logs stats myapp-api

# See recent Docker events (crashes, OOM kills, restarts)
portainer-logs events --since 2h
```

## Commands

| Command | Description |
|---------|-------------|
| `instance add` | Add a Portainer server (validates connectivity) |
| `instance remove` | Remove a configured server |
| `instance set-default` | Set the default server |
| `instance list` | List all configured servers |
| `instance status` | Check reachability and Portainer version |
| `config set` | Set a global option |
| `config list` | Show all global settings |
| `containers` | List containers (running by default, `--all` for stopped) |
| `stacks list` | List deployed stacks with container health |
| `stacks get` | Show the docker-compose file for a stack |
| `logs` | Fetch container logs with filtering |
| `inspect` | Show full container configuration |
| `stats` | Show CPU, memory, and network usage snapshot |
| `events` | Show recent Docker events |

## Log Filtering

```bash
# Last 500 lines
portainer-logs logs myapp --tail 500

# Last 30 minutes
portainer-logs logs myapp --since 30m

# Errors only
portainer-logs logs myapp --level error

# Grep for a pattern (substring or regex)
portainer-logs logs myapp --grep "order.*failed"

# Combine filters (line must match all)
portainer-logs logs myapp --since 1h --level error --grep "timeout"

# Strip timestamps
portainer-logs logs myapp --no-timestamps
```

## Global Options

| Option | Description |
|--------|-------------|
| `--instance <key>` | Override the default server for this invocation |
| `--format plain\|json` | Output format (default: `plain`) |
| `--fuzzy` | Enable fuzzy name resolution for this invocation |
| `--no-fuzzy` | Require exact name match for this invocation |
| `--env <id>` | Target a specific Portainer environment |
| `--version` | Print the tool version |
| `--help` | Show help for any command |

## Configuration

Settings are managed via CLI commands and stored at `~/.portainer-logs/config.json`.

```bash
portainer-logs config set fuzzy-match false    # Disable fuzzy container/stack name matching
portainer-logs config set default-tail 500     # Default log lines returned
portainer-logs config set default-format json  # Default output format
```

## Fuzzy Name Resolution

Container and stack names in Docker Compose include numeric suffixes (e.g. `myapp-api-1`). With fuzzy matching enabled (the default), you can type just `myapp-api` and the tool resolves to the full name. If multiple containers match, the tool exits with the list of candidates. The resolved name is printed to stderr so it doesn't interfere with piped output.

## JSON Output

Every command supports `--format json` for machine-readable output, making it straightforward to pipe into `jq` or consume from scripts and AI agents.

```bash
portainer-logs containers --format json | jq '.[].name'
portainer-logs logs myapp --format json | jq '.[] | select(.stream == "stderr")'
```

## Using with Claude Code

`portainer-logs` is designed to work as a subprocess for AI coding assistants. There are two ways to integrate it with Claude Code:

- **[Passive awareness](docs/claude-code-passive.md)** — Add an entry to your `CLAUDE.md` so Claude Code knows about the tool in every session and can reach for it when container issues come up.
- **[Diagnostic skill](docs/claude-code-skill.md)** — Create a `/diagnose-containers` slash command that runs a structured triage workflow on demand.

## License

[BSD 3-Clause](LICENSE)
