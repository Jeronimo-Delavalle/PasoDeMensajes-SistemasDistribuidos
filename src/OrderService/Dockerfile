FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app
COPY ["src/OrdersService/OrdersService.csproj", "src/OrdersService/"]
RUN dotnet restore "src/OrdersService/OrdersService.csproj"
COPY . .
WORKDIR "/app/src/OrdersService"
RUN dotnet publish -c Release -o /out

FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /out .
ENTRYPOINT ["dotnet", "OrdersService.dll"]