---
name: fetch
description: >
  HTML to Markdown converter via Fetch.Cli. Fetch web pages and convert them
  to clean Markdown. Supports article extraction and full-page conversion.
  Images are excluded by default; use --images to include them.
  TRIGGER: When the user asks to read a web page, convert HTML to Markdown,
  fetch page content, or extract article text from a URL.
allowed-tools: Bash(fetch)
version: "1.2.0"
---

# Fetch.Cli — HTML to Markdown

Lightweight CLI for converting web pages to Markdown via Fetch.Server. Output is clean Markdown — ideal for AI agents.

## Prerequisites

```bash
fetch --version
```

If not installed:
```bash
dotnet tool install --global Fetch.Cli --add-source https://nuget.pkg.github.com/fan92rus/index.json
```

Requires a running Fetch.Server instance (default: `http://localhost:5020`).

## Basic Usage

```bash
fetch "https://example.com"
```

## Conversion Modes

**Article mode (default)** — extracts main content:
```bash
fetch "https://example.com" --mode article
```

**Full-page mode** — converts entire page:
```bash
fetch "https://example.com" --mode full-page
```

Short flag:
```bash
fetch "https://example.com" -m full-page
```

## Images

Images are **excluded by default** for minimal token usage. Include them with:
```bash
fetch "https://example.com" --images
```

Set images as default in config:
```bash
fetch config set images=true
```

## Loading Types

**HttpRequest (default)** — fast, no JS rendering:
```bash
fetch "https://example.com" --loading-type HttpRequest
```

**Selenium** — full browser rendering for JS-heavy pages:
```bash
fetch "https://example.com" --loading-type Selenium
```

Short flag:
```bash
fetch "https://example.com" -l Selenium
```

## Custom Server

```bash
fetch "https://example.com" --server http://myserver:5020
```

Short flag:
```bash
fetch "https://example.com" -s http://myserver:5020
```

## Configuration File

Set defaults in `~/.config/fetch.cli/config.json`:

```json
{
  "Server": "http://localhost:5020",
  "Images": false
}
```

Priority: CLI flags > config file > built-in defaults.

**Interactive setup:**
```bash
# Via Claude Code slash command
/fetch-init
```

**Manual config:**
```bash
fetch config set server=http://myserver:5020
fetch config set images=true
```

**View current config:**
```bash
fetch config
```

## Output Format

Plain Markdown text written to stdout. Suitable for direct use in AI context.

## Error Handling

| Error | Fix |
|-------|-----|
| `Server error (502)` | Fetch.Server is not running. Start it with `dotnet run --project Fetch.Server` |
| `Connection refused` | Check server URL and network connectivity |
| `Server returned empty response` | Page may be empty or blocked. Try `--mode full-page` |
| Empty JS-rendered content | Use `--loading-type Selenium` with a running browser |

## Best Practices

- **Use article mode** by default — cleaner, more relevant content
- **Exclude images** by default — saves tokens for AI context
- **Use full-page mode** when you need navigation, sidebars, or complete page structure
- **Use Selenium** only when page content requires JavaScript rendering
- **Keep server running** — Fetch.Server must be accessible for the CLI to work

## CLI Reference

See [CLI_REFERENCE.md](${CLAUDE_SKILL_DIR}/resources/CLI-REFERENCE.md) for complete documentation.
