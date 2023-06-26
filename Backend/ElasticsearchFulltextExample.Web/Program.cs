using ElasticsearchFulltextExample.Web.Elasticsearch;
using ElasticsearchFulltextExample.Web.Options;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddCors();

builder.Services.AddOptions<ElasticCodeSearchOptions>("Elasticsearch");
builder.Services.AddSingleton<ElasticCodeSearchClient>();

builder.Services.AddControllers();
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
