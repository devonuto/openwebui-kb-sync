# Stage 1: Build the application
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /source
COPY . .
# Compile the app to the /publish directory
RUN dotnet publish -c Release -o /publish

# Stage 2: Create the lightweight runtime image
FROM mcr.microsoft.com/dotnet/runtime:10.0
# Copy the compiled binaries from the build stage
COPY --from=build /publish /app
# Set the working directory to a separate data folder for the state file
WORKDIR /data
ENTRYPOINT ["dotnet", "/app/app.dll"]