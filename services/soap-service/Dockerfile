FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["SoapService.csproj", "."]
RUN dotnet restore "SoapService.csproj"
COPY . .
RUN dotnet build "SoapService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SoapService.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SoapService.dll"]
