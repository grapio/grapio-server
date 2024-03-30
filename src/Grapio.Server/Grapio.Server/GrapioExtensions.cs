using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace Grapio.Server;

public static class GrapioExtensions
{
    public static void AddGrapio(this IServiceCollection serviceCollection, Action<GrapioConfiguration> config)
    {
        var configuration = new GrapioConfiguration();
        config(configuration);
        new GrapioConfigurationValidator().ValidateAndThrow(configuration);
       
        serviceCollection.AddSingleton(configuration);
        serviceCollection.AddTransient<IFeatureFlagRepository, FeatureFlagRepository>();
        serviceCollection.AddDbContext<GrapioDbContext>(options =>
        {
            options.UseSqlite(configuration.ConnectionString);
        });
    }
}
