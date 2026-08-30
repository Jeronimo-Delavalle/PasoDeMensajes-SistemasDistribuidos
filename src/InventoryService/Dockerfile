FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app
COPY ["src/InventoryService/InventoryService.csproj", "src/InventoryService/"]
RUN dotnet restore "src/InventoryService/InventoryService.csproj"
COPY . .
WORKDIR "/app/src/InventoryService"
RUN dotnet publish -c Release -o /out

FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /out .
ENTRYPOINT ["dotnet", "InventoryService.dll"]