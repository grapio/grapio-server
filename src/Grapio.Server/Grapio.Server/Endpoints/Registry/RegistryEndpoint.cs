using FastEndpoints;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Grapio.Server.Endpoints.Registry;

public class RegistryEndpoint : Endpoint<RegistrationRequest, RegistrationResponse>
{
    public override void Configure()
    {
        Post("/registry");
        Version(1);
        
        Options(opts =>
        {
            opts.Accepts<RegistrationRequest>("application/json");
            opts.Produces<RegistrationResponse>(200, "application/json");
            opts.Produces<BadRequest>(400, "application/json");
            opts.Produces<Conflict>(409, "application/json");
            opts.Produces<InternalErrorResponse>(500, "application/json");
        });
        
        Summary(s =>
        {
            s.Summary = "Registers a service in the registry.";
            s.Description = "Registers a service in the registry and responds with the configuration for the specific service based on its name.";
            s.Responses[200] = "Service is registered successfully.";
            s.Responses[400] = "The request for registration failed.";
            s.Responses[409] = "The service instance is already registered in the registry.";
            s.Responses[500] = "The server failed to register the service in the registry.";
        });
        
        AllowAnonymous();
    }

    public override async Task HandleAsync(RegistrationRequest req, CancellationToken ct)
    {
        await SendAsync(new RegistrationResponse(), 200, ct);
    }
}