using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .AddJsonFile("/config/appsettings.json", true)
    .AddEnvironmentVariables();

builder.Services
    .AddRouting(options => options.LowercaseUrls = true)
    .AddControllers()
    .AddMvcOptions(options => options.AddWebApiToolkitFilters())
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

builder.Services
    .AddPlayerServiceCore()
    .AddPlayerServiceRepositories((sp, optionsBuilder) =>
    {
        var connectionString = sp.GetRequiredService<IConfiguration>().GetConnectionString("Tgs");
        _ = optionsBuilder.UseNpgsql(connectionString);
    });

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Player Service",
        Version = "v1"
    });

    options.ApplyWebApiToolkitSupport();
    options.ApplyDisplayNameSchemaId();
});

builder.Services.AddHealthChecks();

var app = builder.Build();

app.MapControllers();
app.MapSwagger();
app.UseSwaggerUI();

app.MapHealthChecks("/healthz");

app.Run();
