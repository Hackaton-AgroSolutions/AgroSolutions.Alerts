using AgroSolutions.AzureFunction.Domain.Events;
using AgroSolutions.AzureFunction.Functions.Interfaces;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Writes;
using Microsoft.Azure.Functions.Worker;
using Serilog;
using Serilog.Context;
using System.Text.Json;

namespace AgroSolutions.AzureFunction.Functions.Functions;

public class SaveDataToInfluxFunction(IInfluxDbService influxDb)
{
    private readonly IInfluxDbService _influxDb = influxDb;

    [Function("SaveSensorDataToInfluxDb")]
    public async Task Run(
        [RabbitMQTrigger(
            queueName: "received-sensor-data-to-influxdb",
            ConnectionStringSetting = "RabbitMQConnection"
        )] string message)
    {
        Log.Information("Processing message from Messaging.");
        try
        {
            ReceivedSensorDataEvent? receivedSensorDataEvent = JsonSerializer.Deserialize<ReceivedSensorDataEvent>(message);
            if (receivedSensorDataEvent is null)
            {
                Log.Error("Invalid ReceivedSensorDataEvent message: {Message}.", message);
                return;
            }

            Log.Information("Sendinng received data form sensor with ID {SensorClientId} and Field with ID {FieldId} to InfluxDb.", receivedSensorDataEvent.SensorClientId, receivedSensorDataEvent.FieldId);
            using (LogContext.PushProperty("CorrelationId", receivedSensorDataEvent.CorrelationId))
            {
                PointData pointData = PointData
                    .Measurement("agro_sensors")
                    .Tag("sensor_client_id", receivedSensorDataEvent.SensorClientId.ToString())
                    .Field("field_id", receivedSensorDataEvent.FieldId)
                    .Field("soil_moisture_percent", receivedSensorDataEvent.SoilMoisturePercent)
                    .Field("air_temperature_c", receivedSensorDataEvent.AirTemperatureC)
                    .Field("precipitation_mm", receivedSensorDataEvent.PrecipitationMm)
                    .Field("air_humidity_percent", receivedSensorDataEvent.AirHumidityPercent)
                    .Field("soil_ph", receivedSensorDataEvent.SoilPH)
                    .Field("wind_speed_kmh", receivedSensorDataEvent.WindSpeedKmh)
                    .Field("data_quality_score", receivedSensorDataEvent.DataQualityScore)
                    .Timestamp(receivedSensorDataEvent.Timestamp, WritePrecision.Ns);
                await _influxDb.WritePointDataAsync(pointData);
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error processing ReceivedSensorDataEvent with Message {Message}", message);
        }
    }
}
