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

    string filePath = Path.Combine(
      AppContext.BaseDirectory, "weather_data.csv");

    var importer = new CsvImportService();
    var result = await importer.ImportAsync(filePath);

    Console.WriteLine(
        $"{result.Imported} rader importerade, " +
        $"{result.Rejected} rader förkastade på grund av fel.");

    Console.WriteLine(
        $"{result.Skipped} mätningar överhoppade som dubbletter.");

    string logPath = Path.Combine(
        AppContext.BaseDirectory, "bad_data.log");

    string[] logLines = await File.ReadAllLinesAsync(logPath);

    Console.WriteLine($"Antal rader i felloggen: {logLines.Length}");
    Console.WriteLine($"Fellogg: {logPath}");
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