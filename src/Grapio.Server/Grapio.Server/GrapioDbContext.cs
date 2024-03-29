using Microsoft.EntityFrameworkCore;

namespace Grapio.Server;

internal class GrapioDbContext(DbContextOptions<GrapioDbContext> options) : DbContext(options)
{
    public DbSet<FeatureFlag> FeatureFlags { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<FeatureFlag>()
            .Property(ff => ff.FlagKey)
            .IsRequired()
            .HasMaxLength(50)
            .HasColumnName("Key")
            .HasColumnType("VARCHAR(50)");
        
        modelBuilder.Entity<FeatureFlag>()
            .Property(ff => ff.Value)
            .IsRequired()
            .HasColumnName("Value")
            .HasColumnType("TEXT");
        
        modelBuilder.Entity<FeatureFlag>()
            .Property(ff => ff.Consumer)
            .HasDefaultValue("*")
            .HasColumnName("Consumer")
            .HasColumnType("VARCHAR(150)");

        modelBuilder.Entity<FeatureFlag>(builder => builder.HasKey(ff => new { ff.FlagKey, ff.Consumer} ));
        
        base.OnModelCreating(modelBuilder);
    }
}
