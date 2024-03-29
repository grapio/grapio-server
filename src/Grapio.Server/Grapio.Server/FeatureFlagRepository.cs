using Microsoft.EntityFrameworkCore.Internal;

namespace Grapio.Server;

public interface IFeatureFlagRepository
{
    Task InsertOrUpdateFeatureFlag(FeatureFlag featureFlag, CancellationToken cancellationToken);
    Task DeleteFeatureFlag(string key, string consumer, CancellationToken cancellationToken);
    IEnumerable<FeatureFlag> FetchFeatureFlagsByKey(string key);
    IEnumerable<FeatureFlag> FetchFeatureFlagsByConsumer(string consumer);
    Task<FeatureFlag?> FetchFeatureFlagByKeyAndConsumer(string key, string consumer, CancellationToken cancellationToken);
    IEnumerable<(string? Key, string? Consumer)> FetchFeatureFlags();
}

internal class FeatureFlagRepository(GrapioDbContext dbContext, ILogger<FeatureFlagRepository> logger) : IFeatureFlagRepository
{
    private const string UniversalConsumer = "*";
    
    public async Task InsertOrUpdateFeatureFlag(FeatureFlag featureFlag, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(dbContext, nameof(dbContext));
        ArgumentNullException.ThrowIfNull(logger, nameof(logger));
        ArgumentNullException.ThrowIfNull(featureFlag, nameof(featureFlag));

        var existingFlag = await dbContext.FeatureFlags.FindAsync([featureFlag.FlagKey, featureFlag.Consumer], cancellationToken);
        
        if (existingFlag == null)
        {
            await dbContext.AddAsync(featureFlag, cancellationToken);
            logger.LogDebug("Inserted feature flag {featureFlag}", featureFlag);
        }
        else
        {
            dbContext.Entry(existingFlag).CurrentValues.SetValues(featureFlag);
            logger.LogDebug("Updated feature flag {featureFlag}", featureFlag);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Successfully saved feature flag {featureFlag}", featureFlag);
    }
    
    public async Task DeleteFeatureFlag(string key, string consumer, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(dbContext, nameof(dbContext));
        ArgumentNullException.ThrowIfNull(logger, nameof(logger));
        ArgumentException.ThrowIfNullOrEmpty(key, nameof(key));
        ArgumentException.ThrowIfNullOrEmpty(consumer, nameof(consumer));

        logger.LogDebug("Finding ({key},{consumer}) to delete", key, consumer);
        var featureFlag = await dbContext.FeatureFlags.FindAsync([key, consumer], cancellationToken);

        if (featureFlag == null)
        {
            logger.LogWarning("No match found for ({key},{consumer}) to delete", key, consumer);
            return;
        }

        dbContext.FeatureFlags.Remove(featureFlag);
        await dbContext.SaveChangesAsync(cancellationToken);
        
        logger.LogInformation("Successfully deleted ({key},{consumer})", key, consumer);
    }
    
    public IEnumerable<FeatureFlag> FetchFeatureFlagsByKey(string key)
    {
        ArgumentNullException.ThrowIfNull(dbContext, nameof(dbContext));
        ArgumentNullException.ThrowIfNull(logger, nameof(logger));
        ArgumentException.ThrowIfNullOrEmpty(key, nameof(key));

        logger.LogDebug("Fetching feature flags that match key {key}", key);
        return dbContext.FeatureFlags.Where(ff => ff.FlagKey == key);
    }

    public IEnumerable<FeatureFlag> FetchFeatureFlagsByConsumer(string consumer)
    {
        ArgumentNullException.ThrowIfNull(dbContext, nameof(dbContext));
        ArgumentNullException.ThrowIfNull(logger, nameof(logger));
        ArgumentException.ThrowIfNullOrEmpty(consumer, nameof(consumer));

        logger.LogDebug("Fetching feature flags that match consumer {consumer} or *", consumer);
        return dbContext.FeatureFlags.Where(ff => ff.Consumer == consumer || ff.Consumer == UniversalConsumer);
    }
    
    public async Task<FeatureFlag?> FetchFeatureFlagByKeyAndConsumer(string key, string consumer, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(dbContext, nameof(dbContext));
        ArgumentNullException.ThrowIfNull(logger, nameof(logger));
        ArgumentException.ThrowIfNullOrEmpty(key, nameof(key));
        ArgumentException.ThrowIfNullOrEmpty(consumer, nameof(consumer));
        
        logger.LogDebug("Fetching feature flag that match ({key},{consumer})", key, consumer);
        return await dbContext.FeatureFlags.FindAsync([key, consumer], cancellationToken);
    }

    public IEnumerable<(string? Key, string? Consumer)> FetchFeatureFlags()
    {
        ArgumentNullException.ThrowIfNull(dbContext, nameof(dbContext));
        ArgumentNullException.ThrowIfNull(logger, nameof(logger));

        logger.LogDebug("Fetching all feature flags");
        var featureFlags = dbContext.FeatureFlags.Select(ff => new { ff.FlagKey, ff.Consumer });
        
        foreach (var featureFlag in featureFlags)
            yield return (featureFlag.FlagKey, featureFlag.Consumer);
    }
}
