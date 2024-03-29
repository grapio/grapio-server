using Grpc.Core;

namespace Grapio.Server.Services;

public class GrapioControlService(IFeatureFlagRepository repository): ControlService.ControlServiceBase
{
    public override Task FetchFeatureFlags(FeatureFlagsFetchRequest request, IServerStreamWriter<FeatureFlagsFetchReply> responseStream, ServerCallContext context)
    {
        ArgumentNullException.ThrowIfNull(repository);
        
        var featureFlags = repository.FetchFeatureFlags();

        foreach (var ff in featureFlags)
        {
            responseStream.WriteAsync(new FeatureFlagsFetchReply
            {
                Key = ff.Item1,
                Consumer = ff.Item2
            });
        }

        return Task.CompletedTask;
    }

    public override Task FetchFeatureFlagsByKey(FeatureFlagsByKeyFetchRequest request, IServerStreamWriter<FeatureFlagFetchReply> responseStream, ServerCallContext context)
    {
        return base.FetchFeatureFlagsByKey(request, responseStream, context);
    }
    
    public override Task FetchFeatureFlagsByConsumer(FeatureFlagsByConsumerFetchRequest request, IServerStreamWriter<FeatureFlagFetchReply> responseStream, ServerCallContext context)
    {
        return base.FetchFeatureFlagsByConsumer(request, responseStream, context);
    }

    public override Task<FeatureFlagFetchReply> FetchFeatureFlagByKeyAndConsumer(FeatureFlagByKeyAndConsumerFetchRequest request, ServerCallContext context)
    {
        return base.FetchFeatureFlagByKeyAndConsumer(request, context);
    }

    public override async Task<FeatureFlagControlReply> SetFeatureFlag(FeatureFlagSetRequest request, ServerCallContext context)
    {
        ArgumentNullException.ThrowIfNull(repository);
        
        var featureFlags = repository.FetchFeatureFlagsByKey(request.Key).ToArray();

        if (request.Consumer == "*" && featureFlags.Any(ff => ff.Consumer != "*"))
        {
            return new FeatureFlagControlReply
            {
                Success = false,
                Message = $"A specific consumer exists for {request.Key}. A universal consumer cannot be added for the same key."
            };
        }
        
        if (request.Consumer != "*" && featureFlags.Any(ff => ff.Consumer == "*"))
        {
            return new FeatureFlagControlReply
            {
                Success = false,
                Message = $"A universal consumer exists for {request.Key}. A specific consumer cannot be added for the same key."
            };
        }
        
        await repository.InsertOrUpdateFeatureFlag(new FeatureFlag(request.Key, request.Value, request.Consumer), context.CancellationToken);
        return new FeatureFlagControlReply { Success = true, Message = $"Successfully set ({request.Key},{request.Consumer})."};
    }

    public override async Task<FeatureFlagControlReply> UnsetFeatureFlag(FeatureFlagUnsetRequest request, ServerCallContext context)
    {
        ArgumentNullException.ThrowIfNull(repository);
        
        await repository.DeleteFeatureFlag(request.Key, request.Consumer, context.CancellationToken);
        
        return new FeatureFlagControlReply
        {
            Success = true, 
            Message = $"Successfully unset ({request.Key},{request.Consumer})"
        };
    }
}
