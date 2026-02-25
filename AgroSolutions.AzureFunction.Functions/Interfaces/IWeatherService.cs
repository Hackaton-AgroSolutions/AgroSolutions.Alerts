namespace AgroSolutions.AzureFunction.Functions.Interfaces;

public interface IWeatherService
{
    Task<(WeatherData, ICollection<WeatherData>)> GetCurrentAndFutureWeatherDataAsync();

    public record WeatherData(DateTime Time, double Temperature, double Humidity, double Precipitation, double RainProbability, double WindSpeed, string WeatherDescription);
}
