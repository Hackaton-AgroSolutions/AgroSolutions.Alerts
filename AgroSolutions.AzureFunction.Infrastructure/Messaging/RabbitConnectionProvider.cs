using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using Serilog;
using System.Text.Json;

namespace AgroSolutions.AzureFunction.Infrastructure.Messaging;

public class RabbitConnectionProvider : IRabbitConnectionProvider
{
    private readonly ConnectionFactory _factory;
    private IConnection? _connection;
    private readonly SemaphoreSlim _lock = new(1, 1);

    public RabbitConnectionProvider(IOptions<RabbitMqOptions> options)
    {
        Log.Information(JsonSerializer.Serialize(options.Value));
        _factory = new ConnectionFactory
        {
            HostName = options.Value.Host,
            Port = options.Value.Port,
            UserName = options.Value.Username,
            Password = options.Value.Password,
            AutomaticRecoveryEnabled = true
        };
    }

    public async Task<IConnection> GetConnectionAsync(CancellationToken cancellationToken)
    {
        if (_connection is not null && _connection.IsOpen)
            return _connection;

        await _lock.WaitAsync(cancellationToken);
        try
        {
            if (_connection is null || !_connection.IsOpen)
                _connection = await _factory.CreateConnectionAsync(cancellationToken);

            return _connection;
        }
        finally
        {
            _lock.Release();
        }
    }
}
