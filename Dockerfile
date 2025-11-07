FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Копируем существующие проекты
COPY ArmNavigation.Host/ArmNavigation.Host.csproj ./ArmNavigation.Host/
COPY ArmNavigation.Domain/ArmNavigation.Domain.csproj ./ArmNavigation.Domain/
COPY ArmNavigation.Presentation/ArmNavigation.Presentation.csproj ./ArmNavigation.Presentation/
COPY ArmNavigation.Infrastructure.Postgres/ArmNavigation.Infrastructure.Postgres.csproj ./ArmNavigation.Infrastructure.Postgres/
COPY ArmNavigation.Infrastructure.Migrator/ArmNavigation.Infrastructure.Migrator.csproj ./ArmNavigation.Infrastructure.Migrator/
COPY ArnNavigation.Application/ArnNavigation.Application.csproj ./ArnNavigation.Application/

# Восстанавливаем каждый проект отдельно
RUN dotnet restore ArmNavigation.Host/ArmNavigation.Host.csproj
RUN dotnet restore ArmNavigation.Domain/ArmNavigation.Domain.csproj  
RUN dotnet restore ArmNavigation.Presentation/ArmNavigation.Presentation.csproj
RUN dotnet restore ArmNavigation.Infrastructure.Postgres/ArmNavigation.Infrastructure.Postgres.csproj
RUN dotnet restore ArmNavigation.Infrastructure.Migrator/ArmNavigation.Infrastructure.Migrator.csproj
RUN dotnet restore ArnNavigation.Application/ArnNavigation.Application.csproj

# Копируем весь код и публикуем
COPY . .
RUN dotnet publish ArmNavigation.Host/ArmNavigation.Host.csproj -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "ArmNavigation.Host.dll"]
