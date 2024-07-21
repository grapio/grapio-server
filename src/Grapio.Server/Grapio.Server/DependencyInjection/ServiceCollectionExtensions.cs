using Correlate.DependencyInjection;
using FastEndpoints;
using FastEndpoints.Swagger;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Grapio.Server.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static void ConfigureServices(this IServiceCollection services, IHostEnvironment environment)
    {
        services.AddEndpoints();
        services.AddCorrelation();
        services.AddMediatR(ConfigureMediator);
        services.AddOTel(environment.ApplicationName);
        services.SwaggerDocument(CreateSwaggerDocumentForRelease1);
    }

    private static void CreateSwaggerDocumentForRelease1(DocumentOptions opts)
    {
        opts.MaxEndpointVersion = 1;
        opts.DocumentSettings = settings =>
        {
            settings.DocumentName = "Release 1.0";
            settings.Title = "Grapio Server API";
            settings.Description = "Official Grapio Server API";
        };
    }

    private static void AddEndpoints(this IServiceCollection services)
    {
        services.AddFastEndpoints();
    }

    private static void AddOTel(this IServiceCollection services, string serviceName)
    {
        services.AddOpenTelemetry()
            .ConfigureResource(resource => resource.AddService(serviceName))
            .WithLogging(logging => logging
                .AddConsoleExporter()
                .AddOtlpExporter()
                .AddConsoleExporter()
            ).WithTracing(tracing => tracing
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddOtlpExporter()
                .AddConsoleExporter()
            ).WithMetrics(metrics => metrics
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddOtlpExporter()
            );
    }

    private static void AddCorrelation(this IServiceCollection services)
    {
        services.AddCorrelate(options =>
        {
            options.RequestHeaders = ["Correlation-ID"];
            options.IncludeInResponse = true;
        });
    }

    private static void ConfigureMediator(MediatRServiceConfiguration config)
    {
        config.RegisterServicesFromAssemblyContaining<Program>();
    }
}