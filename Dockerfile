# Use the official .NET 8 SDK image to build the app
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy the .csproj file and restore dependencies
COPY *.csproj ./
RUN dotnet restore

# Copy the rest of the application code and build the app in Release mode
COPY . ./
RUN dotnet publish -c Release -o /out

# Use the official .NET 8 runtime image to run the app
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Copy the compiled app and configuration file from the build stage
COPY --from=build /out ./
COPY config-bot.toml ./

# Set the entry point to run the application
ENTRYPOINT ["dotnet", "NureBot.dll"]
