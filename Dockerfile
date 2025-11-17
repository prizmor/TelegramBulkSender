FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["TelegramBulkSender.API/TelegramBulkSender.API.csproj", "TelegramBulkSender.API/"]
RUN dotnet restore "TelegramBulkSender.API/TelegramBulkSender.API.csproj"
COPY . .
WORKDIR "/src/TelegramBulkSender.API"
RUN dotnet build -c Release -o /app/build

FROM build AS publish
RUN dotnet publish -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TelegramBulkSender.API.dll"]
