using System.Text.Json;
using FailSafeWeatherMonitor.Data;
using FailSafeWeatherMonitor.Services;
using FailSafeWeatherMonitor.UI;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Spectre.Console;

try
{
    await using var db = new WeatherDbContext();
    await db.Database.MigrateAsync();
}
catch (SqlException ex)
{
    Console.WriteLine($"Kunde inte förbereda databasen: {ex.Message}");
    return;
}

var menu = new MainMenu();
var importer = new CsvImportService();
var weatherService = new OpenMeteoService();

bool running = true;

while (running)
{
    Console.Clear();

    string choice = menu.Show();
    Console.WriteLine();

    try
    {
        switch (choice)
        {
            case "Importera historisk data från fil":
                {
                    string filePath = Path.Combine(
                        AppContext.BaseDirectory, "weather_data.csv");

                    var result = await importer.ImportAsync(filePath);

                    Console.WriteLine(
                        $"{result.Imported} rader importerade, " +
                        $"{result.Rejected} rader förkastade på grund av fel.");

                    Console.WriteLine(
                        $"{result.Skipped} mätningar överhoppade som dubbletter.");

                    string logPath = Path.Combine(
                        AppContext.BaseDirectory, "bad_data.log");

                    Console.WriteLine($"Fellogg: {logPath}");

                    break;
                }

            case "Hämta live-data från API":
                {
                    var result = await weatherService.FetchAndSaveAsync();
                    var measurement = result.Measurement;

                    var table = new Table()
                        .Title("[bold cyan]Väder från Open-Meteo[/]")
                        .Border(TableBorder.Rounded)
                        .BorderColor(Color.Cyan);

                    table.AddColumn("Information");
                    table.AddColumn("Värde");

                    table.AddRow(
                        "Station", measurement.StationName.EscapeMarkup());

                    table.AddRow(
                        "Tid (UTC)", $"{measurement.Timestamp:yyyy-MM-dd HH:mm}");

                    table.AddRow(
                        "Temperatur", $"{measurement.Temperature:F2} °C");

                    table.AddRow(
                        "Luftfuktighet", $"{measurement.Humidity:F2} %");

                    table.AddRow(
                        "Källa", measurement.DataSource.EscapeMarkup());

                    AnsiConsole.Write(table);

                    Console.WriteLine(result.Saved
                        ? "Mätningen sparades i databasen."
                        : "Mätningen finns redan och sparades inte igen.");

                    break;
                }
        
            case "Visa väderstatistik":
                {
                    var statisticsView = new WeatherStatisticsView();
                    await statisticsView.ShowAsync();
                    break;
                }
            case "Avsluta":
                running = false;
                break;
        }
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
        Console.WriteLine(
            $"Kunde inte tolka API-svarets JSON: {ex.Message}");
    }
    catch (InvalidDataException ex)
    {
        Console.WriteLine($"Fel i API-svarets väderdata: {ex.Message}");
    }
    catch (IOException ex)
    {
        Console.WriteLine($"Filfel: {ex.Message}");
    }
    catch (UnauthorizedAccessException ex)
    {
        Console.WriteLine($"Åtkomst till filen nekades: {ex.Message}");
    }
    catch (DbUpdateException ex)
    {
        Console.WriteLine($"Kunde inte spara mätningarna: {ex.Message}");
    }
    catch (SqlException ex)
    {
        Console.WriteLine($"Databasfel: {ex.Message}");
    }

    if (running)
    {
        Console.WriteLine(
            "\nTryck på valfri tangent för att återgå till menyn.");

        Console.ReadKey(true);
    }
}