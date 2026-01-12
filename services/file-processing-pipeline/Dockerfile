FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["FileProcessingPipeline.csproj", "."]
RUN dotnet restore "FileProcessingPipeline.csproj"
COPY . .
RUN dotnet build "FileProcessingPipeline.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "FileProcessingPipeline.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Create temp directory for file processing
RUN mkdir -p /app/temp

ENTRYPOINT ["dotnet", "FileProcessingPipeline.dll"]
