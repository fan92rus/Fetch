# Fetch.Cli

dotnet global tool для CLI-доступа к Fetch.Server.

## Установка

```bash
dotnet tool install --global Fetch.Cli --add-source https://nuget.pkg.github.com/fan92rus/index.json
```

## Использование

```bash
# Статья (по умолчанию)
fetch https://example.com

# Полная страница
fetch https://example.com -m full-page

# Через Selenium (для JS-рендеринга)
fetch https://example.com -l Selenium

# С изображениями
fetch https://example.com --images

# Другой сервер
fetch https://example.com -s http://myserver:5020
```

## Опции

| Флаг | Описание |
|------|----------|
| `-m, --mode` | `article` (default) или `full-page` |
| `-l, --loading-type` | `HttpRequest` (default) или `Selenium` |
| `-s, --server` | URL бэкенда (default: из конфига или `http://localhost:5020`) |
| `--images` | Включить изображения в вывод |
| `-h, --help` | Справка |

## Конфигурация

```bash
# Показать текущую конфигурацию
fetch config

# Установить сервер по умолчанию
fetch config set server=http://myserver:5020

# Включить изображения по умолчанию
fetch config set images=true
```

Конфиг хранится в `~/.config/fetch.cli/config.json`.
