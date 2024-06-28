# Elasticsearch Code Search #

This repository implements a Code Search Engine using ASP.NET Core and Elasticsearch. You can use it 
to index repositories from GitHub, Codeberg, GitLab and other sources. It comes with a Blazor 
Frontend built upon FluentUI Components.

There is a Page to get an overview pfor your Elasticsearch index:

<a href="https://raw.githubusercontent.com/bytefish/ElasticsearchCodeSearch/main/doc/img/ElasticsearchCodeSearch_SearchClusterOverview.jpg">
    <img src="https://raw.githubusercontent.com/bytefish/ElasticsearchCodeSearch/main/doc/img/ElasticsearchCodeSearch_SearchClusterOverview.jpg" alt="The final Code Search with the Blazor Frontend" width="100%" />
</a>

And there's a page to query the Elasticsearch index:

<a href="https://raw.githubusercontent.com/bytefish/ElasticsearchCodeSearch/main/doc/img/ElasticsearchCodeSearch_SearchCode.jpg">
    <img src="https://raw.githubusercontent.com/bytefish/ElasticsearchCodeSearch/main/doc/img/ElasticsearchCodeSearch_SearchCode.jpg" alt="The final Code Search with the Blazor Frontend" width="100%" />
</a>

There are more pages, like:

* <a href="https://raw.githubusercontent.com/bytefish/ElasticsearchCodeSearch/main/doc/img/ElasticsearchCodeSearch_IndexGitRepository.jpg">Page for indexing a Git Repository</a>
* <a href="https://raw.githubusercontent.com/bytefish/ElasticsearchCodeSearch/main/doc/img/ElasticsearchCodeSearch_IndexGitHubRepository.jpg">Page for indexing a GitHub Repository</a>
* <a href="https://raw.githubusercontent.com/bytefish/ElasticsearchCodeSearch/main/doc/img/ElasticsearchCodeSearch_DebuggingManageIndex.jpg">Page for Managing the Elasticsearch Index</a>

## Getting Started ##

Getting started is as simple as cloning this repository and running the following command:

```
docker compose --profile dev up
```

You can then navigate to `https://localhost:5001` and start searching and indexing Git Repositories.

### Docker Profiles ###

There are 4 Docker Profiles:

* `dev`
    * Starts all 3 services.
* `elastic`
    * Starts the Elasticsearch Server.
* api
    * Starts the ElasticsearchCodeSearch API
* web
    * Starts the ElasticsearchCodeSearch Blazor App

To only run the Elasticsearch Server you would pass the `elastic` profile:

```
docker compose --profile elastic up
```

To run the Elasticsearch Server and the Code Search API you would pass the `elastic` and `api` profiles:

```
docker compose --profile elastic --profile api up
```

This is super useful, if you want to locally debug the ASP.NET Core services.

## How it was done ##

### Configuring HTTPS for Elasticsearch ###

To secure the HTTPS communication with Elasticsearch we need to generate a certificate 
first. The easiest way to do this is to use the `elasticsearch-certutil` command line 
tool.

We start by creating a Certificate Authority (CA):

```powershell
elasticsearch-certutil ca --silent --pem -out ./elastic-cert/ca.zip
```

In the `docker/elasticsearch/elastic-cert` we will now find a `ca.zip` and unzip it.

Next we create a `instances.yml`file for the local Elasticsearch instance `es01`:

```yaml
instances:
  - name: es01
    dns:
      - es01
      - localhost
    ip:
        - 127.0.0.1
```

We can then pass the `instances.yml` to the `elasticsearch-certutil` command line tool and 
create a certificate using our previously generated CA certificate:

```powershell
elasticsearch-certutil cert --silent --pem -out ./certs.zip --in ./instances.yml --ca-cert ./elastic-cert/ca/ca.crt --ca-key ./elastic-cert/ca/ca.key
```

In an `.env` file we are defining the Environment variables as:

```ini
ELASTIC_HOSTNAME=es01
ELASTIC_USERNAME=elastic
ELASTIC_PASSWORD=secret
ELASTIC_PORT=9200
ELASTIC_SECURITY=true
ELASTIC_SCHEME=https
ELASTIC_VERSION=8.14.1
```

In the `docker-compose.yml` we then configure `elasticsearch` with our generated certificates as:

