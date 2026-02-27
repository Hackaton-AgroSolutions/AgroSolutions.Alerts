using AgroSolutions.Alert.Infrastructure.Interfaces;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Writes;
using Microsoft.Azure.Functions.Worker;
using Serilog;
using static AgroSolutions.Alert.Infrastructure.Interfaces.IWeatherService;

namespace AgroSolutions.Alert.Functions.Functions;

public class WeatherCollectorFunction(IInfluxDbService influxDb, IWeatherService weatherService)
{
    private readonly IInfluxDbService _influxDb = influxDb;
    private readonly IWeatherService _weatherService = weatherService;

    [Function("WeatherCollector")]
    public async Task Run([TimerTrigger("* */1 * * * *", RunOnStartup = true)] TimerInfo myTimer)
    {
        Log.Information("Fetching Weather data from API.");
        (WeatherData currentWeather, IEnumerable<WeatherData> futureWeathers) = await _weatherService.GetCurrentAndFutureWeatherDataAsync();

        Log.Information("Sending current data weather with Time {Time} and Temperature {Temperature} to InfluxDb.", currentWeather.Time, currentWeather.Temperature);
        PointData currentPoint = PointData
            .Measurement("weather_current")
            .Tag("city", "sao_paulo")
            .Field("temperature", currentWeather.Temperature)
            .Field("humidity", currentWeather.Humidity)
            .Field("precipitation", currentWeather.Precipitation)
            .Field("rain_probability", currentWeather.RainProbability)
            .Field("wind_speed", currentWeather.WindSpeed)
            .Field("weather_desc", currentWeather.WeatherDescription)
            .Timestamp(currentWeather.Time, WritePrecision.Ns);
        await _influxDb.WritePointDataAsync(currentPoint);

        Log.Information("Sending {Total} future datas weather to InfluxDb.", futureWeathers.Count());
        foreach (WeatherData futureWeather in futureWeathers)
        {
            PointData pointData = PointData
                .Measurement("weather_forecast")
                .Tag("city", "sao_paulo")
                .Field("temperature", futureWeather.Temperature)
                .Field("humidity", futureWeather.Humidity)
                .Field("precipitation", futureWeather.Precipitation)
                .Field("rain_probability", futureWeather.RainProbability)
                .Field("wind_speed", futureWeather.WindSpeed)
                .Field("weather_desc", futureWeather.WeatherDescription)
                .Timestamp(futureWeather.Time, WritePrecision.Ns);
            Log.Information("Sending future data weather with Time {Time} and Temperature {Temperature} to InfluxDb.", futureWeather.Time, futureWeather.Temperature);
            await _influxDb.WritePointDataAsync(pointData);
        }
    }
}
