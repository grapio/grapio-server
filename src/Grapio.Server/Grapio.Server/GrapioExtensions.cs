using Microsoft.EntityFrameworkCore;

namespace Grapio.Server;

public static class GrapioExtensions
{
    public static void AddGrapio(this IServiceCollection serviceCollection, Action<GrapioConfiguration> configuration)
    {
        var config = new GrapioConfiguration();
        configuration(config);
        
        serviceCollection.AddSingleton(config);

        serviceCollection.AddDbContext<GrapioDbContext>(options =>
        {
            options.UseSqlite(config.ConnectionString);
        });
        
        serviceCollection.AddTransient<IFeatureFlagRepository, FeatureFlagRepository>();
    }
}
