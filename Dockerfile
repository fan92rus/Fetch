FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

ARG GITHUB_TOKEN
COPY nuget.config .

RUN if [ -n "$GITHUB_TOKEN" ]; then \
      dotnet nuget add source --name github "https://nuget.pkg.github.com/fan92rus/index.json" \
        --username fan92rus --password "$GITHUB_TOKEN" --store-password-in-clear-text; \
    fi

COPY Fetch.Server/Fetch.Server.csproj Fetch.Server/
COPY Fetch.Cli/Fetch.Cli.csproj Fetch.Cli/
COPY Funny.WebScrape/Funny.WebScrape.csproj Funny.WebScrape/
COPY RestSharp.Extensions/RestSharp.Polly.Extensions.Core.csproj RestSharp.Extensions/
COPY SimHash/SimHash.csproj SimHash/
COPY UniversalProductScraper.sln .

RUN dotnet restore

COPY . .
RUN dotnet publish Fetch.Server/Fetch.Server.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/runtime:9.0 AS runtime

RUN apt-get update && apt-get install -y --no-install-recommends \
    chromium \
    && rm -rf /var/lib/apt/lists/*

ENV CHROME_BIN=/usr/bin/chromium
ENV REMOTE_BROWSER_URL=

WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 5020

ENTRYPOINT ["dotnet", "Fetch.Server.dll"]
