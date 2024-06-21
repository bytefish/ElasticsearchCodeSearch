# Get the dotnet Build Environment:
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /source

# COPY Project Files:
COPY ../src/ElasticsearchCodeSearch/*.csproj ./ElasticsearchCodeSearch/
COPY ../src/ElasticsearchCodeSearch.Shared/*.csproj ./ElasticsearchCodeSearch.Shared/

# And restore the NuGet Packages:
RUN dotnet restore "ElasticsearchCodeSearch/ElasticsearchCodeSearch.csproj"

# COPY 
COPY ../src/ElasticsearchCodeSearch/. ./src/ElasticsearchCodeSearch/
COPY ../src/ElasticsearchCodeSearch.Shared/. ./src/ElasticsearchCodeSearch.Shared/

RUN dotnet publish ./src/ElasticsearchCodeSearch/ElasticsearchCodeSearch.csproj -c release -o /app

# Build the final image
FROM mcr.microsoft.com/dotnet/aspnet:8.0

# Copy Artifacts from Build:
WORKDIR /app
COPY --from=build /app ./

# Install Git
RUN apt-get -y update
RUN apt-get -y install git

# Start the Kestrel Server:
ENTRYPOINT ["dotnet", "ElasticsearchCodeSearch.dll"]