# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY CicdDashboard.sln ./
COPY CicdDashboard.Api/CicdDashboard.Api.csproj CicdDashboard.Api/
COPY CicdDashboard.Api.Tests/CicdDashboard.Api.Tests.csproj CicdDashboard.Api.Tests/
RUN dotnet restore CicdDashboard.Api/CicdDashboard.Api.csproj
COPY . .
RUN dotnet publish CicdDashboard.Api/CicdDashboard.Api.csproj -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 5085
ENV ASPNETCORE_URLS=http://+:5085
ENTRYPOINT ["dotnet", "CicdDashboard.Api.dll"]
