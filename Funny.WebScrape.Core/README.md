# Funny.WebScrape.Core

Библиотека для извлечения основного контента из HTML и конвертации в Markdown. Не содержит сетевых зависимостей — только парсинг и конвертация.

## Установка

```bash
dotnet add package Funny.WebScrape.Core --source https://nuget.pkg.github.com/fan92rus/index.json
```

## Компоненты

### `SmartReader`

Извлекает основной контент статьи из HTML (readability-подобный алгоритм на AngleSharp). Автоматически детектирует форумы (`.post_body`, `.post-body`) и выбирает лучший кандидат по скорингу.

```csharp
using Funny.WebScrape.Converters;

var reader = new SmartReader();
var articleHtml = reader.ExtractArticleContent(html);
```

### `HtmlToMarkdownConverter`

Оркестрирует извлечение → конвертацию через [mdream](https://github.com/harlan-zw/mdream) (нативный Rust через P/Invoke).

```csharp
using Funny.WebScrape.Converters;

var converter = new HtmlToMarkdownConverter();
var markdown = await converter.ConvertAsync(html, url: "https://example.com", mode: ConversionMode.Article);
```

**Режимы:**
- `ConversionMode.Article` — извлечение основного контента через `SmartReader`
- `ConversionMode.FullPage` — конвертация всего HTML

### DI

```csharp
using Funny.WebScrape;

services.AddWebScrapeCore();
```

Регистрирует `HtmlToMarkdownConverter` как singleton.

## Зависимости

- AngleSharp
- MdreamWrapper
- Microsoft.Extensions.DependencyInjection
