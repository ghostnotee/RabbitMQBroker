using RabbitMQ.Client;

namespace RabbitMQWeb.WaterMark.Services;

public class RabbitMQClientService
{
    private readonly ConnectionFactory _connectionFactory;
    private IConnection _connection;
    private IModel _channel;
    public static string ExchangeName = "ImageDirectExchange";
    public static string RoutingWatermark = "watermark-route-image";
    public static string QueueName = "watermark-image";
    private readonly ILogger<RabbitMQClientService> _logger;

    public RabbitMQClientService(ConnectionFactory connectionFactory, ILogger<RabbitMQClientService> logger)
    {
        _connectionFactory = connectionFactory;
        _logger = logger;
    }

    public IModel Connect()
    {
        _connection = _connectionFactory.CreateConnection();
        if (_channel is {IsOpen: true}) return _channel;
        _channel = _connection.CreateModel();
        _channel.ExchangeDeclare(ExchangeName, ExchangeType.Direct, durable: true, autoDelete: false);
        _channel.QueueDeclare(QueueName, true, false, false, null);
        _channel.QueueBind(exchange: ExchangeName, queue: QueueName, routingKey: RoutingWatermark);
        _logger.LogInformation("RabbitMQ ile bağlantı sağlandı...");
        return _channel;
    }

    public void Dispose()
    {
        _channel?.Close();
        _channel?.Dispose();
        _channel?.Close();
        _connection?.Dispose();
        _logger.LogInformation("RabbitMQ ile bağlantı sonlandı...");
    }
}