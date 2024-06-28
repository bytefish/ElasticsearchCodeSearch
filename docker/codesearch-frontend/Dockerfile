# Get the dotnet Build Environment:
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /source

# COPY Project Files:
COPY ../src/ElasticsearchCodeSearch.Client/*.csproj ./ElasticsearchCodeSearch.Client/
COPY ../src/ElasticsearchCodeSearch.Shared/*.csproj ./ElasticsearchCodeSearch.Shared/

# And restore the NuGet Packages:
RUN dotnet restore "ElasticsearchCodeSearch.Client/ElasticsearchCodeSearch.Client.csproj"

# COPY 
COPY ../src/ElasticsearchCodeSearch.Client/. ./src/ElasticsearchCodeSearch.Client/
COPY ../src/ElasticsearchCodeSearch.Shared/. ./src/ElasticsearchCodeSearch.Shared/

RUN dotnet publish ./src/ElasticsearchCodeSearch.Client/ElasticsearchCodeSearch.Client.csproj -c Release -o /app

# Build the final image
FROM mcr.microsoft.com/dotnet/aspnet:8.0

# Copy Artifacts from Build:
WORKDIR /app
COPY --from=build /app ./

# Start the Kestrel Server:
ENTRYPOINT ["dotnet", "ElasticsearchCodeSearch.Client.dll"]