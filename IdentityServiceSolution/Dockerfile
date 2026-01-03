# 1. Build Stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy the solution file and project files first (for better caching)
COPY ["IdentityServiceSolution.slnx", "./"]
COPY ["Identity.API/Identity.API.csproj", "Identity.API/"]
COPY ["Identity.Core/Identity.Core.csproj", "Identity.Core/"]
COPY ["Identity.Tests/Identity.Tests.csproj", "Identity.Tests/"]

# Restore dependencies
RUN dotnet restore "Identity.API/Identity.API.csproj"

# Copy the rest of the source code
COPY . .

# Build and Publish the API
WORKDIR "/src/Identity.API"
RUN dotnet build "Identity.API.csproj" -c Release -o /app/build
RUN dotnet publish "Identity.API.csproj" -c Release -o /app/publish

# 2. Runtime Stage (The actual image that runs)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 80
EXPOSE 443
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Identity.API.dll"]