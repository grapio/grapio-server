using System.Runtime.CompilerServices;
using Grapio.Server;
using Grapio.Server.Services;
using Microsoft.EntityFrameworkCore;

[assembly:InternalsVisibleTo("Grapio.Server.Tests")]

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .SetBasePath(builder.Environment.ContentRootPath)
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
    .AddEnvironmentVariables();

builder.Services.AddGrpc();

builder.Services.AddGrapio(config =>
{
    builder.Configuration.GetSection("Grapio").Bind(config);
});

var app = builder.Build();

using (var serviceScope = app.Services.GetRequiredService<IServiceScopeFactory>().CreateScope())
{
    var context = serviceScope.ServiceProvider.GetRequiredService<GrapioDbContext>();
    context.Database.Migrate();
}

app.MapGrpcService<GrapioProviderService>();
app.MapGrpcService<GrapioControlService>();

app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client.");

app.Run();
