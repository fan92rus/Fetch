---
description: Convert a web page to Markdown via Fetch.Cli
---

Convert a web page to Markdown using the Fetch.Cli tool.

**Step 1: Run fetch**
```bash
fetch "<url>" [--mode <mode>] [--loading-type <type>] [--images] [--server <url>]
```

**Step 2: Analyze result**
- The command outputs the page content as Markdown directly to stdout
- Images are excluded by default; use `--images` to include them
- If error: report the error message from stderr

**Step 3: Suggest next actions**
- If content is truncated or empty: suggest trying `full-page` mode
- If connection fails: suggest checking the server URL or starting Fetch.Server
- If JS-rendered content is missing: suggest using `--loading-type Selenium`

**Examples:**
```bash
fetch "https://example.com"
fetch "https://example.com" --images
fetch "https://example.com" --mode full-page
fetch "https://example.com" --loading-type Selenium
fetch "https://example.com" --server http://myserver:5020
```
