# Elasticsearch Code Search Experiments #

This repository is an Elasticsearch experiment to see how to build a code search engine. I wanted to learn about Powershell and the recent updates to the 
Elasticsearch .NET client.

<a href="https://raw.githubusercontent.com/bytefish/ElasticsearchCodeSearch/main/Screenshots/ElasticsearchCodeSearch.jpg">
    <img src="https://raw.githubusercontent.com/bytefish/ElasticsearchCodeSearch/main/Screenshots/ElasticsearchCodeSearch.jpg" alt="The final Code Search with the Blazor Frontend" width="100%" />
</a>

The Git Repositories are read by a Powershell script in `ElasticsearchCodeSearch\ElasticsearchCodeSearch.Indexer\git_indexer.ps1`, 
which sends the data to be indexed to an ASP.NET Core Backend. The ASP.NET Core Backend then sends a Bulk Indexing Request to the 
Elasticsearch server.

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

By default the Backend sorts all matches by their latest commit date, that has been extracted 
from the git repository. 

This is an Open Source project, feel free to contribute!

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
