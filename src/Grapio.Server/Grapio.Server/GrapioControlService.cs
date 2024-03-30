using Grpc.Core;

namespace Grapio.Server.Services;

public class GrapioControlService(IFeatureFlagRepository repository): ControlService.ControlServiceBase
{
    public override async Task FetchFeatureFlags(FeatureFlagsFetchRequest request, IServerStreamWriter<FeatureFlagsFetchReply> responseStream, ServerCallContext context)
    {
        ArgumentNullException.ThrowIfNull(repository);
        
        var featureFlags = repository.FetchFeatureFlags();

        foreach (var ff in featureFlags)
        {
            await responseStream.WriteAsync(new FeatureFlagsFetchReply
            {
                Key = ff.Item1,
                Consumer = ff.Item2
            });
        }
    }

    public override async Task FetchFeatureFlagsByKey(FeatureFlagsByKeyFetchRequest request, IServerStreamWriter<FeatureFlagFetchReply> responseStream, ServerCallContext context)
    {
        ArgumentNullException.ThrowIfNull(repository);

        var featureFlags = repository.FetchFeatureFlagsByKey(request.Key);
        await WriteFeatureFlagsToStream(responseStream, featureFlags, context.CancellationToken);
    }
    
    public override async Task FetchFeatureFlagsByConsumer(FeatureFlagsByConsumerFetchRequest request, IServerStreamWriter<FeatureFlagFetchReply> responseStream, ServerCallContext context)
    {
        ArgumentNullException.ThrowIfNull(repository);

        var featureFlags = repository.FetchFeatureFlagsByConsumer(request.Consumer);
        await WriteFeatureFlagsToStream(responseStream, featureFlags, context.CancellationToken);
    }

    public override async Task<FeatureFlagFetchReply> FetchFeatureFlagByKeyAndConsumer(FeatureFlagByKeyAndConsumerFetchRequest request, ServerCallContext context)
    {
        ArgumentNullException.ThrowIfNull(repository);

        var featureFlag = await repository.FetchFeatureFlagByKeyAndConsumer(request.Key, request.Consumer, context.CancellationToken);
        return featureFlag == null ? new FeatureFlagFetchReply { IsPopulated = false} : PopulateFeatureFlagFetchReply(featureFlag);
    }

    public override async Task<FeatureFlagControlReply> SetFeatureFlag(FeatureFlagSetRequest request, ServerCallContext context)
    {
        ArgumentNullException.ThrowIfNull(repository);
        
        var featureFlags = repository.FetchFeatureFlagsByKey(request.Key).ToArray();

        if (request.Consumer == Constants.UniversalConsumer && featureFlags.Any(ff => ff.Consumer != Constants.UniversalConsumer))
        {
            return new FeatureFlagControlReply
            {
                Success = false,
                Message = $"A specific consumer exists for {request.Key}. A universal consumer cannot be added for the same key."
            };
        }
        
        if (request.Consumer != Constants.UniversalConsumer && featureFlags.Any(ff => ff.Consumer == Constants.UniversalConsumer))
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
            Message = $"Successfully unset ({request.Key},{request.Consumer})."
        };
    }
    
    private static async Task WriteFeatureFlagsToStream(IAsyncStreamWriter<FeatureFlagFetchReply> responseStream, IEnumerable<FeatureFlag> featureFlags, CancellationToken cancellationToken)
    {
        foreach (var ff in featureFlags)
            await responseStream.WriteAsync(PopulateFeatureFlagFetchReply(ff), cancellationToken);
    }

    private static FeatureFlagFetchReply PopulateFeatureFlagFetchReply(FeatureFlag ff)
    {
        return new FeatureFlagFetchReply
        {
            Key = ff.FlagKey,
            Consumer = ff.Consumer,
            Value = ff.Value,
            IsPopulated = true
        };
    }
}
