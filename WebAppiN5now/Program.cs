using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using WebAppiN5now.Data;
using Nest;
using WebAppiN5now.Configurations;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<ElasticsearchSettings>(builder.Configuration.GetSection("ElasticsearchSettings"));

var elasticsearchSettings = new ElasticsearchSettings();
builder.Configuration.GetSection("ElasticsearchSettings").Bind(elasticsearchSettings);
var settings = new ConnectionSettings(new Uri(elasticsearchSettings.Uri)).DefaultIndex("permissions");

var client = new ElasticClient(settings);
builder.Services.AddSingleton<IElasticClient>(client);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();