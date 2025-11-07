FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# ПОЛНОЕ ОТКЛЮЧЕНИЕ GIT
ENV DOTNET_DISABLE_GIT_TASKS=true
ENV DOTNET_CLI_TELEMETRY_OPTOUT=1
ENV BuildWithGit=false
ENV EnableSourceControlManagerQueries=false
ENV EnableSourceLink=false

# Копируем и восстанавливаем КАЖДЫЙ ПРОЕКТ ОТДЕЛЬНО
COPY ArmNavigation.Host/ArmNavigation.Host.csproj ./ArmNavigation.Host/
RUN dotnet restore ArmNavigation.Host/ArmNavigation.Host.csproj /p:BuildWithGit=false

COPY ArmNavigation.Domain/ArmNavigation.Domain.csproj ./ArmNavigation.Domain/
RUN dotnet restore ArmNavigation.Domain/ArmNavigation.Domain.csproj /p:BuildWithGit=false

COPY ArmNavigation.Presentation/ArmNavigation.Presentation.csproj ./ArmNavigation.Presentation/
RUN dotnet restore ArmNavigation.Presentation/ArmNavigation.Presentation.csproj /p:BuildWithGit=false

COPY ArmNavigation.Infrastructure.Postgres/ArmNavigation.Infrastructure.Postgres.csproj ./ArmNavigation.Infrastructure.Postgres/
RUN dotnet restore ArmNavigation.Infrastructure.Postgres/ArmNavigation.Infrastructure.Postgres.csproj /p:BuildWithGit=false

COPY ArmNavigation.Infrastructure.Migrator/ArmNavigation.Infrastructure.Migrator.csproj ./ArmNavigation.Infrastructure.Migrator/
RUN dotnet restore ArmNavigation.Infrastructure.Migrator/ArmNavigation.Infrastructure.Migrator.csproj /p:BuildWithGit=false

COPY ArnNavigation.Application/ArnNavigation.Application.csproj ./ArnNavigation.Application/
RUN dotnet restore ArnNavigation.Application/ArnNavigation.Application.csproj /p:BuildWithGit=false

# Копируем весь код и публикуем
COPY . .
RUN dotnet publish ArmNavigation.Host/ArmNavigation.Host.csproj -c Release -o /app /p:BuildWithGit=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app .
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 8080
ENTRYPOINT ["dotnet", "ArmNavigation.Host.dll"]
