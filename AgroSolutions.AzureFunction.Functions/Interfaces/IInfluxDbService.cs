using InfluxDB.Client.Writes;

namespace AgroSolutions.AzureFunction.Functions.Interfaces;

public interface IInfluxDbService
{
    Task WritePointDataAsync(PointData pointData);
}
