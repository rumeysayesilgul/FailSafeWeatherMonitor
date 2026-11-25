using FailSafeWeatherMonitor.Data;
using Microsoft.EntityFrameworkCore;

await using (var db = new WeatherDbContext())
{
    await db.Database.MigrateAsync();
}

Console.WriteLine("Databasen är redo.");
