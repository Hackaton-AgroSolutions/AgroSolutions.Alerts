using AgroSolutions.Alert.Infrastructure.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
using Serilog;

namespace AgroSolutions.Alert.Functions.Functions;

public class TesteInflux(IInfluxDbService influxDb)
{
    private readonly IInfluxDbService _influxDb = influxDb;

    [Function("ProcessSensorData")]
    public async Task Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "teste")] HttpRequest req)
    {
        Log.Information("Testing query.");
        try
        {
            var res = await _influxDb.QueryAsync($@"from(bucket: ""agrosolutions-bucket"")
  |> range(start: v.timeRangeStart, stop: v.timeRangeStop)
  |> filter(fn: (r) => r[""_field""] == ""humidity"")
  |> yield(name: ""mean"")");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error during testing influx");
        }
    }
}
