# Используем многоступенчатую сборку
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Копируем и восстанавливаем зависимости
COPY BlockchainApp/BlockchainApp.csproj BlockchainApp/
COPY BlockchainApp.BusinessLogic/BlockchainApp.BusinessLogic.csproj BlockchainApp.BusinessLogic/
COPY BlockchainApp.Domain/BlockchainApp.Domain.csproj BlockchainApp.Domain/
COPY BlockchainApp.Infrastructure/BlockchainApp.Infrastructure.csproj BlockchainApp.Infrastructure/

RUN dotnet restore "BlockchainApp/BlockchainApp.csproj"

# Копируем весь исходный код
COPY . .

# Собираем проект
WORKDIR "/src/BlockchainApp"
RUN dotnet build "BlockchainApp.csproj" -c Release -o /app/build

# Публикуем проект
FROM build AS publish
RUN dotnet publish "BlockchainApp.csproj" -c Release -o /app/publish

# Финальный образ
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Копируем опубликованные файлы
COPY --from=publish /app/publish .

# Копируем конфигурационные файлы
COPY BlockchainApp/appsettings.json ./appsettings.json

# Копируем скрипт для запуска
COPY entrypoint.sh ./entrypoint.sh
RUN chmod +x ./entrypoint.sh

# Переменные окружения по умолчанию
ENV APP_MODE=api

# Открываем порты
EXPOSE 80
EXPOSE 443
EXPOSE 5049

# Запускаем скрипт
ENTRYPOINT ["./entrypoint.sh"]