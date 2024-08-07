#SSO dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["Authorization.SSO/Authorization.SSO.csproj", "Authorization.SSO/"]
RUN dotnet restore "Authorization.SSO/Authorization.SSO.csproj"
COPY . .
WORKDIR "/src/Authorization.SSO"
RUN dotnet build "Authorization.SSO.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Authorization.SSO.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Authorization.SSO.dll"]
