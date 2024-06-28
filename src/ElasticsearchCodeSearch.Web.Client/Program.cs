// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using ElasticsearchCodeSearch.Web.Client;
using ElasticsearchCodeSearch.Web.Client.Infrastructure;
using ElasticsearchCodeSearch.Shared.Services;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.FluentUI.AspNetCore.Components;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped<ApplicationErrorTranslator>();
builder.Services.AddScoped<ApplicationErrorMessageService>();

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

builder.Services.AddHttpClient<ElasticsearchCodeSearchService>((services, client) =>
{
    client.BaseAddress = new Uri(builder.Configuration["ElasticsearchCodeSearchApi:BaseAddress"]!);
});

builder.Services.AddLocalization();

// Fluent UI
builder.Services.AddFluentUIComponents();

await builder.Build().RunAsync();
