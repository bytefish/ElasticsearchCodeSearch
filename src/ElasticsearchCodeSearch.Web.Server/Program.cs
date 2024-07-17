// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using ElasticsearchCodeSearch.Web.Server;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

builder.Services.AddRazorComponents()
    .AddInteractiveWebAssemblyComponents();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
} 
else
{
    app.UseHsts();

}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.MapRazorComponents<App>()
        .AddInteractiveWebAssemblyRenderMode()
       .AddAdditionalAssemblies(typeof(ElasticsearchCodeSearch.Web.Client._Imports).Assembly);

app.UseRouting();
app.UseAntiforgery();

app.Run();
