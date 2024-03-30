using System.Text;
using Google.Protobuf;
using Grpc.Core;

namespace Grapio.Server.Services;

internal class GrapioProviderService(IFeatureFlagRepository repository): ProviderService.ProviderServiceBase
{
    public override async Task FetchFeatureFlags(FeatureFlagsRequest request, IServerStreamWriter<FeatureFlagReply> responseStream, ServerCallContext context)
    {
        ArgumentNullException.ThrowIfNull(repository, nameof(repository));

        var fearFeatureFlags = repository.FetchFeatureFlagsByConsumer(request.Requester);

        foreach (var featureFlag in fearFeatureFlags)
        {
            var type = TypeDetector.Instance.Detect(featureFlag.Value!);
            
            switch(type)
            {
                case OpenFeatureType.Boolean:
                    await responseStream.WriteAsync(new FeatureFlagReply { 
                        Key = featureFlag.FlagKey, 
                        BooleanValue = Convert.ToBoolean(featureFlag.Value) 
                    });
                    break;
                case OpenFeatureType.Integer:
                    await responseStream.WriteAsync(new FeatureFlagReply { 
                        Key = featureFlag.FlagKey, 
                        IntegerValue = Convert.ToInt32(featureFlag.Value) 
                    });
                    break;
                case OpenFeatureType.Double:
                    await responseStream.WriteAsync(new FeatureFlagReply { 
                        Key = featureFlag.FlagKey, 
                        DoubleValue = Convert.ToDouble(featureFlag.Value) 
                    });
                    break;
                case OpenFeatureType.String:
                    await responseStream.WriteAsync(new FeatureFlagReply { 
                        Key = featureFlag.FlagKey, 
                        StringValue = featureFlag.Value 
                    });
                    break;
                case OpenFeatureType.Structured:
                    await responseStream.WriteAsync(new FeatureFlagReply { 
                        Key = featureFlag.FlagKey, 
                        StructureValue = ByteString.CopyFrom(featureFlag.Value, Encoding.UTF8) 
                    });
                    break;
                case OpenFeatureType.None:
                default:
                    throw new NotSupportedException($"Type is not supported for feature flag {featureFlag.FlagKey}.");
            }
        }
    }
}