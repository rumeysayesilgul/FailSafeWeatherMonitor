using System.Text.Json;
using FailSafeWeatherMonitor.Data;
using FailSafeWeatherMonitor.Services;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

try
{
    await using (var db = new WeatherDbContext())
    {
        await db.Database.MigrateAsync();
    }

    Console.WriteLine("Databasen är redo.");

    var service = new OpenMeteoService();
    var result = await service.FetchAndSaveAsync();

    var measurement = result.Measurement;

    Console.WriteLine($"Station: {measurement.StationName}");
    Console.WriteLine(
        $"Tid: {measurement.Timestamp:yyyy-MM-dd HH:mm} UTC");
    Console.WriteLine(
        $"Temperatur: {measurement.Temperature:F1} °C");
    Console.WriteLine(
        $"Luftfuktighet: {measurement.Humidity:F1} %");

    Console.WriteLine(result.Saved
        ? "Mätningen sparades i databasen."
        : "Mätningen finns redan och sparades inte igen.");
}
catch (HttpRequestException ex)
{
    Console.WriteLine($"Kunde inte hämta väderdata: {ex.Message}");
}
catch (OperationCanceledException)
{
    Console.WriteLine("API-anropet tog för lång tid. Försök igen.");
}
catch (JsonException ex)
{
    Console.WriteLine($"Kunde inte tolka API-svarets JSON: {ex.Message}");
}
catch (InvalidDataException ex)
{
    Console.WriteLine($"Fel i API-svarets väderdata: {ex.Message}");
}
catch (IOException ex)
{
    Console.WriteLine($"Filfel: {ex.Message}");
}
catch (DbUpdateException ex)
{
    Console.WriteLine($"Kunde inte spara mätningen: {ex.Message}");
}
catch (SqlException ex)
{
    Console.WriteLine($"Databasfel: {ex.Message}");
}