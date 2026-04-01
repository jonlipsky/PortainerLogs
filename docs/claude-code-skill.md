# Creating a diagnostic skill for Claude Code

If you'd rather invoke `portainer-logs` through a structured diagnostic workflow — only when you're actively debugging — you can package it as a Claude Code skill (custom slash command).

## How skills work

Skills are markdown files in `~/.claude/commands/` that expand into prompts when invoked. They're loaded on demand, not at session start, so they don't consume context until you need them.

```
~/.claude/commands/diagnose-containers.md   →   /diagnose-containers
```

When you type `/diagnose-containers myapp-api is returning 500s`, Claude Code reads the skill file and follows its instructions using the description you provided as context.

## Setup

Create `~/.claude/commands/diagnose-containers.md` with the following content:

````markdown
---
description: Diagnose production container issues using portainer-logs CLI
argument-hint: "<symptom, container name, or issue description>"
---

# Container Diagnostics

Systematically triage production container issues using the `portainer-logs` CLI tool (installed as a dotnet global tool).

## Context

`portainer-logs` is a read-only CLI for inspecting containerised infrastructure across Portainer instances. All commands support `--format json` for structured output. Fuzzy container name matching is enabled by default — you don't need the exact container name.

If no Portainer instance is configured yet, guide the user through `portainer-logs instance add`.

## Diagnostic Workflow

Work through these steps in order. Skip steps that aren't relevant to the reported symptom. Summarise findings after each step before proceeding.

### 1. Identify the target

If the user gave a container name, use it. Otherwise, list what's running to find it:

```
portainer-logs containers --format json
```

If the user mentioned a stack name, list stacks to find related containers:

```
portainer-logs stacks list --format json
```

### 2. Check for crashes, OOM kills, and restarts

This is the fastest way to spot infrastructure-level problems. Look for `exitCode=137` (OOM kill), repeated `die`/`start` cycles (crash loop), and unexpected `stop` events.

```
portainer-logs events --since 2h --container <name> --format json
```

### 3. Pull error logs

Fetch recent errors. Widen the `--since` window if the initial results don't cover the incident timeframe. Use `--grep` to narrow further if needed.

```
portainer-logs logs <container> --since 1h --level error
```

If the user described a specific error or keyword:

```
portainer-logs logs <container> --since 1h --grep "<keyword>"
```

### 4. Check resource pressure

High memory percentage or CPU spikes often explain intermittent failures and OOM kills.

```
portainer-logs stats <container> --format json
```

### 5. Inspect configuration

Check for misconfigured environment variables, missing volume mounts, wrong port bindings, or unhealthy health checks. Only do this if earlier steps suggest a config problem.

```
portainer-logs inspect <container> --format json
```

### 6. Check the deployed compose file

If the issue might be a deployment mismatch (wrong image tag, missing service, changed config), compare what's actually deployed:

```
portainer-logs stacks get <stack-name>
```

## Reporting

After completing the relevant steps, provide:

1. **Root cause** — what's actually wrong (or top 2-3 candidates if uncertain)
2. **Evidence** — the specific log lines, events, or metrics that point to it
3. **Recommended action** — what the user should do next

Keep the report concise. Lead with the answer, not the investigation narrative.
````

## Usage

```
/diagnose-containers myapp-api is returning 500 errors intermittently
/diagnose-containers the monitoring stack keeps restarting
/diagnose-containers high memory usage on the orleans silo
```

Claude Code will work through the diagnostic steps, running `portainer-logs` commands and summarising findings as it goes.

## Choosing between passive and skill approaches

| Approach | Best for |
|----------|----------|
| [CLAUDE.md entry](claude-code-passive.md) | You want Claude to proactively use portainer-logs whenever container issues come up in any conversation |
| Skill (this page) | You want a structured diagnostic workflow that only runs when you explicitly invoke it |

You can use both — the `CLAUDE.md` entry for awareness, and the skill for a thorough triage workflow.
