using System.ComponentModel.DataAnnotations;

namespace FailSafeWeatherMonitor.Models;

public class WeatherDataPoint
{
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string StationName { get; set; } = null!;

    public DateTime Timestamp { get; set; }

    public decimal Temperature { get; set; }

    public decimal Humidity { get; set; }

    [Required]
    [MaxLength(20)]
    public string DataSource { get; set; } = null!;
}