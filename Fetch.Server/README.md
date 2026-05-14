# Fetch.Server

HTTP-сервер на EmbedIO для загрузки и конвертации веб-страниц в Markdown.

## Запуск

```bash
dotnet run --project Fetch.Server
```

Сервер слушает `http://*:5020`.

## API

### `GET /parse/url`

| Параметр | Обязательный | Значение | Описание |
|----------|--------------|----------|----------|
| `url` | да | string | URL страницы |
| `loadingType` | да | `HttpRequest` \| `Selenium` | Способ загрузки |
| `mode` | нет | `Article` (default) \| `FullPage` | Режим конвертации |
| `images` | нет | `false` (default) \| `true` | Включать изображения в Markdown |

**Пример:**
```bash
curl "http://localhost:5020/parse/url?url=https://example.com&loadingType=HttpRequest&mode=Article"
```

**Ответ:**
```json
{"content": "# Example Domain\n\nThis domain is for use in illustrative examples..."}
```

## Docker

```bash
docker build -t fetch-server .
docker run -p 5020:5020 fetch-server
```

Удалённый Chrome (Selenoid и т.п.):
```bash
docker run -p 5020:5020 -e REMOTE_BROWSER_URL=http://selenoid:4444/wd/hub fetch-server
```

## Зависимости

- Funny.WebScrape
- EmbedIO
- WebDriverManager
