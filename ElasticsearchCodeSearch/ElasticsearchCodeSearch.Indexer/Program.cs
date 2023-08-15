
using ElasticsearchCodeSearch.Indexer.Client;
using ElasticsearchCodeSearch.Indexer.Client.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

// Create the GitClientOptions by using the GH_TOKEN Key:
builder.Services.Configure<GitHubClientOptions>(o =>
{
    o.RequestDelayInMilliseconds = 50;
    o.AccessToken = Environment.GetEnvironmentVariable("GH_TOKEN")!;
});

builder.Services.AddSingleton<GitHubClient>();

using IHost host = builder.Build();

// create a service scope
using var scope = host.Services.CreateScope();

var services = scope.ServiceProvider;

var client = services.GetRequiredService<GitHubClient>();

var res = await client.GetAllRepositoriesByOrganizationAsync("microsoft", 100, default);

await host.RunAsync();