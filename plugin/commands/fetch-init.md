---
description: Configure Fetch.Cli defaults and ensure CLAUDE.md has fetch rules
---

Interactive setup for Fetch.Cli. Checks existing config and ensures the agent always uses `fetch` for web pages.

**Step 1: Verify fetch is installed**
```bash
fetch --help
```
If not found, install it:
```bash
dotnet tool install --global Fetch.Cli --add-source https://nuget.pkg.github.com/fan92rus/index.json
```

**Step 2: Check existing config**
```bash
fetch config
```
If config is already set (server and images have values the user is happy with), skip to Step 4. Otherwise, ask the user for preferences one at a time.

| Setting | Question | Default |
|---------|----------|---------|
| `Server` | Fetch.Server URL (e.g. http://localhost:5020) | `http://localhost:5020` |
| `Images` | Include images by default? (true/false) | `false` |

**Step 3: Write config file (only if values changed)**

Write to `~/.config/fetch.cli/config.json` (`%USERPROFILE%\.config\fetch.cli\config.json` on Windows).

Only include fields that the user explicitly set or that have non-null defaults.

```bash
fetch config set server=<url>
fetch config set images=<true|false>
```

**Step 4: Ensure CLAUDE.md has fetch rules (ALWAYS do this)**

Check if the project has a CLAUDE.md file. If it exists, check if it already contains a `fetch` rule. If not, append the following section:

```markdown
## Web Page Reading

Always use the `fetch` CLI tool to read or convert web pages to Markdown. Do NOT use WebSearch, webReader, curl, or any other method to retrieve page content.

```bash
fetch "<url>"                    # Article mode, no images (default)
fetch "<url>" --images           # Include images
fetch "<url>" -m full-page       # Full page conversion
fetch "<url>" -l Selenium        # JS rendering via FlareSolverr
```
```

If CLAUDE.md does not exist, create it with this section.

Confirm to the user that the rule was added or was already present.

**Step 5: Confirm everything**

```bash
fetch config
```
Show the user the final config and confirm CLAUDE.md status.
