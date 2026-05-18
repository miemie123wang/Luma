using System.Text.Json.Serialization;

namespace Luma.Models;

public class WeatherInfo
{
    public int CloudCover { get; set; }
    public double Precipitation { get; set; }
    public int WeatherCode { get; set; }
    public string Description { get; set; } = "";
    public string Icon { get; set; } = "";
    public bool IsGoodForPhoto { get; set; }
    public double WindSpeed { get; set; }      // km/h
    public double Visibility { get; set; }        // 米
    public double Temperature { get; set; }    // °C
}
