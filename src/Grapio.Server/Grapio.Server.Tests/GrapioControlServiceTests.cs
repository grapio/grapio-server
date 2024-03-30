using Grapio.Server.Services;
using Grpc.Core;
using Moq;

namespace Grapio.Server.Tests;

public class GrapioControlServiceTests
{
    private readonly Mock<IFeatureFlagRepository> _repository;
    private readonly GrapioControlService _service;

    public GrapioControlServiceTests()
    {
        _repository = new Mock<IFeatureFlagRepository>();
        _service = new GrapioControlService(_repository.Object);
    }
    
    [Fact]
    public async Task FetchFeatureFlags_should_write_all_feature_flags_to_stream()
    {
        var expected = new List<(string? key, string? consumer)>
        {
            new ValueTuple<string?, string?>("key-1", "consumer-1"),
            new ValueTuple<string?, string?>("key-2", "consumer-2")
        };
        
        _repository.Setup(r => r.FetchFeatureFlags()).Returns(expected);
        
        var responseStream = new Mock<IServerStreamWriter<FeatureFlagsFetchReply>>();
        
        responseStream.Setup(r => r.WriteAsync(
            It.Is<FeatureFlagsFetchReply>(m => m.Key == "key-1" && m.Consumer == "consumer-1")
        )).Verifiable();
        
        responseStream.Setup(r => r.WriteAsync(
            It.Is<FeatureFlagsFetchReply>(m => m.Key == "key-2" && m.Consumer == "consumer-2")
        )).Verifiable();
        
        await _service.FetchFeatureFlags(new FeatureFlagsFetchRequest(), responseStream.Object, new Mock<ServerCallContext>().Object);
        
        responseStream.Verify();
    }

    [Fact]
    public async Task SetFeatureFlag_should_return_error_when_adding_universal_consumer_and_specific_consumer_exists_for_the_same_key()
    {
        _repository.Setup(r => r.FetchFeatureFlagsByKey("key-1")).Returns([
            new FeatureFlag("key-1", "value-1", "consumer-1")
        ]);
        
        var result = await _service.SetFeatureFlag(new FeatureFlagSetRequest
        {
            Consumer = "*",
            Key = "key-1",
            Value = "value-1"
        }, new Mock<ServerCallContext>().Object);
        
        Assert.False(result.Success);
        Assert.Equal("A specific consumer exists for key-1. A universal consumer cannot be added for the same key.", result.Message);
    }
    
    [Fact]
    public async Task SetFeatureFlag_should_return_error_when_adding_specific_consumer_and_universal_consumer_exists_for_the_same_key()
    {
        _repository.Setup(r => r.FetchFeatureFlagsByKey("key-1")).Returns([
            new FeatureFlag("key-1", "value-1", "*")
        ]);
        
        var result = await _service.SetFeatureFlag(new FeatureFlagSetRequest
        {
            Consumer = "consumer-1",
            Key = "key-1",
            Value = "value-1"
        }, new Mock<ServerCallContext>().Object);
        
        Assert.False(result.Success);
        Assert.Equal("A universal consumer exists for key-1. A specific consumer cannot be added for the same key.", result.Message);
    }
    
    [Fact]
    public async Task SetFeatureFlag_should_insert_or_update_the_feature_flag()
    {
        _repository.Setup(r => r.FetchFeatureFlagsByKey("key-1")).Returns([]);
        _repository.Setup(r => r.InsertOrUpdateFeatureFlag(
            new FeatureFlag("key-1", "value-1", "consumer-1"), CancellationToken.None)
        ).Verifiable();
        
        var result = await _service.SetFeatureFlag(new FeatureFlagSetRequest
        {
            Consumer = "consumer-1",
            Key = "key-1",
            Value = "value-1"
        }, new Mock<ServerCallContext>().Object);

        _repository.Verify();
        Assert.True(result.Success);
        Assert.Equal("Successfully set (key-1,consumer-1).", result.Message);
    }
    
    [Fact]
    public async Task UnsetFeatureFlag_should_delete_the_feature_flag()
    {
        _repository.Setup(r => r.FetchFeatureFlagsByKey("key-1")).Returns([]);
        _repository.Setup(r => r.DeleteFeatureFlag("key-1", "consumer-1", CancellationToken.None)).Verifiable();
        
        var result = await _service.UnsetFeatureFlag(new FeatureFlagUnsetRequest
        {
            Key = "key-1",
            Consumer = "consumer-1"
        }, new Mock<ServerCallContext>().Object);

        _repository.Verify();
        Assert.True(result.Success);
        Assert.Equal("Successfully unset (key-1,consumer-1).", result.Message);
    }
    
