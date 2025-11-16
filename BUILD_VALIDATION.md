# Валидация проекта без компиляции

## 1. Проверка синтаксиса C#
Используйте удалённое окружение с dotnet 8.0:
- GitHub Codespaces
- GitHub Actions (workflow build.yml)
- Локальный контейнер с `mcr.microsoft.com/dotnet/sdk:8.0`

## 2. Проверка структуры проекта
### Обязательные файлы
- [x] TelegramBulkSender.API/TelegramBulkSender.API.csproj
- [x] Program.cs
- [x] appsettings.json
- [x] Data/ApplicationDbContext.cs
- [x] Models (11 файлов)
- [x] Services (10+ файлов)
- [x] Middleware/JwtMiddleware.cs
- [x] Pages (Login, Chats, ChatGroups, Broadcast, BroadcastHistory, Templates, Users, UserLogs, Index)
- [x] Dockerfile, docker-compose.yml, .env.example, README.md
- [x] CODE_REVIEW_REPORT.md, BUILD_VALIDATION.md, NEXT_STEPS.md, CHANGES.md

## 3. Статическая проверка зависимостей
Убедитесь, что `TelegramBulkSender.API.csproj` содержит:
- WTelegramClient 3.6.4
- Microsoft.EntityFrameworkCore 8.0.0
- Pomelo.EntityFrameworkCore.MySql 8.0.0
- Microsoft.EntityFrameworkCore.Design 8.0.0
- Microsoft.EntityFrameworkCore.Tools 8.0.0
- Serilog.AspNetCore 8.0.0
- Serilog.Sinks.File 5.0.0
- BCrypt.Net-Next 4.0.3
- System.IdentityModel.Tokens.Jwt 7.0.3
- Microsoft.AspNetCore.Authentication.JwtBearer 8.0.0

## 4. Проверка конфигурации
### appsettings.json должен содержать
- ConnectionStrings.DefaultConnection
- Jwt.Secret (>=32 символов), Issuer, Audience, TTL
- Telegram.ApiId, ApiHash, PhoneNumber, SessionPath
- Serilog.WriteTo с File sink

### .env.example должен содержать
- TELEGRAM_API_ID
- TELEGRAM_API_HASH
- TELEGRAM_PHONE
- ROOT_PASSWORD
- JWT_SECRET
- MYSQL_CONNECTION
- MYSQL_ROOT_PASSWORD

## 5. Проверка Docker
### Dockerfile
- Multi-stage (sdk + aspnet)
- EXPOSE 80
- ENTRYPOINT `dotnet TelegramBulkSender.API.dll`

### docker-compose.yml
- Сервисы app + mysql
- Volumes: telegram.session, logs, uploads
- env_file: .env
- depends_on: mysql

## 6. Ручная проверка логики
### Program.cs
1. Сервисы зарегистрированы (DbContext, AuthService, TelegramService, HostedService и т.д.)
2. JWT берёт секрет из `Jwt:Secret` или `JWT_SECRET`
3. Антифрод: Antiforgery + RateLimiter
4. Порядок middleware: Static -> Routing -> RateLimiter -> Authentication -> Authorization -> JwtMiddleware -> Endpoints
5. Авто-миграции выполняются перед запуском

### ApplicationDbContext.cs
1. DbSet для всех сущностей
2. Индексы на Username, TelegramChatId, RefreshToken
3. Все FK настроены с DeleteBehavior.Cascade
4. Root пользователь seed

### TelegramService.cs
1. Клиент лениво создаётся и переиспользуется
2. FloodWaitException ловится с задержкой
3. Сохранение чатов в БД
4. Отправка документов/сообщений учитывает флаги isImage

### JwtMiddleware.cs
1. Валидирует access token
2. Читает refresh token и выдаёт новый access token
3. Записывает UserId/UserRole в `HttpContext.Items`

## 7. Проверка безопасности
- [x] BCrypt для паролей
- [x] JWT secret не хранится в коде
- [x] .env в .gitignore
- [x] Antiforgery включён
- [x] [Authorize] на всех приватных страницах
- [x] Root проверки в Users/UserLogs

## 8. Чек-лист готовности
### Компиляция (выполняется в окружении с .NET)
1. `dotnet restore TelegramBulkSender.API/TelegramBulkSender.API.csproj`
2. `dotnet build TelegramBulkSender.API/TelegramBulkSender.API.csproj --configuration Release`
3. `dotnet ef migrations add InitialCreate`
4. `dotnet ef database update`

### Runtime
1. `dotnet run --project TelegramBulkSender.API`
2. Проверка логов Serilog
3. Ввод кода Telegram при первом запуске
4. Логин root / проверка JWT cookie

### Docker
1. `cp .env.example .env` и заполнить
2. `docker-compose build`
3. `docker-compose up -d`
4. `docker-compose logs -f app`
5. Проверка volume `/app/telegram.session`

## 9. GitHub Actions (опционально)
Создайте `.github/workflows/build.yml` с шагами restore/build и прогоняйте при каждом push.

## 10. Troubleshooting
- **Unable to connect to MySQL** – проверьте MYSQL_CONNECTION и переменные docker-compose.
- **IDX10503: Signature validation failed** – секрет в .env должен совпадать с Jwt:Secret.
- **Request body too large** – убедитесь, что FormOptions и Kestrel лимиты применены (2 ГБ).
- **FloodWait** – TelegramService автоматически ждёт указанное время; мониторьте Serilog warning.
