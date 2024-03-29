using System.Runtime.CompilerServices;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace Grapio.Server.Tests;

public class FeatureFlagRepositoryTests: IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly DbContextOptions<GrapioDbContext> _options;
    private readonly GrapioDbContext _context;
    private readonly FeatureFlagRepository _repository;

    public FeatureFlagRepositoryTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        _options = new DbContextOptionsBuilder<GrapioDbContext>()
            .UseSqlite(_connection)
            .Options;

        _context = new GrapioDbContext(_options);
        _context.Database.EnsureCreated();
        
        _repository = new FeatureFlagRepository(_context, NullLogger<FeatureFlagRepository>.Instance);
    }
    
    [Fact]
    public async Task InsertOrUpdate_should_insert_the_feature_flag_if_it_does_not_exist()
    {
        await DeleteFeatureFlags(_context);
            
        var expected = new FeatureFlag("key-1", "value-1", "service-1");
        await _repository.InsertOrUpdateFeatureFlag(expected, CancellationToken.None);

        var result = await _context.FindAsync<FeatureFlag>(["key-1", "service-1"]);
        Assert.Equal(expected, result);
    }

    [Fact]
    public async Task InsertOrUpdate_should_update_the_feature_flag_if_it_exists()
    {
        await DeleteFeatureFlags(_context);
        
        await _context.Database.ExecuteSqlAsync(
            FormattableStringFactory.Create("INSERT INTO FeatureFlags (Key, Value, Consumer) VALUES ('key-1', 'value-1', 'service-1')")
        );
        
        var expected = new FeatureFlag("key-1", "value-2", "service-1");
        await _repository.InsertOrUpdateFeatureFlag(expected, CancellationToken.None);

        var result = await _context.FindAsync<FeatureFlag>(["key-1", "service-1"]);
        Assert.Equal(expected, result);
    }
    
    [Fact]
    public async Task DeleteFeatureFlag_should_remove_the_feature_flag_from_the_database()
    {
        await DeleteFeatureFlags(_context);
        
        await _context.Database.ExecuteSqlAsync(
            FormattableStringFactory.Create("INSERT INTO FeatureFlags (Key, Value, Consumer) VALUES ('key-1', 'value-1', 'service-1')")
        );
        
        var repository = new FeatureFlagRepository(_context, NullLogger<FeatureFlagRepository>.Instance);
        await repository.DeleteFeatureFlag("key-1", "service-1", CancellationToken.None);

        Assert.Null(await _context.FindAsync<FeatureFlag>(["key-1", "service-1"], CancellationToken.None));
    }
    
    [Fact]
    public async Task FetchFeatureFlagByKeyAndConsumer_should_return_the_feature_flag_by_key_and_consumer()
    {
        await DeleteFeatureFlags(_context);
        
        await _context.Database.ExecuteSqlAsync(
            FormattableStringFactory.Create("INSERT INTO FeatureFlags (Key, Value, Consumer) VALUES ('key-1', 'value-1', 'service-1')")
        );
        await _context.Database.ExecuteSqlAsync(
            FormattableStringFactory.Create("INSERT INTO FeatureFlags (Key, Value, Consumer) VALUES ('key-1', 'value-1', 'service-2')")
        );
        await _context.Database.ExecuteSqlAsync(
            FormattableStringFactory.Create("INSERT INTO FeatureFlags (Key, Value, Consumer) VALUES ('key-1', 'value-1', '*')")
        );
        await _context.Database.ExecuteSqlAsync(
            FormattableStringFactory.Create("INSERT INTO FeatureFlags (Key, Value, Consumer) VALUES ('key-3', 'value-1', 'service-1')")
        );
        
        var result = await _repository.FetchFeatureFlagByKeyAndConsumer("key-1", "service-1", CancellationToken.None);

        var expected = new FeatureFlag("key-1", "value-1", "service-1");
        Assert.Equal(expected, result);
    }
    
    [Fact]
    public async Task FetchFeatureFlagsByConsumer_should_return_the_feature_flags_for_a_consumer()
    {
        await DeleteFeatureFlags(_context);
        
        await _context.Database.ExecuteSqlAsync(
            FormattableStringFactory.Create("INSERT INTO FeatureFlags (Key, Value, Consumer) VALUES ('key-1', 'value-1', 'service-1')")
        );
        await _context.Database.ExecuteSqlAsync(
            FormattableStringFactory.Create("INSERT INTO FeatureFlags (Key, Value, Consumer) VALUES ('key-2', 'value-2', '*')")
        );
        await _context.Database.ExecuteSqlAsync(
            FormattableStringFactory.Create("INSERT INTO FeatureFlags (Key, Value, Consumer) VALUES ('key-3', 'value-3', 'service-2')")
        );
        
        var result = _repository.FetchFeatureFlagsByConsumer("service-1");

        var expected = new[] {
            new FeatureFlag("key-1", "value-1", "service-1"), 
            new FeatureFlag("key-2", "value-2", "*")
        };
        
        Assert.Equal(expected, result);
    }
    
    [Fact]
    public async Task FetchFeatureFlagsByKey_should_return_the_feature_flags_for_a_key()
    {
        await DeleteFeatureFlags(_context);
        
        await _context.Database.ExecuteSqlAsync(
            FormattableStringFactory.Create("INSERT INTO FeatureFlags (Key, Value, Consumer) VALUES ('key-1', 'value-1', 'service-1')")
        );
        await _context.Database.ExecuteSqlAsync(
            FormattableStringFactory.Create("INSERT INTO FeatureFlags (Key, Value, Consumer) VALUES ('key-1', 'value-2', 'service-2')")
        );
        await _context.Database.ExecuteSqlAsync(
            FormattableStringFactory.Create("INSERT INTO FeatureFlags (Key, Value, Consumer) VALUES ('key-3', 'value-3', 'service-2')")
        );
        
        var result = _repository.FetchFeatureFlagsByKey("key-1");

        var expected = new[] {
            new FeatureFlag("key-1", "value-1", "service-1"), 
            new FeatureFlag("key-1", "value-2", "service-2")
        };
        
        Assert.Equal(expected, result);
    }
    
    [Fact]
    public async Task FetchFeatureFlags_should_return_an_enumerable_of_feature_flag_keys_and_consumers()
    {
        await DeleteFeatureFlags(_context);
        await _context.Database.ExecuteSqlAsync(
            FormattableStringFactory.Create("INSERT INTO FeatureFlags (Key, Value, Consumer) VALUES ('key-1', 'value-1', 'service-1')")
        );
        await _context.Database.ExecuteSqlAsync(
            FormattableStringFactory.Create("INSERT INTO FeatureFlags (Key, Value, Consumer) VALUES ('key-2', 'value-2', 'service-1')")
        );
        
        var result = _repository.FetchFeatureFlags();

        var expected = new List<(string?, string?)>
        {
            ("key-1", "service-1"), 
            ("key-2", "service-1")
        };
        
        Assert.Equal(expected, result);
    }
    
    [Fact]
    public async Task FetchFeatureFlagKeys_should_return_an_empty_enumerable_when_the_table_is_empty()
    {
        await using var context = new GrapioDbContext(_options);

        await DeleteFeatureFlags(context);
        var repository = new FeatureFlagRepository(context, NullLogger<FeatureFlagRepository>.Instance);
        var result = repository.FetchFeatureFlags();

        Assert.NotNull(result);
        Assert.Empty(result);
    }

    private static async Task DeleteFeatureFlags(DbContext context)
    {
        await context.Database.ExecuteSqlAsync(FormattableStringFactory.Create("DELETE FROM FeatureFlags"));
    }
    
    public void Dispose()
    {
        _connection.Close();
        GC.SuppressFinalize(this);
    }
}
