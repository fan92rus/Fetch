FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

ARG GITHUB_TOKEN
COPY nuget.config .

RUN if [ -n "$GITHUB_TOKEN" ]; then \
      dotnet nuget update source github --username fan92rus --password "$GITHUB_TOKEN" --store-password-in-clear-text; \
    fi

COPY Fetch.Server/Fetch.Server.csproj Fetch.Server/
COPY Fetch.Cli/Fetch.Cli.csproj Fetch.Cli/
COPY Funny.WebScrape.Core/Funny.WebScrape.Core.csproj Funny.WebScrape.Core/
COPY Funny.WebScrape/Funny.WebScrape.csproj Funny.WebScrape/
COPY RestSharp.Extensions/RestSharp.Polly.Extensions.Core.csproj RestSharp.Extensions/
COPY SimHash/SimHash.csproj SimHash/
COPY Fetch.sln .

RUN dotnet restore Fetch.Server/Fetch.Server.csproj

COPY . .
RUN dotnet publish Fetch.Server/Fetch.Server.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/runtime:9.0 AS runtime

WORKDIR /app
COPY --from=build /app/publish .

ENV FLARESOLVERR_URL=http://flaresolverr:8191

EXPOSE 5020

ENTRYPOINT ["dotnet", "Fetch.Server.dll"]
