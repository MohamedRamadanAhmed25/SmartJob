# Use the official .NET 9.0 SDK image to build the app
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

# Copy the csproj file and restore any dependencies (via 'dotnet restore')
COPY *.csproj ./
RUN dotnet restore

# Copy the rest of the app's source code and build it
COPY . ./
RUN dotnet publish -c Release -o out

# Build the runtime image using the official .NET 9.0 ASP.NET runtime image
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /app/out .

# Expose port 8080 (the default port for ASP.NET Core apps in .NET 8/9 containers)
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

# Specify the command to run the app
ENTRYPOINT ["dotnet", "SmartJob.API.dll"]
