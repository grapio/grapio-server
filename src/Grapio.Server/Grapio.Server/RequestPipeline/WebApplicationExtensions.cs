using Correlate.AspNetCore;
using FastEndpoints;
using FastEndpoints.Swagger;

namespace Grapio.Server.RequestPipeline;

public static class WebApplicationExtensions
{
    public static WebApplication Configure(this WebApplication app)
    {
        app.UseCorrelate();
        app.UseHttpsRedirection();
        app.UseHsts();
        app.UseDefaultExceptionHandler();

        app.UseFastEndpoints(c =>
        {
            c.Versioning.Prefix = "v";
            c.Errors.UseProblemDetails();
        });

        app.UseSwaggerGen();

        return app;
    }
}