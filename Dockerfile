# Use the official .NET SDK image as the base
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src

# Copy all project files for multi-project solution
COPY UniversalProductScraper.sln .
COPY RestSharp.Extensions/RestSharp.Polly.Extensions.Core.csproj RestSharp.Extensions/
COPY SimHash/SimHash.csproj SimHash/
COPY UniversalProductScraper/UniversalProductScraper.csproj UniversalProductScraper/

# Restore dependencies for the entire solution
RUN dotnet restore UniversalProductScraper.sln -s https://api.nuget.org/v3/index.json --packages /packages

# Build each project in the solution
RUN dotnet build RestSharp.Extensions/RestSharp.Polly.Extensions.Core.csproj -c Release -o /app/build/RestSharp.Extensions
RUN dotnet build SimHash/SimHash.csproj -c Release -o /app/build/SimHash
RUN dotnet build UniversalProductScraper.sln -c Release -o /app/build/UniversalProductScraper

# Publish each project in the solution
FROM build AS publish
RUN dotnet publish RestSharp.Extensions/RestSharp.Polly.Extensions.Core.csproj -c Release -o /app/publish/RestSharp.Extensions
RUN dotnet publish SimHash/SimHash.csproj -c Release -o /app/publish/SimHash
RUN dotnet publish UniversalProductScraper.sln -c Release -o /app/publish/UniversalProductScraper

# Copy all published projects to the final image

# Use the ASP.NET runtime image for running the app
FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS final
WORKDIR /app

# Copy all published applications from the publish container
COPY --from=publish /app/publish/RestSharp.Extensions /app/RestSharp.Extensions
COPY --from=publish /app/publish/SimHash /app/SimHash
COPY --from=publish /app/publish/UniversalProductScraper .

# Expose port 5020 as specified by the user
EXPOSE 5020/tcp

# Set environment variable for .NET runtime
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=0

# Set the working directory and entry point to run the UniversalProductScraper application
WORKDIR /app/UniversalProductScraper
ENTRYPOINT ["dotnet", "UniversalProductScraper.dll"]
