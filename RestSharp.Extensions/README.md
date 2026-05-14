# RestSharp.Polly.Extensions.Core

Расширения для RestSharp с интеграцией Polly (retry/circuit-breaker).

## Установка

```bash
dotnet add package RestSharp.Polly.Extensions.Core --source https://nuget.pkg.github.com/fan92rus/index.json
```

## Методы

### `ExecuteWithPolicy`

Выполняет запрос через Polly Policy с обработкой исключений.

```csharp
using Extensions.RestSharp;

var policy = Policy
    .HandleResult<RestResponse>(r => !r.IsSuccessful)
    .WaitAndRetryAsync(3, retryAttempt =>
        TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));

var response = client.ExecuteWithPolicy(request, policy);
```

### `ExecuteWitHeaders`

Выполняет запрос с предустановленными браузерными заголовками (Accept, Accept-Language, User-Agent, Origin и др.) и fallback на `X-Requested-With` при `StatusCode == 0`.

```csharp
var response = client.ExecuteWitHeaders(request, policy);
```

## Зависимости

- RestSharp
- Polly
