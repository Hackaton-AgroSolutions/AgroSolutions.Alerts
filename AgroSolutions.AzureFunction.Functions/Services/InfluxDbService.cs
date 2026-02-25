using AgroSolutions.AzureFunction.Functions.Interfaces;
using InfluxDB.Client;
using InfluxDB.Client.Writes;
using Microsoft.Extensions.Configuration;

namespace AgroSolutions.AzureFunction.Functions.Services;

public class InfluxDbService(IConfiguration configuration) : IInfluxDbService
{
    private InfluxDBClient GetClient() => new(new InfluxDBClientOptions(configuration["InfluxDB:Url"])
    {
        Bucket = configuration["InfluxDB:Bucket"],
        Org = configuration["InfluxDB:Org"],
        Username = configuration["InfluxDB:Username"],
        Password = configuration["InfluxDB:Password"],
        Token = configuration["InfluxDB:Token"]
    });

    public async Task WritePointDataAsync(PointData pointData)
    {
        using InfluxDBClient client = GetClient();
        WriteApiAsync writeApiAsync = client.GetWriteApiAsync();
        await writeApiAsync.WritePointAsync(pointData);//, bucket, org);
    }
}
