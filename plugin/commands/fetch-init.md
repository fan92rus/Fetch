---
description: Configure Fetch.Cli defaults and create config file
---

Interactive setup for Fetch.Cli. Creates or updates the config file with your preferred defaults.

**Step 1: Verify fetch is installed**
```bash
fetch --help
```
If not found, install it:
```bash
dotnet tool install --global Fetch.Cli --add-source https://nuget.pkg.github.com/fan92rus/index.json
```

**Step 2: Ask the user for preferences**

Ask each question one at a time. If the user presses Enter without typing anything, use the default value.

| Setting | Question | Default |
|---------|----------|---------|
| `Server` | Fetch.Server URL (e.g. http://localhost:5020) | `http://localhost:5020` |
| `Images` | Include images by default? (true/false) | `false` |

**Step 3: Write config file**

Write the collected values to `~/.config/fetch.cli/config.json` (`%USERPROFILE%\.config\fetch.cli\config.json` on Windows).

Only include fields that the user explicitly set or that have non-null defaults. Omit empty / null values.

Example output:
```json
{
  "Server": "http://localhost:5020",
  "Images": false
}
```

**Step 4: Confirm**

Show the user the final path and file contents, and confirm it was written successfully.

```bash
fetch config
```
