// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using ElasticsearchCodeSearch.Hosting;
using System.Text.Json.Serialization;
using ElasticsearchCodeSearch.Shared.Elasticsearch;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddCors();

// Add Options
builder.Services.AddOptions();
builder.Services.Configure<ElasticCodeSearchOptions>(builder.Configuration.GetSection("Elasticsearch"));

// Add Client
builder.Services.AddSingleton<ElasticCodeSearchClient>();

// Add Hosted Services
builder.Services.AddHostedService<ElasticsearchInitializerHostedService>();

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
