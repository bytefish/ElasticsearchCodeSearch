# Elasticsearch Code Search Experiments #

This repository is an Elasticsearch experiment to see how to build a code search engine. I wanted to learn about Powershell and the recent updates to the 
Elasticsearch .NET client.

There is a Page to get an overview for your Elasticsearch index:

<a href="https://raw.githubusercontent.com/bytefish/ElasticsearchCodeSearch/main/doc/img/ElasticsearchCodeSearch_Home_Light.jpg">
    <img src="https://raw.githubusercontent.com/bytefish/ElasticsearchCodeSearch/main/doc/img/ElasticsearchCodeSearch_Home_Light.jpg" alt="The final Code Search with the Blazor Frontend" width="100%" />
</a>

And there's a page to query the Elasticsearch index:

<a href="https://raw.githubusercontent.com/bytefish/ElasticsearchCodeSearch/main/doc/img/ElasticsearchCodeSearch_Search_Light.jpg">
    <img src="https://raw.githubusercontent.com/bytefish/ElasticsearchCodeSearch/main/doc/img/ElasticsearchCodeSearch_Search_Light.jpg" alt="The final Code Search with the Blazor Frontend" width="100%" />
</a>

The Git Repositories are read by a Powershell script in `ElasticsearchCodeSearch\ElasticsearchCodeSearch.Indexer\git_indexer.ps1`, 
which sends the data to an ASP.NET Core Backend. The ASP.NET Core Backend then sends a Bulk Indexing Request to the Elasticsearch 
server.

You'll need to adjust the `Username`, `Password` and `CertificateFingerprint` for your Elasticsearch 
instance. See the Elasticsearch "Getting Started" guide on Elasticsearch.NET to learn how to obtain 
these values:

* [https://www.elastic.co/guide/en/elasticsearch/client/net-api/current/introduction.html](https://www.elastic.co/guide/en/elasticsearch/client/net-api/current/introduction.html)

You configure the Credentials in the `ElasticsearchCodeSearch\ElasticsearchCodeSearch\appsettings.json`:

```json
{
  "Elasticsearch": {
    "Uri": "https://localhost:9200",
    "IndexName": "documents",
    "Username": "elastic",
    "Password": "Sya9P0cOKK9yknjhHJKW",
    "CertificateFingerprint": "a6390608f670486f1bc31fe6e8d78fdb93f6026bd9ce58f0732961d362fd9f82"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning",
      "Microsoft.Hosting.Lifetime": "Information"
    }
  },
  "AllowedHosts": "*"
}
```

## How it's done ##


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

```
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

Nice!

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

Why is it a User Secret? Because I want to be 100% sure I am not accidentally leak my 
GitHub Token. If you want to do something similar in Production, you might want to 
consider using Azure KeyVault, Consul, ... or any safe secret store.

## Contributions ##

This is an Open Source project, feel free to contribute!

## Articles ##

The Search Engine has been introduced in a three articles: 

* [Implementing a Code Search: Elasticsearch and ASP.NET Core Backend (Part 1)](https://www.bytefish.de/blog/elasticsearch_code_search_part1_backend_elasticsearch.html)
* [Implementing a Code Search: Indexing Git Repositories using PowerShell (Part 2)](https://www.bytefish.de/blog/elasticsearch_code_search_part2_indexer_powershell.html)
* [Implementing a Code Search: A Frontend with ASP.NET Core Blazor (Part 3)](https://www.bytefish.de/blog/elasticsearch_code_search_part3_frontend_blazor.html)

## License ##

The code in this repository is licensed under MIT License.

The Pagination component has been taken from the Fluent UI project.

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