    [Fact]
    public async Task FetchFeatureFlagsByKey_should_write_feature_flags_to_stream_that_matches_the_requested_key()
    {
        var expected = new [] {
            new FeatureFlag("key-1", "value-1", "consumer-1"),
            new FeatureFlag("key-1", "value-1", "consumer-2")
        };
        
        _repository.Setup(r => r.FetchFeatureFlagsByKey("key-1")).Returns(expected);
        
        var responseStream = new Mock<IServerStreamWriter<FeatureFlagFetchReply>>();
        
        responseStream.Setup(r => r.WriteAsync(
            It.Is<FeatureFlagFetchReply>(m => m.Key == "key-1" && m.Consumer == "consumer-1"),
            It.IsAny<CancellationToken>()
        )).Verifiable();
        
        responseStream.Setup(r => r.WriteAsync(
            It.Is<FeatureFlagFetchReply>(m => m.Key == "key-1" && m.Consumer == "consumer-2"),
            It.IsAny<CancellationToken>()
        )).Verifiable();
        
        await _service.FetchFeatureFlagsByKey(new FeatureFlagsByKeyFetchRequest
        {
            Key = "key-1"
        }, responseStream.Object, new Mock<ServerCallContext>().Object);
        
        responseStream.Verify();
    }
    
    [Fact]
    public async Task FetchFeatureFlagsByConsumer_should_write_feature_flags_to_stream_that_matches_the_requested_consumer()
    {
        var expected = new [] {
            new FeatureFlag("key-1", "value-1", "consumer-1"),
            new FeatureFlag("key-2", "value-2", "consumer-1")
        };
        
        _repository.Setup(r => r.FetchFeatureFlagsByConsumer("consumer-1")).Returns(expected);
        
        var responseStream = new Mock<IServerStreamWriter<FeatureFlagFetchReply>>();
        
        responseStream.Setup(r => r.WriteAsync(
            It.Is<FeatureFlagFetchReply>(m => m.Key == "key-1" && m.Consumer == "consumer-1"),
            It.IsAny<CancellationToken>()
        )).Verifiable();
        
        responseStream.Setup(r => r.WriteAsync(
            It.Is<FeatureFlagFetchReply>(m => m.Key == "key-2" && m.Consumer == "consumer-1"), 
            It.IsAny<CancellationToken>()
        )).Verifiable();
        
        await _service.FetchFeatureFlagsByConsumer(new FeatureFlagsByConsumerFetchRequest
        {
            Consumer = "consumer-1"
        }, responseStream.Object, new Mock<ServerCallContext>().Object);
        
        responseStream.Verify();
    }
    
    [Fact]
    public async Task FetchFeatureFlagsByKeyAndConsumer_should_return_the_feature_flag_that_matches_the_requested_key_and_consumer()
    {
        var expected = new FeatureFlag("key-1", "value-1", "consumer-2");
   
        _repository.Setup(r => r.FetchFeatureFlagByKeyAndConsumer("key-1", "consumer-2", It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);
        
        var result = await _service.FetchFeatureFlagByKeyAndConsumer(new FeatureFlagByKeyAndConsumerFetchRequest
        {
            Key = "key-1",
            Consumer = "consumer-2"
        }, new Mock<ServerCallContext>().Object);
        
        Assert.Equal(expected.Consumer, result.Consumer);
        Assert.Equal(expected.FlagKey, result.Key);
        Assert.Equal(expected.Value, result.Value);
        Assert.True(result.IsPopulated);
    }
    
    [Fact]
    public async Task FetchFeatureFlagsByKeyAndConsumer_should_return_an_unpopulated_feature_flag_when_not_matched()
    {
        _repository.Setup(r => r.FetchFeatureFlagByKeyAndConsumer("key-1", "consumer-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync((FeatureFlag)null!);
        
        var result = await _service.FetchFeatureFlagByKeyAndConsumer(new FeatureFlagByKeyAndConsumerFetchRequest
        {
            Key = "key-1",
            Consumer = "consumer-1"
        }, new Mock<ServerCallContext>().Object);
        
        Assert.False(result.IsPopulated);
    }
}
