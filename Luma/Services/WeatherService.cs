using System;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Luma.Models;

namespace Luma.Services;

public class WeatherService
{
    private readonly HttpClient _http;

    public WeatherService(HttpClient http)
    {
        _http = http;
    }

    public async Task<WeatherInfo?> GetCurrentWeatherAsync(double lat, double lng)
    {
        var url = $"https://api.open-meteo.com/v1/forecast" +
           $"?latitude={lat}&longitude={lng}" +
           $"&current=cloud_cover,precipitation,weather_code,wind_speed_10m,visibility,temperature_2m" +
           $"&forecast_days=1";

        try
        {
            var response = await _http.GetFromJsonAsync<OpenMeteoResponse>(url);
            var current = response?.Current;
            if (current == null) return null;

            return new WeatherInfo
            {
                CloudCover = current.CloudCover,
                Precipitation = current.Precipitation,
                WeatherCode = current.WeatherCode,
                Description = GetDescription(current.WeatherCode),
                Icon = GetIcon(current.WeatherCode),
                IsGoodForPhoto = current.CloudCover < 50 && current.Precipitation == 0,
                WindSpeed = current.WindSpeed,
                Visibility = current.Visibility,
                Temperature = current.Temperature
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"WeatherService error: {ex.Message}");
            return null;
        }
    }

    private static string GetDescription(int code) => code switch
    {
        0 => "晴天",
        1 => "大致晴朗",
        2 => "局部多云",
        3 => "阴天",
        45 or 48 => "有雾",
        51 or 53 or 55 => "毛毛雨",
        61 or 63 or 65 => "下雨",
        71 or 73 or 75 => "下雪",
        80 or 81 or 82 => "阵雨",
        95 => "雷暴",
        _ => "未知"
    };

    private static string GetIcon(int code) => code switch
    {
        0 => "☀️",
        1 => "🌤️",
        2 => "⛅",
        3 => "☁️",
        45 or 48 => "🌫️",
        51 or 53 or 55 or 61 or 63 or 65 or 80 or 81 or 82 => "🌧️",
        71 or 73 or 75 => "❄️",
        95 => "⛈️",
        _ => "🌡️"
    };
}

// Open-Meteo 响应模型 - 私有内部类
file class OpenMeteoResponse
{
    [JsonPropertyName("current")]
    public OpenMeteoCurrent? Current { get; set; }
}

file class OpenMeteoCurrent
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