FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app
COPY ["src/AnalyticsService/AnalyticsService.csproj", "src/AnalyticsService/"]
RUN dotnet restore "src/AnalyticsService/AnalyticsService.csproj"
COPY . .
WORKDIR "/app/src/AnalyticsService"
RUN dotnet publish -c Release -o /out

FROM mcr.microsoft.com/dotnet/runtime:9.0
WORKDIR /app
COPY --from=build /out .
ENTRYPOINT ["dotnet", "AnalyticsService.dll"]