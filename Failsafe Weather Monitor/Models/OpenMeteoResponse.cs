using System.Text.Json.Serialization;

namespace FailSafeWeatherMonitor.Models;

public class OpenMeteoResponse
{
    [JsonPropertyName("hourly")]
    public HourlyWeatherData Hourly { get; set; } = null!;

    [JsonPropertyName("utc_offset_seconds")]
    public int? UtcOffsetSeconds { get; set; }
}

public class HourlyWeatherData
{
    [JsonPropertyName("time")]
    public List<string> Time { get; set; } = null!;

    [JsonPropertyName("temperature_2m")]
    public List<decimal?> Temperatures { get; set; } = null!;

    [JsonPropertyName("relative_humidity_2m")]
    public List<decimal?> Humidities { get; set; } = null!;
}