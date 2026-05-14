# Funny.WebScrape

Библиотека для загрузки веб-страниц и конвертации HTML → Markdown. Включает `Funny.WebScrape.Core` транзитивно.

## Установка

```bash
dotnet add package Funny.WebScrape --source https://nuget.pkg.github.com/fan92rus/index.json
```

## Загрузчики

### `HttpRequestLoader`

Базовая загрузка через `HttpClient` с повторными попытками (Polly).

### `SeleniumLoader`

Загрузка через Selenium WebDriver для JS-рендеринга. Поддерживает удалённый браузер через `REMOTE_BROWSER_URL`.

### `RequestWebLoader` / `tls-client-sharp`

Расширенные загрузчики с обходом fingerprinting.

## DI

```csharp
using Funny.WebScrape;

services.AddWebScrapingServices();
```

Регистрирует:
- `HtmlToMarkdownConverter` (из Core)
- `ILoaderFactory` → `LoaderFactory`

## Использование

```csharp
var loaderFactory = serviceProvider.GetRequiredService<ILoaderFactory>();
var converter = serviceProvider.GetRequiredService<HtmlToMarkdownConverter>();

var loader = loaderFactory.CreateLoader(LoadingType.HttpRequest);
var html = await loader.GetPageContentAsync("https://example.com");
var markdown = await converter.ConvertAsync(html, "https://example.com", ConversionMode.Article);
```

## Зависимости

- Funny.WebScrape.Core (транзитивно)
- RestSharp
- Polly
- Selenium.WebDriver
- tls-client-sharp
