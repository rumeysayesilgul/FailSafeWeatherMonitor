using FailSafeWeatherMonitor.Data;
using FailSafeWeatherMonitor.Models;
using Microsoft.EntityFrameworkCore;

namespace FailSafeWeatherMonitor.Services;

public class WeatherStatisticsService
{
    public async Task<List<string>> GetStationsAsync()
    {
        await using var db = new WeatherDbContext();

        return await db.WeatherDataPoints
            .Select(point => point.StationName)
            .Distinct()
            .OrderBy(stationName => stationName)
            .ToListAsync();
    }

    public async Task<List<WeatherDataPoint>> GetMeasurementsAsync(
        string stationName)
    {
        await using var db = new WeatherDbContext();

        return await db.WeatherDataPoints
            .AsNoTracking()
            .Where(point => point.StationName == stationName)
            .OrderBy(point => point.Timestamp)
            .ToListAsync();
    }
}