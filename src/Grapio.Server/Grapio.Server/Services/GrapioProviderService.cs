using Grpc.Core;

namespace Grapio.Server.Services;

public class GrapioProviderService(IFeatureFlagRepository repository): ProviderService.ProviderServiceBase
{
    public override async Task FetchFeatureFlags(FeatureFlagsRequest request, IServerStreamWriter<FeatureFlagReply> responseStream, ServerCallContext context)
    {
        ArgumentNullException.ThrowIfNull(repository, nameof(repository));
        
        
    }
}