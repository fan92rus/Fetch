# Fetch.Cli — CLI Reference

## Installation

```bash
dotnet tool install --global Fetch.Cli --add-source https://nuget.pkg.github.com/fan92rus/index.json
```

## Commands

### `fetch <url>`

Convert a web page to Markdown.

**Arguments:**
- `url` (required) — URL of the page to parse

**Options:**
| Option | Short | Type | Default | Description |
|--------|-------|------|---------|-------------|
| `--mode` | `-m` | string | `article` | Conversion mode: `article` or `full-page` |
| `--loading-type` | `-l` | string | `HttpRequest` | Loader: `HttpRequest` or `Selenium` |
| `--server` | `-s` | string | `http://localhost:5020` | Fetch.Server URL |
| `--help` | `-h` | — | — | Show help |

**Examples:**
```bash
fetch https://example.com
fetch https://example.com --mode full-page
fetch https://example.com -m full-page -l Selenium
fetch https://example.com --server http://myserver:5020
```

### `fetch config`

Show current configuration.

```bash
fetch config
```

### `fetch config set server=<url>`

Set the default Fetch.Server URL.

```bash
fetch config set server=http://myserver:5020
fetch config set server http://myserver:5020
```

## Configuration File

**Path (Windows):** `%APPDATA%\fetch.cli\config.json`
**Path (Linux/macOS):** `~/.config/fetch.cli/config.json`

**Schema:**
```json
{
  "Server": "string?"
}
```

## Environment Variables

None. Use config file or CLI flags.

## Exit Codes

| Code | Meaning |
|------|---------|
| 0 | Success |
| 1 | Error (connection, server, parsing) |
