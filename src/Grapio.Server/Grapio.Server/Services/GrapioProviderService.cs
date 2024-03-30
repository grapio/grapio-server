using System.Text;
using Google.Protobuf;
using Grpc.Core;

namespace Grapio.Server.Services;

internal class GrapioProviderService(IFeatureFlagRepository repository, ILogger<GrapioProviderService> logger): ProviderService.ProviderServiceBase
{
    public override async Task FetchFeatureFlags(FeatureFlagsRequest request, IServerStreamWriter<FeatureFlagReply> responseStream, ServerCallContext context)
    {
        ArgumentNullException.ThrowIfNull(repository, nameof(repository));
        ArgumentNullException.ThrowIfNull(logger, nameof(logger));

        logger.LogDebug("Fetching feature flags for {requester}", request.Requester);
        var fearFeatureFlags = repository.FetchFeatureFlagsByConsumer(request.Requester);

        foreach (var featureFlag in fearFeatureFlags)
        {
            var type = TypeDetector.Instance.Detect(featureFlag.Value!);
            logger.LogDebug("Detected {type} type for ({key},{consumer})", type, featureFlag.FlagKey, request.Requester);
            await WriteFeatureFlagToStream(responseStream, type, featureFlag, context.CancellationToken);
        }
    }

    private async Task WriteFeatureFlagToStream(
        IAsyncStreamWriter<FeatureFlagReply> responseStream, 
        OpenFeatureType type, 
        FeatureFlag featureFlag, 
        CancellationToken cancellationToken)
    {
        switch(type)
        {
            case OpenFeatureType.Boolean:
                await responseStream.WriteAsync(new FeatureFlagReply { 
                    Key = featureFlag.FlagKey, 
                    BooleanValue = Convert.ToBoolean(featureFlag.Value) 
                }, cancellationToken);
                break;
            case OpenFeatureType.Integer:
                await responseStream.WriteAsync(new FeatureFlagReply { 
                    Key = featureFlag.FlagKey, 
                    IntegerValue = Convert.ToInt32(featureFlag.Value) 
                }, cancellationToken);
                break;
            case OpenFeatureType.Double:
                await responseStream.WriteAsync(new FeatureFlagReply { 
                    Key = featureFlag.FlagKey, 
                    DoubleValue = Convert.ToDouble(featureFlag.Value) 
                }, cancellationToken);
                break;
            case OpenFeatureType.String:
                await responseStream.WriteAsync(new FeatureFlagReply { 
                    Key = featureFlag.FlagKey, 
                    StringValue = featureFlag.Value 
                }, cancellationToken);
                break;
            case OpenFeatureType.Structured:
                await responseStream.WriteAsync(new FeatureFlagReply { 
                    Key = featureFlag.FlagKey, 
                    StructureValue = ByteString.CopyFrom(featureFlag.Value, Encoding.UTF8) 
                }, cancellationToken);
                break;
            case OpenFeatureType.None:
            default:
                logger.LogError("Type is not supported for feature flag {key}", featureFlag.FlagKey);
                throw new NotSupportedException($"Type is not supported for feature flag {featureFlag.FlagKey}.");
        }
    }
}