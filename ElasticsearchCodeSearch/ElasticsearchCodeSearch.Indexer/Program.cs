// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using ElasticsearchCodeSearch.Indexer.Client;
using ElasticsearchCodeSearch.Indexer.Client.Options;
using ElasticsearchCodeSearch.Indexer.Git;
using ElasticsearchCodeSearch.Indexer.Services;
using ElasticsearchCodeSearch.Shared.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

// Create the GitClientOptions by using the GH_TOKEN Key:
builder.Services.Configure<GitHubClientOptions>(o =>
{
    o.RequestDelayInMilliseconds = 0;
    o.AccessToken = Environment.GetEnvironmentVariable("GH_TOKEN")!;
});

builder.Services.AddHttpClient<ElasticsearchCodeSearchService>((services, client) =>
{
    client.BaseAddress = new Uri(builder.Configuration["ElasticsearchCodeSearchApi:BaseAddress"]!);
});

builder.Services.Configure<GitIndexerOptions>(o =>
{
    o.BaseDirectory = @"C:\Temp";
    o.MaxParallelClones = 1;
    o.MaxParallelBulkRequests = 4;
    o.BatchSize = 20;
    o.AllowedFilenames = new[]
    {
        ".gitignore",
        ".editorconfig",
        "README",
        "CHANGELOG"
    };
    o.AllowedExtensions = new[]
    {
        // C / C++
        ".c",
        ".cpp",
        ".h",
        ".hpp",
        // .NET
        ".cs",
        ".cshtml",
        ".csproj",
        ".fs",
        ".razor",
        ".sln",
        ".xaml",
        // CSS
        ".css",
        ".scss",
        ".sass",
        // CSV / TSV
        ".csv",
        ".tsv",
        // HTML
        ".html",
        ".htm",
        // JSON
        ".json", 
        // JavaScript
        ".js",
        ".jsx",
        ".spec.js",
        ".config.js",
        // Typescript
        ".ts",
        ".tsx", 
        // TXT
        ".txt", 
        // Powershell
        ".ps1",
        // Python
        ".py",
        // Configuration
        ".ini",
        ".config",
        // XML
        ".xml",
        ".xsl",
        ".xsd",
        ".dtd",
        ".wsdl",
        // Markdown
        ".md",
        ".markdown",
        // reStructured Text
        ".rst",
        // LaTeX
        ".tex",
        ".bib",
        ".bbx",
        ".cbx"
    };
});

builder.Services.AddSingleton<GitClient>();
builder.Services.AddSingleton<GitHubClient>();
builder.Services.AddSingleton<GitIndexerService>();

using IHost host = builder.Build();

// create a service scope
using var scope = host.Services.CreateScope();

var services = scope.ServiceProvider;

var indexer = services.GetRequiredService<GitIndexerService>();

await indexer.IndexOrganizationAsync("microsoft", default);

await host.RunAsync();