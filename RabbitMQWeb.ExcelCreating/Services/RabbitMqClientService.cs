using RabbitMQ.Client;

namespace RabbitMQWeb.ExcelCreating.Services;

public class RabbitMqClientService
{
    private readonly ConnectionFactory _connectionFactory;
    private IConnection _connection;
    private IModel _channel;
    public static string ExchangeName = "ExcelDirectExchange";
    public static string RoutingExcel = "excel-route-file";
    public static string QueueName = "queue-excel-file";
    private readonly ILogger<RabbitMqClientService> _logger;

    public RabbitMqClientService(ConnectionFactory connectionFactory, ILogger<RabbitMqClientService> logger)
    {
        _connectionFactory = connectionFactory;
        _logger = logger;
    }

    public IModel Connect()
    {
        _connectionFactory.DispatchConsumersAsync = true;
        _connection = _connectionFactory.CreateConnection();
        if (_channel is {IsOpen: true})
            return _channel;

        _channel = _connection.CreateModel();
        _channel.ExchangeDeclare(ExchangeName, type: "direct", true, false);
        _channel.QueueDeclare(QueueName, true, false, false, null);
        _channel.QueueBind(exchange: ExchangeName, queue: QueueName, routingKey: RoutingExcel);
        _logger.LogInformation("RabbitMQ ile bağlantı kuruldu.");
        return _channel;
    }

    public void Dispose()
    {
        _channel?.Close();
        _channel?.Dispose();
        _connection?.Close();
        _connection?.Dispose();
        _logger.LogInformation("RabbitMQ ile bağlantı koptu.");
    }
}