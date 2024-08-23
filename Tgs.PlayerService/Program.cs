using System.Text.Json.Serialization;
using FluentdForward.OpenTelemetry.MessagePack;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Npgsql;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Tgs.PlayerService.Middlewares;
using Tgs.PlayerService.Swagger;

var builder = WebApplication.CreateBuilder(args);

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

builder.WebHost.UseSentry();

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
    .AddPlayerServiceCache()
    .AddMarketingReportClient((options, _) => options.BaseUrl =
        builder.Configuration.GetValue<Uri>("PlayerService:MarketingReportBaseUrl")!)
    .AddCashServiceClient((options, _) => options.BaseUrl =
        builder.Configuration.GetValue<Uri>("PlayerService:CashServiceBaseUrl")!);

var excludeExtensions = new[] { "html", "ico", "png", "jpg" };
builder.Services
    .AddOpenTelemetry()
    .ConfigureResource(_ => _.AddService(builder.Configuration.GetValue<string>("OTEL:OTEL_SERVICE_NAME")))
    .WithTracing(traceProviderBuilder => traceProviderBuilder
        .AddSource("Tgs.*")
        .AddSource("TGS.*")
        .AddSource("Valhalla.*")
        .AddNpgsql()
        .AddAspNetCoreInstrumentation(options => options.Filter =
            httpContext => !httpContext.Request.Path.HasValue
                || !httpContext.Request.Path.StartsWithSegments("/swagger")
                && !httpContext.Request.Path.StartsWithSegments("/health")
                && !httpContext.Request.Path.StartsWithSegments("/metrics")
                && !httpContext.Request.Path.StartsWithSegments("/profiler/")
                && !excludeExtensions.Any(
                    ext => httpContext.Request.Path.Value.EndsWith($".{ext}")))
        .AddProfileViewExporter()
        .AddOtlpExporter(options => options.Endpoint = builder.Configuration.GetValue<Uri>("OTEL:OTEL_COLLECTOR")))
    .WithMetrics(meterBuiler => meterBuiler
        .SetResourceBuilder(ResourceBuilder.CreateDefault()
            .AddEnvironmentVariableDetector())
            .AddRuntimeInstrumentation()
            .AddAspNetCoreInstrumentation()
            .AddOtlpExporter())
    .Services
    .AddProfileViewer();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("obsolete", new OpenApiInfo
    {
        Title = "Player Service",
        Version = "Obsolete"
    });

    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Player Service",
        Version = "v1"
    });

    options.DocInclusionPredicate((docName, desc) =>
    {
        var docInfo = options.SwaggerGeneratorOptions.SwaggerDocs[docName];

        return docInfo is not null
            && (docName is "v1"
                ? desc.GroupName is null || desc.GroupName == docInfo.Version
                : desc.GroupName == docInfo.Version);
    });

    options.OperationFilter<HttpHeaderConstraintOperationFilter>();

    options.ApplyWebApiToolkitSupport();
    options.ApplyDisplayNameSchemaId();
});

builder.Services
    .AddHealthChecks();
builder.Services.AddScoped<ExceptionMiddleware>();

var app = builder.Build();

app.UseMiddleware<ExceptionMiddleware>();
app.MapControllers();
app.MapProfileViewer();
app.MapSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
    options.SwaggerEndpoint("/swagger/obsolete/swagger.json", "Obsolete");
});

app.MapHealthChecks("/health");

app.Run();
