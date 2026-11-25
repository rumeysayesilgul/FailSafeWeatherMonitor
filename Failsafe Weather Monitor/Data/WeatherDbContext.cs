using FailSafeWeatherMonitor.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace FailSafeWeatherMonitor.Data;

public class WeatherDbContext : DbContext
{
    public DbSet<WeatherDataPoint> WeatherDataPoints { get; set; } = null!;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json")
            .Build();

        var connectionString = configuration
            .GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException(
                "Anslutningssträngen DefaultConnection saknas i appsettings.json.");

        optionsBuilder.UseSqlServer(connectionString);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<WeatherDataPoint>()
            .HasIndex(point => new { point.StationName, point.Timestamp })
            .IsUnique();

        modelBuilder.Entity<WeatherDataPoint>()
            .Property(point => point.Temperature)
            .HasPrecision(6, 3);

        modelBuilder.Entity<WeatherDataPoint>()
            .Property(point => point.Humidity)
            .HasPrecision(6, 3);
    }
}