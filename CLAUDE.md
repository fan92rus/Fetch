# Fetch Project

## Agent Rules

When the user asks to read, fetch, or convert a web page to Markdown — always use the `fetch` CLI tool. Do NOT use WebSearch, webReader, curl, or any other method to retrieve page content.

```bash
fetch "<url>"                    # Article mode, no images (default)
fetch "<url>" --images           # Include images
fetch "<url>" -m full-page       # Full page conversion
fetch "<url>" -l Selenium        # JS rendering via FlareSolverr
```

## Web Search

When you need to search the web, use the `searx` skill or the `/searx-search` slash command.
Do not use raw web search tools — always route web searches through SearXNG CLI.
