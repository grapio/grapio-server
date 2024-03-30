using System.Diagnostics.CodeAnalysis;
using System.Text;
using Google.Protobuf;
using Grapio.Server.Services;
using Grpc.Core;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace Grapio.Server.Tests;

public class GrapioProviderServiceTests
{
    private GrapioProviderService _service;
    private Mock<IFeatureFlagRepository> _repository;
    private Mock<IServerStreamWriter<FeatureFlagReply>> _responseStream;

    public GrapioProviderServiceTests()
    {
        _repository = new Mock<IFeatureFlagRepository>();
        _service = new GrapioProviderService(_repository.Object, NullLogger<GrapioProviderService>.Instance);
        _responseStream = new Mock<IServerStreamWriter<FeatureFlagReply>>();
    }
    
    [Fact]
    public void FetchFeatureFlags_must_write_the_feature_flag_to_the_response_stream_for_boolean_values()
    {
        var featureFlags = new List<FeatureFlag>
        {
            new("key-1", "true", "consumer-1"),
            new("key-2", "false", "consumer-1")
        };
        
        SetupRepositoryWith(featureFlags);

        _responseStream.Setup(s => s.WriteAsync(
            It.Is<FeatureFlagReply>(m => m.Key == "key-1" && m.BooleanValue), 
            It.IsAny<CancellationToken>())
        ).Verifiable();
        
        _responseStream.Setup(s => s.WriteAsync(
            It.Is<FeatureFlagReply>(m => m.Key == "key-2" && !m.BooleanValue), 
            It.IsAny<CancellationToken>())
        ).Verifiable();
        
        _ = _service.FetchFeatureFlags(new FeatureFlagsRequest
        {
            Requester = "consumer-1"
        }, _responseStream.Object, new Mock<ServerCallContext>().Object);
        
        _repository.Verify();
        _responseStream.Verify();
    }
    
    [Fact]
    public void FetchFeatureFlags_must_write_the_feature_flag_to_the_response_stream_for_integer_values()
    {
        var featureFlags = new List<FeatureFlag>
        {
            new("key-1", "102", "consumer-1"),
            new("key-2", "50", "consumer-1")
        };
        
        SetupRepositoryWith(featureFlags);
        
        _responseStream.Setup(s => s.WriteAsync(
            It.Is<FeatureFlagReply>(m => m.Key == "key-1" && m.IntegerValue == 102), 
            It.IsAny<CancellationToken>())
        ).Verifiable();
        
        _responseStream.Setup(s => s.WriteAsync(
            It.Is<FeatureFlagReply>(m => m.Key == "key-2" && m.IntegerValue == 50), 
            It.IsAny<CancellationToken>())
        ).Verifiable();
        
        _ = _service.FetchFeatureFlags(new FeatureFlagsRequest
        {
            Requester = "consumer-1"
        }, _responseStream.Object, new Mock<ServerCallContext>().Object);
        
        _repository.Verify();
        _responseStream.Verify();
    }
    
    [Fact]
    [SuppressMessage("ReSharper", "CompareOfFloatsByEqualityOperator")]
    public void FetchFeatureFlags_must_write_the_feature_flag_to_the_response_stream_for_double_values()
    {
        var featureFlags = new List<FeatureFlag>
        {
            new("key-1", "3.142", "consumer-1"),
            new("key-2", "2.7", "consumer-1")
        };
        
        SetupRepositoryWith(featureFlags);
        
        _responseStream.Setup(s => s.WriteAsync(
            It.Is<FeatureFlagReply>(m => m.Key == "key-1" && m.DoubleValue == 3.142 ), 
            It.IsAny<CancellationToken>())
        ).Verifiable();
        
        _responseStream.Setup(s => s.WriteAsync(
            It.Is<FeatureFlagReply>(m => m.Key == "key-2" && m.DoubleValue == 2.7), 
            It.IsAny<CancellationToken>())
        ).Verifiable();
        
        _ = _service.FetchFeatureFlags(new FeatureFlagsRequest
        {
            Requester = "consumer-1"
        }, _responseStream.Object, new Mock<ServerCallContext>().Object);
        
        _repository.Verify();
        _responseStream.Verify();
    }
    
