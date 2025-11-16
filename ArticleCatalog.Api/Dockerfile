# Этап 1. Runtime-образ (используется final)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081


# Этап 2. Build SDK
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Development
WORKDIR /src

# Копируем основные проекты
COPY ["ArticleCatalog.Api/ArticleCatalog.Api.csproj", "ArticleCatalog.Api/"]
COPY ["ArticleCatalog.Application/ArticleCatalog.Application.csproj", "ArticleCatalog.Application/"]
COPY ["ArticleCatalog.Domain/ArticleCatalog.Domain.csproj", "ArticleCatalog.Domain/"]
COPY ["ArticleCatalog.Infrastructure/ArticleCatalog.Infrastructure.csproj", "ArticleCatalog.Infrastructure/"]

# Копируем тестовые проекты
COPY ["ArticleCatalog.Domain.Tests/ArticleCatalog.Domain.Tests.csproj", "ArticleCatalog.Domain.Tests/"]
COPY ["ArticleCatalog.Application.Tests/ArticleCatalog.Application.Tests.csproj", "ArticleCatalog.Application.Tests/"]
COPY ["ArticleCatalog.Api.Tests/ArticleCatalog.Api.Tests.csproj", "ArticleCatalog.Api.Tests/"]

# Restore
RUN dotnet restore "ArticleCatalog.Api/ArticleCatalog.Api.csproj"

# Копируем весь код
COPY . .

# Build API
WORKDIR "/src/ArticleCatalog.Api"
RUN dotnet build "ArticleCatalog.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build


# Этап 3. Test stage - тестируем ВСЕ проекты
FROM build AS test
WORKDIR /src

# Запуск тестов
RUN dotnet test ArticleCatalog.Domain.Tests/ArticleCatalog.Domain.Tests.csproj --no-build --verbosity normal
RUN dotnet test ArticleCatalog.Application.Tests/ArticleCatalog.Application.Tests.csproj --no-build --verbosity normal
RUN dotnet test ArticleCatalog.Api.Tests/ArticleCatalog.Api.Tests.csproj --no-build --verbosity normal


# Этап 4. Publish API
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
RUN dotnet publish "ArticleCatalog.Api/ArticleCatalog.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false


# Этап 5. Финальный контейнер
FROM base AS final
WORKDIR /app

# Копируем результат публикации
COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "ArticleCatalog.Api.dll"]
