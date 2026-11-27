using Spectre.Console;

namespace FailSafeWeatherMonitor.UI;

public class MainMenu
{
    public string Show()
    {
        return AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("[bold cyan]FailSafe WeatherMonitor[/]")
                .AddChoices(
                    "Importera historisk data från fil",
                    "Hämta live-data från API",
                    "Visa väderstatistik",
                    "Avsluta"));
    }
}