    [Fact]
    public void FetchFeatureFlags_must_write_the_feature_flag_to_the_response_stream_for_structured_values()
    {
        var featureFlags = new List<FeatureFlag>
        {
            new("key-1", "<note><to>Tove</to><from>Jani</from><heading>Reminder</heading><body>Don't forget me this weekend!</body></note>", "consumer-1"),
            new("key-2", "---" + 
                         "root: {}", "consumer-1"),
            new("key-3", "{\"Name\":\"Jani\"}", "*"),
            new("key-4", "[{\"Name\":\"Jani\"},{\"Name\":\"Tove\"}]", "*")
        };
        
        SetupRepositoryWith(featureFlags);
        
        _responseStream.Setup(s => s.WriteAsync(
            It.Is<FeatureFlagReply>(m => m.Key == "key-1" && m.StructureValue == ByteString.CopyFrom(featureFlags[0].Value, Encoding.UTF8)), 
            It.IsAny<CancellationToken>())
        ).Verifiable();
        
        _responseStream.Setup(s => s.WriteAsync(
            It.Is<FeatureFlagReply>(m => m.Key == "key-2" && m.StructureValue == ByteString.CopyFrom(featureFlags[1].Value, Encoding.UTF8)),
            It.IsAny<CancellationToken>())
        ).Verifiable();
        
        _responseStream.Setup(s => s.WriteAsync(
            It.Is<FeatureFlagReply>(m => m.Key == "key-3" && m.StructureValue == ByteString.CopyFrom(featureFlags[2].Value, Encoding.UTF8)),
            It.IsAny<CancellationToken>())
        ).Verifiable();
        
        _responseStream.Setup(s => s.WriteAsync(
            It.Is<FeatureFlagReply>(m => m.Key == "key-4" && m.StructureValue == ByteString.CopyFrom(featureFlags[3].Value, Encoding.UTF8)),
            It.IsAny<CancellationToken>())
        ).Verifiable();
        
        _ = _service.FetchFeatureFlags(new FeatureFlagsRequest
        {
            Requester = "consumer-1"
        }, _responseStream.Object, new Mock<ServerCallContext>().Object);
        
        _repository.Verify();
        _responseStream.Verify();
    }
    
    [Fact]
    public void FetchFeatureFlags_must_write_the_feature_flag_to_the_response_stream_for_string_values()
    {
        var featureFlags = new List<FeatureFlag>
        {
            new("key-1", "Tove", "consumer-1"),
            new("key-2", "Jani", "consumer-1")
        };
        
        SetupRepositoryWith(featureFlags);
        
        _responseStream.Setup(s => s.WriteAsync(
            It.Is<FeatureFlagReply>(m => m.Key == "key-1" && m.StringValue == "Tove" ), 
            It.IsAny<CancellationToken>())
        ).Verifiable();
        
        _responseStream.Setup(s => s.WriteAsync(
            It.Is<FeatureFlagReply>(m => m.Key == "key-2" && m.StringValue == "Jani"), 
            It.IsAny<CancellationToken>())
        ).Verifiable();
        
        _ = _service.FetchFeatureFlags(new FeatureFlagsRequest
        {
            Requester = "consumer-1"
        }, _responseStream.Object, new Mock<ServerCallContext>().Object);
        
        _repository.Verify();
        _responseStream.Verify();
    }

    private void SetupRepositoryWith(IEnumerable<FeatureFlag> featureFlags)
    {
        _repository.Setup(r => r.FetchFeatureFlagsByConsumer("consumer-1"))
            .Returns(featureFlags)
            .Verifiable();
    }
}
