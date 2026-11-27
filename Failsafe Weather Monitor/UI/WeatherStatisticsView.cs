using FailSafeWeatherMonitor.Services;
using Spectre.Console;

namespace FailSafeWeatherMonitor.UI;

public class WeatherStatisticsView
{
    public async Task ShowAsync()
    {
        var service = new WeatherStatisticsService();
        var stations = await service.GetStationsAsync();

        if (stations.Count == 0)
        {
            AnsiConsole.MarkupLine(
                "[yellow]Det finns inga sparade mätningar ännu.[/]");
            return;
        }

        string stationName = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("[cyan]Välj en mätstation:[/]")
                .UseConverter(station => station.EscapeMarkup())
                .AddChoices(stations));

        var measurements = await service.GetMeasurementsAsync(stationName);

        if (measurements.Count == 0)
        {
            AnsiConsole.MarkupLine(
                "[yellow]Stationen saknar mätningar.[/]");
            return;
        }

        var measurementsTable = new Table()
            .Title($"[bold cyan]{stationName.EscapeMarkup()}[/]")
            .Border(TableBorder.Rounded)
            .BorderColor(Color.Cyan);

        measurementsTable.AddColumn("Tidpunkt");
        measurementsTable.AddColumn("Temperatur (°C)");
        measurementsTable.AddColumn("Luftfuktighet (%)");
        measurementsTable.AddColumn("Källa");

        foreach (var measurement in measurements)
        {
            string timeText = measurement.Timestamp.ToString(
                "yyyy-MM-dd HH:mm:ss");

            if (measurement.DataSource == "API")
            {
                timeText += " UTC";
            }

            measurementsTable.AddRow(
                timeText,
                measurement.Temperature.ToString("F2"),
                measurement.Humidity.ToString("F2"),
                measurement.DataSource.EscapeMarkup());
        }

        AnsiConsole.Write(measurementsTable);
        Console.WriteLine();

        decimal minimum = measurements.Min(point => point.Temperature);
        decimal maximum = measurements.Max(point => point.Temperature);
        decimal average = measurements.Average(point => point.Temperature);

        var statisticsTable = new Table()
            .Title("[bold green]Temperaturstatistik[/]")
            .Border(TableBorder.Rounded)
            .BorderColor(Color.Green);

        statisticsTable.AddColumn("Statistik");
        statisticsTable.AddColumn("Temperatur (°C)");

        statisticsTable.AddRow("[blue]Lägsta[/]", minimum.ToString("F2"));
        statisticsTable.AddRow("[red]Högsta[/]", maximum.ToString("F2"));
        statisticsTable.AddRow("[green]Medel[/]", average.ToString("F2"));

        AnsiConsole.Write(statisticsTable);
    }
}