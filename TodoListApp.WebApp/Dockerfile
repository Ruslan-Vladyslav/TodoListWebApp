FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
workdir /src

COPY ["TodoListApp.WebApp/TodoListApp.WebApp.csproj", "TodoListApp.WebApp/"]
COPY ["TodoListApp.Services.Database/TodoListApp.Services.Database.csproj", "TodoListApp.Services.Database/"]
COPY ["TodoListApp.Services.WebApi/TodoListApp.Services.WebApi.csproj", "TodoListApp.Services.WebApi/"]
COPY ["TodoListApp.Services/TodoListApp.Services.csproj", "TodoListApp.Services/"]
COPY ["TodoListApp.WebApi.Models/TodoListApp.WebApi.Models.csproj", "TodoListApp.WebApi.Models/"]
COPY ["stylecop.json", "."]

RUN dotnet restore "TodoListApp.WebApp/TodoListApp.WebApp.csproj"

COPY . .
workdir "/src/TodoListApp.WebApp"
RUN dotnet publish "TodoListApp.WebApp.csproj" -c Release -o /app/publish --no-restore

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "TodoListApp.WebApp.dll"]
