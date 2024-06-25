// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using ElasticsearchCodeSearch.Hosting;
using System.Text.Json.Serialization;
using ElasticsearchCodeSearch.Shared.Elasticsearch;
using ElasticsearchCodeSearch.Indexer.Git;
using ElasticsearchCodeSearch.Indexer.GitHub.Options;
using ElasticsearchCodeSearch.Indexer.GitHub;
using ElasticsearchCodeSearch.Services;

var builder = WebApplication.CreateBuilder(args);

// Add Configuration Sources
builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
    .AddEnvironmentVariables()
    .AddUserSecrets<Program>();


// Add Options
builder.Services.AddOptions();



ConfigureElasticsearch(builder);
ConfigureIndexingServices(builder);

// Add CORS Services
builder.Services.AddCors();

builder.Services.AddControllers().AddJsonOptions(options =>
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

static void ConfigureElasticsearch(WebApplicationBuilder builder)
{
    builder.Services.Configure<ElasticCodeSearchOptions>(builder.Configuration.GetSection("Elasticsearch"));

    // Add Client
    builder.Services.AddSingleton<ElasticCodeSearchClient>();

    // Add Hosted Services
    builder.Services.AddHostedService<ElasticsearchInitializerHostedService>();
}

static void ConfigureIndexingServices(WebApplicationBuilder builder)
{
    // Create the GitClientOptions by using the GH_TOKEN Key:
    builder.Services.Configure<GitHubClientOptions>(builder.Configuration.GetSection("GitHubClient"));
    builder.Services.Configure<GitIndexerOptions>(builder.Configuration.GetSection("GitHubIndexer"));

    builder.Services.AddSingleton<GitExecutor>();
    builder.Services.AddSingleton<GitExecutor>();
    builder.Services.AddSingleton<GitHubClient>();
    builder.Services.AddSingleton<GitHubIndexerService>();

    builder.Services.AddSingleton<IndexerJobQueues>();
    builder.Services.AddHostedService<ElasticsearchIndexerHostedService>();
}