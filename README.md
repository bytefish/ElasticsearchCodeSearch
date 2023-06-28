# Elasticsearch Code Search Experiments #

This repository is an Elasticsearch experiment to see how to build a very simple 
code search. I wanted to learn about Powershell and the recent updates to the 
Elasticsearch .NET client.

The Git Repositories are read by a Powershell script in `Scripts\git_indexer.ps1`, 
which sends the data to be indexed to an ASP.NET Core Backend. The ASP.NET Core 
Backend then sends a Bulk Indexing Request to the Elasticsearch server.

You'll need to adjust the Username, Password and CertificateFingerprint for your 
Elasticsearch instance.

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

By default it sorts all matches by their latest commit date, that has been extracted 
from the git repository.

This is an Open Source project, feel free to contribute!