# Multi-stage Dockerfile for SMS Prototype Backend
# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy solution and project files
COPY ["SMSPrototype1/SMSPrototype1.csproj", "SMSPrototype1/"]
COPY ["SMSDataContext/SMSDataContext.csproj", "SMSDataContext/"]
COPY ["SMSDataModel/SMSDataModel.csproj", "SMSDataModel/"]
COPY ["SMSRepository/SMSRepository.csproj", "SMSRepository/"]
COPY ["SMSServices/SMSServices.csproj", "SMSServices/"]
COPY ["SMSPrototype1.sln", "./"]

# Restore dependencies
RUN dotnet restore "SMSPrototype1.sln"

# Copy all source code
COPY . .

# Build the application
WORKDIR "/src/SMSPrototype1"
RUN dotnet build "SMSPrototype1.csproj" -c Release -o /app/build

# Stage 2: Publish
FROM build AS publish
RUN dotnet publish "SMSPrototype1.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 3: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# Create non-root user for security
RUN groupadd -r smsapp && useradd -r -g smsapp smsapp

# Copy published app from publish stage
COPY --from=publish /app/publish .

# Create logs directory
RUN mkdir -p /app/logs && chown -R smsapp:smsapp /app/logs

# Switch to non-root user
USER smsapp

# Expose ports
EXPOSE 8080
EXPOSE 8081

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=10s --retries=3 \
    CMD curl --fail http://localhost:8080/health || exit 1

# Environment variables
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

# Entry point
ENTRYPOINT ["dotnet", "SMSPrototype1.dll"]
