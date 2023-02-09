using System.Text.Json.Serialization;
using FluentdForward.OpenTelemetry.MessagePack;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Npgsql;
using OpenTelemetry.Instrumentation.AspNetCore;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Tgs.PlayerService.Swagger;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .AddJsonFile("/config/appsettings.json", true)
    .AddEnvironmentVariables();

builder.Logging
    .AddOpenTelemetry(options =>
    {
        options.IncludeFormattedMessage = true;
        options.ParseStateValues = true;
        _ = options.AddFluentdForwardExporter(fluentOptions =>
        {
            var nodeIp = builder.Configuration.GetValue<string>("NODE_IP")!;
            fluentOptions.Host = nodeIp;
            fluentOptions.Tag = builder.Configuration.GetValue<string>("OTEL_SERVICE_NAME")!;
            fluentOptions.UseMessagePack(LogRecordFormatterResolver.GetResolverInstanceWithExtendInfo(
                "MachineInfo",
                () => new
                {
                    Environment.MachineName
                }));
        });
    });

builder.Services
    .AddRouting(options => options.LowercaseUrls = true)
    .AddControllers()
    .AddMvcOptions(options => options.AddWebApiToolkitFilters())
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

builder.Services
    .AddSecurity()
    .AddSerialNumberGenerator()
    .AddPlayerServiceCore()
    .AddPlayerServiceRepositories((sp, optionsBuilder) => optionsBuilder
        .UseNpgsql("name=Tgs"))
    .AddMemoryCache()
    .AddPlayerServiceCache();

builder.Services
    .AddOpenTelemetryTracing(
        tracingBuilder => tracingBuilder
            .SetResourceBuilder(ResourceBuilder.CreateDefault()
                .AddEnvironmentVariableDetector())
            .AddSource("Tgs.*")
            .AddSource("TGS.*")
            .AddNpgsql()
            .AddAspNetCoreInstrumentation()
            .AddProfileViewExporter()
            .AddOtlpExporter())
    .AddOpenTelemetryMetrics(meterBuiler => meterBuiler
        .SetResourceBuilder(ResourceBuilder.CreateDefault()
            .AddEnvironmentVariableDetector())
            .AddMeter("Tgs.PlayerBucket")
            .AddRuntimeInstrumentation()
            .AddAspNetCoreInstrumentation()
            .AddOtlpExporter())
    .AddProfileViewer()
    .Configure<AspNetCoreInstrumentationOptions>(options => options.Filter =
        httpContext => !httpContext.Request.Path.HasValue
            || !httpContext.Request.Path.StartsWithSegments("/swagger")
            && !httpContext.Request.Path.StartsWithSegments("/healthz")
            && !httpContext.Request.Path.StartsWithSegments("/metrics")
            && !httpContext.Request.Path.StartsWithSegments("/profiler/")
            && !new[] { "html", "ico", "png", "jpg" }.Any(
                ext => httpContext.Request.Path.Value.EndsWith($".{ext}")));

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Player Service",
        Version = "v1"
    });

    options.OperationFilter<HttpHeaderConstraintOperationFilter>();

    options.ApplyWebApiToolkitSupport();
    options.ApplyDisplayNameSchemaId();

    options.ResolveConflictingActions(apiDescriptions => 
        apiDescriptions.First());
});

builder.Services.AddHealthChecks();

var app = builder.Build();

app.MapControllers();
app.MapProfileViewer();
app.MapSwagger();
app.UseSwaggerUI();

app.MapHealthChecks("/healthz");

app.Run();