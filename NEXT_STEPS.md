# Следующие шаги

## 1. Локальная проверка (если доступен .NET SDK)
1. `cd TelegramBulkSender.API`
2. `dotnet restore`
3. `dotnet build`
4. `dotnet ef migrations add InitialCreate`
5. Проверить миграцию, затем `dotnet ef database update`
6. Настроить MySQL (docker-compose или локально)
7. `dotnet run` и убедиться, что Root может войти

## 2. Проверка через GitHub Codespaces
1. Создайте codespace из репозитория
2. Выполните команды из пункта 1
3. Подтвердите отсутствие ошибок компиляции

## 3. Docker проверка
1. `cp .env.example .env` и заполните значениями
2. `docker-compose build`
3. `docker-compose up -d`
4. `docker-compose logs -f app`
5. Убедитесь, что volumes (telegram.session, logs, uploads) созданы

## 4. Первый запуск Telegram клиента
1. На первом запуске клиент попросит код подтверждения
2. Введите код из официального клиента Telegram
3. Убедитесь, что файл `telegram.session` сохраняется в volume
4. Перезапустите контейнер/приложение при необходимости

## 5. Тестирование функционала
1. Зайдите на `/Login` и авторизуйтесь под root
2. Проверьте страницы Chats, ChatGroups, Broadcast, Templates, Users, UserLogs
3. Создайте тестовый шаблон, группу и рассылку (в системный чат)
4. Просмотрите историю рассылок и логи пользователей

## 6. Мониторинг и поддержка
1. Контролируйте логи `logs/app-*.log`
2. Следите за `docker-compose logs app` и `docker-compose logs mysql`
3. Регулярно чистите `/app/uploads` после завершённых рассылок
4. Обновляйте Root пароль в `.env` и перезапускайте приложение при ротации
