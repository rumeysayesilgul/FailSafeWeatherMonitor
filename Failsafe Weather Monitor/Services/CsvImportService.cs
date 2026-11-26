using System.Globalization;
using FailSafeWeatherMonitor.Data;
using FailSafeWeatherMonitor.Models;
using Microsoft.EntityFrameworkCore;

namespace FailSafeWeatherMonitor.Services;

public class CsvImportService
{
    public async Task<(int Imported, int Rejected, int Skipped)>
        ImportAsync(string filePath)
    {
        var result = await ReadValidDataAsync(filePath);

        await using var db = new WeatherDbContext();

        var seen = new HashSet<(string StationName, DateTime Timestamp)>();
        int skipped = 0;

        foreach (WeatherDataPoint dataPoint in result.DataPoints)
        {
            var key = (
                dataPoint.StationName.ToUpperInvariant(),
                dataPoint.Timestamp);

            if (!seen.Add(key))
            {
                skipped++;
                continue;
            }

            bool exists = await db.WeatherDataPoints.AnyAsync(point =>
                point.StationName == dataPoint.StationName &&
                point.Timestamp == dataPoint.Timestamp);

            if (exists)
            {
                skipped++;
                continue;
            }

            await db.WeatherDataPoints.AddAsync(dataPoint);
        }

        int imported = await db.SaveChangesAsync();

        return (imported, result.Rejected, skipped);
    }

    public async Task<(List<WeatherDataPoint> DataPoints, int Rejected)>
        ReadValidDataAsync(string filePath)
    {
        string logPath = Path.Combine(
            AppContext.BaseDirectory, "bad_data.log");

        await File.WriteAllTextAsync(logPath, "");

        string[] lines = await ReadDataLinesAsync(filePath);

        var dataPoints = new List<WeatherDataPoint>();
        int rejected = 0;

        foreach (string line in lines)
        {
            if (TryParseLine(line, out WeatherDataPoint dataPoint))
            {
                dataPoints.Add(dataPoint);
            }
            else
            {
                await File.AppendAllTextAsync(
                    logPath, line + Environment.NewLine);

                rejected++;
            }
        }

        return (dataPoints, rejected);
    }

    public async Task<string[]> ReadDataLinesAsync(string filePath)
    {
        string[] lines = await File.ReadAllLinesAsync(filePath);

        return lines.Skip(1).ToArray();
    }

    private static bool TryParseLine(
        string line,
        out WeatherDataPoint dataPoint)
    {
        dataPoint = null!;

        string[] fields = line.Split('\t');

        if (fields.Length != 4)
        {
            return false;
        }

        string stationName = fields[0].Trim();

        if (string.IsNullOrWhiteSpace(stationName) ||
            stationName.Length > 100)
        {
            return false;
        }

        if (!DateTime.TryParseExact(
            fields[1].Trim(),
            "yyyy-MM-dd HH:mm:ss",
            CultureInfo.InvariantCulture,
            DateTimeStyles.None,
            out DateTime timestamp))
        {
            return false;
        }

        string temperatureText = fields[2].Trim().Replace(',', '.');
        string humidityText = fields[3].Trim().Replace(',', '.');

        if (!decimal.TryParse(
            temperatureText,
            NumberStyles.Number,
            CultureInfo.InvariantCulture,
            out decimal temperature))
        {
            return false;
        }

        if (!decimal.TryParse(
            humidityText,
            NumberStyles.Number,
            CultureInfo.InvariantCulture,
            out decimal humidity))
        {
            return false;
        }

        if (temperature < -100 || temperature > 100 ||
            humidity < 0 || humidity > 100)
        {
            return false;
        }

        dataPoint = new WeatherDataPoint
        {
            StationName = stationName,
            Timestamp = timestamp,
            Temperature = temperature,
            Humidity = humidity,
            DataSource = "CSV-File"
        };

        return true;
    }
}