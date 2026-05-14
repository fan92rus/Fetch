# Fetch

Веб-сервис для конвертации HTML-страниц в Markdown. Использует [mdream](https://github.com/harlan-zw/mdream) (нативный Rust через P/Invoke) для конвертации и AngleSharp для извлечения контента статей.

## Состав

| Проект | Описание |
|--------|----------|
| [Fetch.Server](Fetch.Server/README.md) | Бэкенд — EmbedIO HTTP-сервер (порт 5020) |
| [Fetch.Cli](Fetch.Cli/README.md) | dotnet tool для CLI-доступа к API |
| [Funny.WebScrape.Core](Funny.WebScrape.Core/README.md) | Извлечение контента + конвертация HTML→MD (без сетевых зависимостей) |
| [Funny.WebScrape](Funny.WebScrape/README.md) | Загрузка страниц + конвертация HTML→MD (включает Core) |
| [RestSharp.Extensions](RestSharp.Extensions/README.md) | Расширения RestSharp + Polly |

## API

### `GET /parse/url`

| Параметр | Значение | Описание |
|----------|----------|----------|
| `url` | string | URL страницы |
| `loadingType` | `HttpRequest` \| `Selenium` | Способ загрузки |
| `mode` | `Article` \| `FullPage` | Режим конвертации (по умолчанию `Article`) |

**Режимы:**
- `Article` — извлечение основного контента (AngleSharp.ContentExtraction), затем mdream
- `FullPage` — конвертация всей страницы через mdream

**Пример:**
```bash
curl "http://localhost:5020/parse/url?url=https://example.com&loadingType=HttpRequest&mode=Article"
```

**Ответ:**
```json
{"content": "# Example Domain\n\nThis domain is for use in illustrative examples..."}
```

## CLI

### Установка

```bash
dotnet tool install --global Fetch.Cli --add-source https://nuget.pkg.github.com/fan92rus/index.json
```

### Использование

```bash
# Статья (по умолчанию)
fetch https://example.com

# Полная страница
fetch https://example.com -m full-page

# Через Selenium (для JS-рендеринга)
fetch https://example.com -l Selenium

# Другой сервер
fetch https://example.com -s http://myserver:5020
```

**Опции:**
| Флаг | Описание |
|------|----------|
| `-m, --mode` | `article` (по умолчанию) или `full-page` |
| `-l, --loading-type` | `HttpRequest` (по умолчанию) или `Selenium` |
| `-s, --server` | URL бэкенда (по умолчанию `http://localhost:5020`) |

## Docker

```bash
docker build -t fetch-server .
docker run -p 5020:5020 fetch-server
```

С переменной `REMOTE_BROWSER_URL` можно подключить удалённый Chrome (Selenoid и т.п.):
```bash
docker run -p 5020:5020 -e REMOTE_BROWSER_URL=http://selenoid:4444/wd/hub fetch-server
```

Образ также доступен в GHCR:
```bash
docker pull ghcr.io/fan92rus/fetch:latest
```

## Разработка

Требования: .NET 9 SDK, [MdreamWrapper](https://github.com/fan92rus/mdream-wrapper) из GitHub Packages.

```bash
# Восстановление зависимостей
dotnet restore

# Сборка
dotnet build

# Запуск сервера
dotnet run --project Fetch.Server
```

Для NuGet-аутентификации в GitHub Packages нужен `nuget.config` с токеном (уже в репозитории для CI).
