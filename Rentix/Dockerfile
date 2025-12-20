# 1. Aşama: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["Rentix.csproj", "./"]
RUN dotnet restore "Rentix.csproj"
COPY . .
RUN dotnet publish "Rentix.csproj" -c Release -o /app/publish

# 2. Aşama: Run
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "Rentix.dll"]