```yaml
services:
  elasticsearch:
    image: docker.elastic.co/elasticsearch/elasticsearch:${ELASTIC_VERSION:-8.14.1}
    container_name: ${ELASTIC_HOSTNAME:-es01}
    hostname: ${ELASTIC_HOSTNAME:-es01}
    restart: ${RESTART_MODE:-unless-stopped}
    healthcheck:
      test: ["CMD-SHELL", "curl --user ${ELASTIC_USER:-elastic}:${ELASTIC_PASSWORD:-secret} --silent --fail https://localhost:9200/_cluster/health -k || exit 1" ]
      interval: 10s
      timeout: 5s
      retries: 3
      start_period: 30s
    env_file:
      - ./.env
    environment:
      - node.name=es01
      - discovery.type=single-node
      - ELASTIC_PASSWORD=${ELASTIC_PASSWORD:-secret}
      - xpack.security.enabled=${ELASTIC_SECURITY:-true}
      - xpack.security.http.ssl.enabled=true
      - xpack.security.http.ssl.verification_mode=none
      - xpack.security.http.ssl.key=/usr/share/elasticsearch/config/cert/es01.key
      - xpack.security.http.ssl.certificate=/usr/share/elasticsearch/config/cert/es01.crt
      - xpack.security.http.ssl.certificate_authorities=/usr/share/elasticsearch/config/cert/ca/ca.crt
      - xpack.security.transport.ssl.enabled=${ELASTIC_SECURITY:-true}
      - xpack.security.transport.ssl.verification_mode=none
      - xpack.security.transport.ssl.certificate_authorities=/usr/share/elasticsearch/config/cert/ca/ca.crt
      - xpack.security.transport.ssl.certificate=/usr/share/elasticsearch/config/cert/es01.crt
      - xpack.security.transport.ssl.key=/usr/share/elasticsearch/config/cert/es01.key
    ulimits:
      memlock:
        soft: -1
        hard: -1
    volumes:
      - ./elasticsearch/elastic-data:/usr/share/elasticsearch/data
      - ./elasticsearch/elastic-cert:/usr/share/elasticsearch/config/cert
    ports:
      - "9200:9200"
      - "9300:9300"
```

We can now verify, if Elasticsearch starts correctly by using `curl`:

```Powershell
C:\Users\philipp>curl -k https://127.0.0.1:9200  -u elastic:secret
{
  "name" : "es01",
  "cluster_name" : "docker-cluster",
  "cluster_uuid" : "2pvSQcC-Tnu-cbQuc1AONw",
  "version" : {
    "number" : "8.14.1",
    "build_flavor" : "default",
    "build_type" : "docker",
    "build_hash" : "93a57a1a76f556d8aee6a90d1a95b06187501310",
    "build_date" : "2024-06-10T23:35:17.114581191Z",
    "build_snapshot" : false,
    "lucene_version" : "9.10.0",
    "minimum_wire_compatibility_version" : "7.17.0",
    "minimum_index_compatibility_version" : "7.0.0"
  },
  "tagline" : "You Know, for Search"
}
```

And finally we calculate the Certificate Fingerprint using:

```powershell
openssl.exe x509 -fingerprint -sha256 -in .\es01.crt
```

And get our Certificate Fingerprint as:

```
31a63ffca5275df7ea7d6fc7e92b42cfa774a0feed7d7fa8488c5e46ea9ade3f
```

And that's it.

In the .NET application I can then create the `ElasticsearchClient` like this:

```csharp
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

// ...

namespace ElasticsearchCodeSearch.Shared.Elasticsearch
{
    public class ElasticCodeSearchClient
    {
    
        // ...
    
        public virtual ElasticsearchClient CreateClient(ElasticCodeSearchOptions options)
        {
            var settings = new ElasticsearchClientSettings(new Uri(options.Uri))
                .CertificateFingerprint(options.CertificateFingerprint)
                .Authentication(new BasicAuthentication(options.Username, options.Password));

            return new ElasticsearchClient(settings);
        }
        
        // ...
    }
}
```

### Setting up HTTPS Communication in ASP.NET Core with Docker ###

We will create and trust the self-signed certificates with the following command:

```powershell
dotnet dev-certs https -ep $env:USERPROFILE\.aspnet\https\aspnetapp.pfx -p SuperStrongPassword
dotnet dev-certs https --trust
```

Next is setting the GH Token for the Client:

```powershell
dotnet user-secrets set "GitHubClient:AccessToken" "<Your GH Token Here>"
```

If you need to manage secrets in Production, you should consider using Docker Secrets, Azure KeyVault, Consul, ... or any other safe secret store.

## Contributions ##

This is an Open Source project, feel free to contribute!

## Articles ##

The Search Engine has been introduced in three articles so far: 

* [Implementing a Code Search: Elasticsearch and ASP.NET Core Backend (Part 1)](https://www.bytefish.de/blog/elasticsearch_code_search_part1_backend_elasticsearch.html)
* [Implementing a Code Search: Indexing Git Repositories using PowerShell (Part 2)](https://www.bytefish.de/blog/elasticsearch_code_search_part2_indexer_powershell.html)
* [Implementing a Code Search: A Frontend with ASP.NET Core Blazor (Part 3)](https://www.bytefish.de/blog/elasticsearch_code_search_part3_frontend_blazor.html)

## License ##

The code in this repository is licensed under MIT License.

The Pagination component and project structure have been taken from the Fluent UI project.

```
MIT License

Copyright (c) Microsoft Corporation. All rights reserved.

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE
```
