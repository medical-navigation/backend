FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore ArmNavigation.Host/ArmNavigation.Host.csproj
RUN dotnet publish ArmNavigation.Host/ArmNavigation.Host.csproj -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app .

# ОТКЛЮЧАЕМ HTTPS ДЛЯ ПРОДКШЕНА
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

EXPOSE 8080
ENTRYPOINT ["dotnet", "ArmNavigation.Host.dll"]
