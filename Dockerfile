FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and project files first for layer caching
COPY ControlCenter.sln .
COPY src/ControlCenter.Api/ControlCenter.Api.csproj src/ControlCenter.Api/
RUN dotnet restore

# Copy everything else and build
COPY . .
RUN dotnet publish src/ControlCenter.Api/ControlCenter.Api.csproj -c Release -o /app/publish --no-restore

# Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

RUN adduser --disabled-password --gecos "" appuser
USER appuser

COPY --from=build /app/publish .

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "ControlCenter.Api.dll"]
