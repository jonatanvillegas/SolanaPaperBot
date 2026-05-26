FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY src/SolanaPaperBot/SolanaPaperBot.csproj src/SolanaPaperBot/
RUN dotnet restore src/SolanaPaperBot/SolanaPaperBot.csproj

COPY . .
RUN dotnet publish src/SolanaPaperBot/SolanaPaperBot.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

RUN mkdir -p /app/data

COPY --from=build /app/publish .

EXPOSE 8080

ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "SolanaPaperBot.dll"]