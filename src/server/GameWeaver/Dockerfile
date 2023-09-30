FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["src/server/GameWeaver/GameWeaver.csproj", "src/server/GameWeaver/"]
COPY ["src/server/Infrastructure/Infrastructure.csproj", "src/server/Infrastructure/"]
COPY ["src/server/Application/Application.csproj", "src/server/Application/"]
COPY ["src/server/Domain/Domain.csproj", "src/server/Domain/"]
RUN dotnet restore "src/server/GameWeaver/GameWeaver.csproj"
COPY . .
WORKDIR "/src/src/server/GameWeaver"
RUN dotnet build "GameWeaver.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "GameWeaver.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "GameWeaver.dll"]
