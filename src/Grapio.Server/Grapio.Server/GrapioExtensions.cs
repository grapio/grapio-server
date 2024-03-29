using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Grapio.Server;

public static class GrapioExtensions
{
    public static void AddGrapio(this IServiceCollection serviceCollection, Action<GrapioConfiguration> configuration)
    {
        var config = new GrapioConfiguration();
        configuration(config);

        serviceCollection.AddDbContext<GrapioDbContext>(options =>
        {
            options.UseSqlite(config.ConnectionString);
        });
        
        serviceCollection.AddTransient<IFeatureFlagRepository, FeatureFlagRepository>();
    }
}
