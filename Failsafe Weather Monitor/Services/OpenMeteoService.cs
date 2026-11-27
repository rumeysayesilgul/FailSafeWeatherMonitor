using System.Globalization;
using System.Text.Json;
using FailSafeWeatherMonitor.Models;
using FailSafeWeatherMonitor.Data;
using Microsoft.EntityFrameworkCore;


namespace FailSafeWeatherMonitor.Services;

public class OpenMeteoService
{
    private static readonly HttpClient client = new();

    private const string ApiUrl =
        "https://api.open-meteo.com/v1/forecast" +
        "?latitude=57.6832&longitude=11.9511" +
        "&hourly=temperature_2m,relative_humidity_2m";
    public async Task<(WeatherDataPoint Measurement, bool Saved)>
    FetchAndSaveAsync()
    {
        WeatherDataPoint measurement = await GetCurrentMeasurementAsync();

        await using var db = new WeatherDbContext();

        bool exists = await db.WeatherDataPoints.AnyAsync(point =>
            point.StationName == measurement.StationName &&
            point.Timestamp == measurement.Timestamp);

        if (exists)
        {
            return (measurement, false);
        }

        await db.WeatherDataPoints.AddAsync(measurement);
        await db.SaveChangesAsync();

        return (measurement, true);
    }
    public async Task<string> GetWeatherJsonAsync()
    {
        return await client.GetStringAsync(ApiUrl);
    }

    public async Task<WeatherDataPoint> GetCurrentMeasurementAsync()
    {
        string json = await GetWeatherJsonAsync();

        var response = JsonSerializer.Deserialize<OpenMeteoResponse>(json)
            ?? throw new InvalidDataException("API-svaret är tomt.");

        var hourly = response.Hourly;

        if (hourly is null ||
            hourly.Time is null ||
            hourly.Temperatures is null ||
            hourly.Humidities is null)
        {
            throw new InvalidDataException(
                "API-svaret saknar väderdata.");
        }

        if (hourly.Time.Count != hourly.Temperatures.Count ||
            hourly.Time.Count != hourly.Humidities.Count)
        {
            throw new InvalidDataException(
                "API-svarets listor har olika längd.");
        }

        if (response.UtcOffsetSeconds != 0)
        {
            throw new InvalidDataException(
                "API-svaret saknar den förväntade UTC-tidszonen.");
        }

        DateTime now = DateTime.UtcNow;

        DateTime currentHour = new(
            now.Year,
            now.Month,
            now.Day,
            now.Hour,
            0,
            0,
            DateTimeKind.Utc);

        string currentHourText = currentHour.ToString(
            "yyyy-MM-dd'T'HH:mm",
            CultureInfo.InvariantCulture);

        int index = hourly.Time.IndexOf(currentHourText);

        if (index == -1)
        {
            throw new InvalidDataException(
                "Aktuell timme saknas i API-svaret.");
        }

        decimal? temperature = hourly.Temperatures[index];
        decimal? humidity = hourly.Humidities[index];

        if (temperature is null || humidity is null)
        {
            throw new InvalidDataException(
                "Aktuell timme saknar temperatur eller luftfuktighet.");
        }

        if (temperature.Value < -100 || temperature.Value > 100 ||
            humidity.Value < 0 || humidity.Value > 100)
        {
            throw new InvalidDataException(
                "API-svaret innehåller orimliga mätvärden.");
        }

        return new WeatherDataPoint
        {
            StationName = "Göteborg",
            Timestamp = currentHour,
            Temperature = temperature.Value,
            Humidity = humidity.Value,
            DataSource = "API"
        };
    }
}