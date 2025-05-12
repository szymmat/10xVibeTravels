# Stage 1: Build the application
FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build
ARG BUILD_CONFIGURATION=Release
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

# Labels for image metadata
LABEL maintainer="szymmat <your-email@example.com>"
LABEL version="1.0.0"
LABEL description="10xVibeTravels Blazor Application."

WORKDIR /src

# Copy project file and restore dependencies
# This layer is cached when dependencies don't change
COPY ["10xVibeTravels/10xVibeTravels.csproj", "app/"]
# If you have a solution file or other project-level files like Directory.Build.props or nuget.config, copy them here first.
# COPY ["YourSolution.sln", "."]
# COPY ["nuget.config", "."]

WORKDIR /src/app
RUN dotnet restore "10xVibeTravels.csproj" -r linux-musl-x64

# Copy the rest of the application code
WORKDIR /src
COPY ["10xVibeTravels/", "app/"]

# Publish the application
WORKDIR /src/app
RUN dotnet publish "10xVibeTravels.csproj" -c $BUILD_CONFIGURATION -o /app/publish --no-restore -r linux-musl-x64 /p:UseAppHost=true /p:PublishTrimmed=true /p:PublishSingleFile=false

# Stage 2: Create the runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS final
WORKDIR /app

# Copy published application from the build stage
COPY --from=build /app/publish .

# Create a non-root user and group
RUN addgroup -S appgroup && adduser -S -G appgroup appuser

# Set environment variables
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://0.0.0.0:8080
# Ensure correct globalization support on Alpine for .NET
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false
RUN apk add --no-cache icu-libs krb5-libs libgcc libintl libssl3 libstdc++ zlib curl

# Switch to non-root user
USER appuser

# Expose port 8080 for the application
EXPOSE 8080

# Healthcheck for the application
# This checks if the application is responding on the root path.
# Adjust the URL or command if you have a specific health check endpoint (e.g., /healthz).
HEALTHCHECK --interval=30s --timeout=5s --start-period=15s --retries=3 \
  CMD curl --fail http://localhost:8080/ || exit 1

# Set the entrypoint for the application
ENTRYPOINT ["dotnet", "10xVibeTravels.dll"] 