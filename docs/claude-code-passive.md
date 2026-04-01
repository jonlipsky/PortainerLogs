# Making portainer-logs available to all Claude Code sessions

If you want Claude Code to know about `portainer-logs` in every conversation — so it can reach for it whenever container issues come up — add a brief entry to your `CLAUDE.md` file.

## How it works

Claude Code reads `CLAUDE.md` files at the start of every conversation. Content in these files acts as persistent context — instructions, tool awareness, project conventions — that Claude carries throughout the session.

There are two levels:

| File | Scope |
|------|-------|
| `~/.claude/CLAUDE.md` | Every Claude Code session on your machine |
| `<project>/CLAUDE.md` | Sessions rooted in that project directory |

Since `portainer-logs` is a globally installed dotnet tool useful across any project, the user-level file is the right place.

## Setup

Add the following to `~/.claude/CLAUDE.md` (create the file if it doesn't exist):

```markdown
## Available Tools

### portainer-logs
A read-only CLI tool for inspecting containerised infrastructure across Portainer instances.
Installed as a dotnet global tool: `portainer-logs`

Common commands:
- `portainer-logs containers` — list running containers
- `portainer-logs logs <container> --since 1h --level error` — fetch filtered logs
- `portainer-logs stacks list` — list deployed stacks
- `portainer-logs inspect <container>` — show container config
- `portainer-logs stats <container>` — CPU/memory/network snapshot
- `portainer-logs events --since 2h` — recent Docker events (crashes, OOM kills)

All commands support `--format json` for machine-readable output.
Fuzzy container name matching is enabled by default.
```

## What this gives you

With this entry in place, Claude Code will:

- Know that `portainer-logs` exists and what it can do
- Reach for it when you mention container issues, log errors, or deployment problems
- Use `--format json` to parse output programmatically
- Use fuzzy matching so it doesn't need exact container names

## Trade-offs

The entry is loaded into context for every session, even when you're not debugging containers. If you'd prefer to only load this context on demand, consider using a [Claude Code skill](claude-code-skill.md) instead.
