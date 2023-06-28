# EntityFramework Core Experiments #

This repository is an Elasticsearch experiment to see how to build a 
very simple code search service. I wanted to learn about Powershell 
and the recent updates to the Elasticsearch .NET client.

The Git Repositories are read by a Powershell script, which sends the 
data to be indexed to an ASP.NET Core Backend. The ASP.NET Core Backend 
sends a Bulk Indexing Request to the Elasticsearch server.

