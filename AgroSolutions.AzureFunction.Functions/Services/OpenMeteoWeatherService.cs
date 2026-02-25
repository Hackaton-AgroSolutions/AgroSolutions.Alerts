using AgroSolutions.AzureFunction.Functions.Interfaces;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using static AgroSolutions.AzureFunction.Functions.Interfaces.IWeatherService;

namespace AgroSolutions.AzureFunction.Functions.Services;

public class OpenMeteoWeatherService(IConfiguration configuration) : IWeatherService
{
    private readonly string _urlApiWeather = configuration["WeatherService:Url"]!.ToString();

    public async Task<(WeatherData, ICollection<WeatherData>)> GetCurrentAndFutureWeatherDataAsync()
    {
        WeatherData currentWeatherData;
        ICollection<WeatherData> futureWeatherDatas = [];

        string finalUrl = _urlApiWeather + "/v1/forecast" +
            "?latitude=-23.5475&longitude=-46.6361" + // São Paulo coordinates
            "&current=temperature_2m,relative_humidity_2m,precipitation,precipitation_probability,wind_speed_10m,weathercode" +
            "&hourly=temperature_2m,relative_humidity_2m,precipitation,precipitation_probability,wind_speed_10m,weathercode" +
            "&forecast_hours=168"; // 7 DAYS

        using HttpClient client = new();
        string response = await client.GetStringAsync(finalUrl);
        JObject json = JObject.Parse(response);

        currentWeatherData = new(
            Time: json["current"]!["time"]!.Value<DateTime>(),
            Temperature: json["current"]!["temperature_2m"]!.Value<double>(),
            Humidity: json["current"]!["relative_humidity_2m"]!.Value<double>(),
            Precipitation: json["current"]!["precipitation"]!.Value<double>(),
            RainProbability: json["current"]!["precipitation_probability"]!.Value<double>(),
            WindSpeed: json["current"]!["wind_speed_10m"]!.Value<double>(),
            WeatherDescription: GetWeatherDescription(json["current"]!["weathercode"]!.Value<int>()));

        JToken times = json["hourly"]!["time"]!;
        JToken temps = json["hourly"]!["temperature_2m"]!;
        JToken humidity = json["hourly"]!["relative_humidity_2m"]!;
        JToken precipitation = json["hourly"]!["precipitation"]!;
        JToken rainProb = json["hourly"]!["precipitation_probability"]!;
        JToken wind = json["hourly"]!["wind_speed_10m"]!;
        JToken weathercode = json["hourly"]!["weathercode"]!;

        for (int i = 0; i < times!.Count(); i++)
        {
            futureWeatherDatas.Add(
                new(
                Time: json["current"]!["time"]!.Value<DateTime>(),
                Temperature: temps![i]!.Value<double>(),
                Humidity: humidity![i]!.Value<double>(),
                Precipitation: precipitation![i]!.Value<double>(),
                RainProbability: rainProb![i]!.Value<int>(),
                WindSpeed: wind![i]!.Value<double>(),
                WeatherDescription: GetWeatherDescription(weathercode![i]!.Value<int>())));
        }

        return (currentWeatherData, futureWeatherDatas);
    }

    private static string GetWeatherDescription(int code) => code switch
    {
        0 => "Sunny",
        1 => "Mostly Clear",
        2 => "Partly Cloudy",
        3 => "Cloudy",
        45 or 48 => "Fog",
        51 or 53 or 55 => "Drizzle",
        61 or 63 or 65 => "Rain",
        80 or 81 or 82 => "Rain Showers",
        95 or 99 => "Thunderstorm",
        _ => "Unknown"
    };
}
