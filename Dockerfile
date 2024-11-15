# Use the official .NET SDK image for .NET 8.0 to build the app
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

# Set the working directory inside the container
WORKDIR /app

# Copy the .csproj file and restore any dependencies (via `dotnet restore`)
COPY ["TomocaCampaignAPI/TomocaCampaignAPI.csproj", "TomocaCampaignAPI/"]

# Restore the dependencies (this step is done before copying the full source code to leverage Docker cache)
RUN dotnet restore "TomocaCampaignAPI/TomocaCampaignAPI.csproj"

# Copy the rest of the application code
COPY . .

# Publish the app to a folder called "out"
RUN dotnet publish "TomocaCampaignAPI/TomocaCampaignAPI.csproj" -c Release -o /out

# Use the official .NET runtime image for .NET 8.0 to run the app
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime

# Set the working directory in the runtime image
WORKDIR /app

# Copy the published app from the build stage
COPY --from=build /out .

# Set environment variables (for production, consider using Docker secrets or environment variables)
ENV ASPNETCORE_ENVIRONMENT=Production

# Expose port 80
EXPOSE 80

# Command to run the application
ENTRYPOINT ["dotnet", "TomocaCampaignAPI.dll"]
