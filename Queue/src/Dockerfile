FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY DataFetcher/DataFetcher.csproj DataFetcher/
COPY DataFetcher.BusinessLogic/DataFetcher.BusinessLogic.csproj DataFetcher.BusinessLogic/
COPY DataFetcher.Domain/DataFetcher.Domain.csproj DataFetcher.Domain/

RUN dotnet restore "DataFetcher/DataFetcher.csproj"

COPY . .

WORKDIR "/src/DataFetcher"
RUN dotnet build "DataFetcher.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "DataFetcher.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

COPY --from=publish /app/publish .


COPY DataFetcher/appsettings.json ./appsettings.json

EXPOSE 80
EXPOSE 443


ENTRYPOINT ["dotnet", "DataFetcher.dll"]

HEALTHCHECK CMD curl --fail http://localhost:80/health || exit 1
