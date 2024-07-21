using Grapio.Server.DependencyInjection;
using Grapio.Server.RequestPipeline;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddEnvironmentVariables();

builder.Services.ConfigureServices(builder.Environment);

builder.Logging.AddOpenTelemetry(opts =>
{
    opts.IncludeScopes = true;
    opts.IncludeFormattedMessage = true;
});

builder.Build().Configure().Run();
