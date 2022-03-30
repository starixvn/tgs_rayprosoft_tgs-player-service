using System.Text.Json.Serialization;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .AddJsonFile("/config/appsettings.json", true)
    .AddEnvironmentVariables();

builder.Services
    .AddRouting(options => options.LowercaseUrls = true)
    .AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Player Service",
        Version = "v1"
    });
});

builder.Services.AddHealthChecks();

var app = builder.Build();

app.MapControllers();
app.MapSwagger();
app.UseSwaggerUI();

app.MapHealthChecks("/healthz");

app.Run();
