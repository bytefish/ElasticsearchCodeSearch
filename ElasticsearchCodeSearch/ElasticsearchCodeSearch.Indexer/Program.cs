// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using ElasticsearchCodeSearch.Indexer.Git;
using ElasticsearchCodeSearch.Indexer.GitHub;
using ElasticsearchCodeSearch.Indexer.GitHub.Options;
using ElasticsearchCodeSearch.Indexer.Hosted;
using ElasticsearchCodeSearch.Indexer.Services;
using ElasticsearchCodeSearch.Shared.Elasticsearch;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddCors();

// Configure the Indexer
ConfigureIndexingServices(builder);

builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseHttpsRedirection();

app.UseSwagger();
app.UseSwaggerUI();

app.UseCors(x => x
    .AllowAnyOrigin()
    .AllowAnyHeader()
    .AllowAnyMethod());

app.MapControllers();

app.Run();

static void ConfigureIndexingServices(WebApplicationBuilder builder)
{
    // Add Options
    builder.Services.Configure<ElasticCodeSearchOptions>(builder.Configuration.GetSection("Elasticsearch"));

    // Add Client
    builder.Services.AddSingleton<ElasticCodeSearchClient>();

    // Create the GitClientOptions by using the GH_TOKEN Key:
    builder.Services.Configure<GitHubClientOptions>(o =>
    {
        o.RequestDelayInMilliseconds = 0;
        o.AccessToken = Environment.GetEnvironmentVariable("GH_TOKEN")!;
    });

    builder.Services.Configure<GitIndexerOptions>(o =>
    {
        o.BaseDirectory = @"C:\Temp";
        o.MaxParallelClones = 1;
        o.MaxParallelBulkRequests = 1;
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

    builder.Services.AddSingleton<GitExecutor>();
    builder.Services.AddSingleton<GitHubClient>();
    builder.Services.AddSingleton<GitHubIndexerService>();

    builder.Services.AddSingleton<IndexerJobQueues>();
    builder.Services.AddHostedService<ElasticsearchIndexerHostedService>();
}