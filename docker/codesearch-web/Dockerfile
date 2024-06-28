# Get the dotnet Build Environment:
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /source

# COPY Project Files:
COPY ../src/ElasticsearchCodeSearch.Web.Client/*.csproj ./ElasticsearchCodeSearch.Web.Client/
COPY ../src/ElasticsearchCodeSearch.Web.Server/*.csproj ./ElasticsearchCodeSearch.Web.Server/
COPY ../src/ElasticsearchCodeSearch.Shared/*.csproj ./ElasticsearchCodeSearch.Shared/

# And restore the NuGet Packages:
RUN dotnet restore "ElasticsearchCodeSearch.Web.Server/ElasticsearchCodeSearch.Web.Server.csproj"

# COPY 
COPY ../src/ElasticsearchCodeSearch.Web.Client/. ./src/ElasticsearchCodeSearch.Web.Client/
COPY ../src/ElasticsearchCodeSearch.Web.Server/. ./src/ElasticsearchCodeSearch.Web.Server/
COPY ../src/ElasticsearchCodeSearch.Shared/. ./src/ElasticsearchCodeSearch.Shared/

RUN dotnet publish ./src/ElasticsearchCodeSearch.Web.Server/ElasticsearchCodeSearch.Web.Server.csproj -c Release -o /app

# Build the final image
FROM mcr.microsoft.com/dotnet/aspnet:8.0

# Copy Artifacts from Build:
WORKDIR /app
COPY --from=build /app ./

# Start the Kestrel Server:
ENTRYPOINT ["dotnet", "ElasticsearchCodeSearch.Web.Server.dll"]