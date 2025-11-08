Medical Navigation - Бэкенд

Бэкенд приложения на .NET 8.

Локальная разработка:
docker-compose up -d
или
dotnet run --project ArmNavigation.Host

API Endpoints:
- Swagger: http://localhost:8080/swagger
- Health check: http://localhost:8080/health

Docker:
- Development: docker-compose up -d (порт 8080)
- Production: используется через deploy репозиторий (порт 5000)

CI/CD: При пуше в main ветку автоматически деплоится на продакшен.

Структура проекта:
ArmNavigation.Host/ - точка входа
ArmNavigation.Domain/ - доменная логика
ArmNavigation.Presentation/ - контроллеры API
ArmNavigation.Infrastructure.Postgres/ - база данных
ArnNavigation.Application/ - сервисы приложения

Важно:
- Не удалять корневой Dockerfile - используется для продакшена
- Не менять структуру папок без согласования
