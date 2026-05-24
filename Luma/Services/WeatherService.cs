using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Luma.Models;

namespace Luma.Services;

public class WeatherService(HttpClient http)
{
    public async Task<WeatherInfo?> GetCurrentWeatherAsync(double lat, double lng)
    {
        var url = "https://api.open-meteo.com/v1/forecast" +
            $"?latitude={lat.ToString(CultureInfo.InvariantCulture)}" +
            $"&longitude={lng.ToString(CultureInfo.InvariantCulture)}" +
            "&current=cloud_cover,precipitation,weather_code,wind_speed_10m,visibility,temperature_2m" +
            "&hourly=precipitation_probability,precipitation,cloud_cover" +
            "&daily=precipitation_probability_max" +
            "&timezone=auto" +
            "&forecast_days=2";

        try
        {
            var response = await http.GetFromJsonAsync<OpenMeteoResponse>(url);
            var current = response?.Current;
            if (current == null) return null;
            int? tomorrowPrecipitationProbability = response?.Daily?.PrecipitationProbabilityMax is { Count: > 1 } probabilities
                ? probabilities[1]
                : null;

            return new WeatherInfo
            {
                CloudCover    = current.CloudCover,
                Precipitation = current.Precipitation,
                WeatherCode   = current.WeatherCode,
                Description   = GetDescriptionKey(current.WeatherCode),
                Icon          = GetIcon(current.WeatherCode),
                IsGoodForPhoto = current.CloudCover < 50 && current.Precipitation == 0,
                WindSpeed     = current.WindSpeed,
                Visibility    = current.Visibility,
                Temperature   = current.Temperature,
                TomorrowPrecipitationProbability = tomorrowPrecipitationProbability,
                HourlyForecast = GetUpcomingHourlyForecast(response?.Hourly)
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"WeatherService error: {ex.Message}");
            return null;
        }
    }

    private static string GetDescriptionKey(int code) => code switch
    {
        0          => "Weather_Clear",
        1          => "Weather_MostlyClear",
        2          => "Weather_PartlyCloudy",
        3          => "Weather_Overcast",
        45 or 48   => "Weather_Foggy",
        51 or 53 or 55 => "Weather_Drizzle",
        61 or 63 or 65 => "Weather_Rain",
        71 or 73 or 75 => "Weather_Snow",
        80 or 81 or 82 => "Weather_Showers",
        95         => "Weather_Thunderstorm",
        _          => "Weather_Unknown"
    };

    private static string GetIcon(int code) => code switch
    {
        0          => "☀️",
        1          => "🌤️",
        2          => "⛅",
        3          => "☁️",
        45 or 48   => "🌫️",
        51 or 53 or 55 or 61 or 63 or 65 or 80 or 81 or 82 => "🌧️",
        71 or 73 or 75 => "❄️",
        95         => "⛈️",
        _          => "🌡️"
    };

    private static IReadOnlyList<HourlyWeatherForecast> GetUpcomingHourlyForecast(OpenMeteoHourly? hourly)
    {
        if (hourly?.Time == null || hourly.PrecipitationProbability == null || hourly.Precipitation == null || hourly.CloudCover == null)
            return [];

        var count = new[]
        {
            hourly.Time.Count,
            hourly.PrecipitationProbability.Count,
            hourly.Precipitation.Count,
            hourly.CloudCover.Count
        }.Min();

        if (count == 0)
            return [];

        var now = DateTime.Now.AddMinutes(-30);
        var horizon = DateTime.Now.AddHours(6);
        var forecast = new List<HourlyWeatherForecast>();

        for (var index = 0; index < count; index++)
        {
            if (!DateTime.TryParse(hourly.Time[index], CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out var time))
                continue;

            if (time < now || time > horizon)
                continue;

            forecast.Add(new HourlyWeatherForecast
            {
                Time = time,
                PrecipitationProbability = hourly.PrecipitationProbability[index],
                Precipitation = hourly.Precipitation[index],
                CloudCover = hourly.CloudCover[index]
            });
        }

        return forecast;
    }
}

class OpenMeteoResponse
{
    [JsonPropertyName("current")]
    public OpenMeteoCurrent? Current { get; set; }

    [JsonPropertyName("daily")]
    public OpenMeteoDaily? Daily { get; set; }

    [JsonPropertyName("hourly")]
    public OpenMeteoHourly? Hourly { get; set; }
}

class OpenMeteoCurrent
{
    [JsonPropertyName("cloud_cover")]
    public int CloudCover { get; set; }

    [JsonPropertyName("precipitation")]
    public double Precipitation { get; set; }

    [JsonPropertyName("weather_code")]
    public int WeatherCode { get; set; }

    [JsonPropertyName("wind_speed_10m")]
    public double WindSpeed { get; set; }

    [JsonPropertyName("visibility")]
    public double Visibility { get; set; }

    [JsonPropertyName("temperature_2m")]
    public double Temperature { get; set; }
}

class OpenMeteoDaily
{
    [JsonPropertyName("precipitation_probability_max")]
    public List<int>? PrecipitationProbabilityMax { get; set; }
}

class OpenMeteoHourly
{
    [JsonPropertyName("time")]
    public List<string>? Time { get; set; }

    [JsonPropertyName("precipitation_probability")]
    public List<int>? PrecipitationProbability { get; set; }

    [JsonPropertyName("precipitation")]
    public List<double>? Precipitation { get; set; }

    [JsonPropertyName("cloud_cover")]
    public List<int>? CloudCover { get; set; }
}
