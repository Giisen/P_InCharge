# Stage 1: Base image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Stage 2: Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["InCharge.Server/InCharge.Server.csproj", "InCharge.Server/"]
RUN dotnet restore "./InCharge.Server/InCharge.Server.csproj"
COPY . .
WORKDIR "/src/InCharge.Server"
RUN dotnet build "./InCharge.Server.csproj" -c Release -o /app/build

# Stage 3: Publish stage
FROM build AS publish
RUN dotnet publish "./InCharge.Server.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 4: Final stage
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Set the user after all file operations for better security
USER app

ENTRYPOINT ["dotnet", "InCharge.Server.dll"]