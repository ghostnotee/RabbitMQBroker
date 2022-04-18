using System.Text;
using System.Text.Json;
using RabbitMQ.Client;

namespace RabbitMQWeb.WaterMark.Services;

public class RabbitMQPublisher
{
    private readonly RabbitMQClientService _rabbitMqClientService;

    public RabbitMQPublisher(RabbitMQClientService rabbitMqClientService)
    {
        _rabbitMqClientService = rabbitMqClientService;
    }

    public void Publish(productImageCreatedEvent productImageCreatedEvent)
    {
        var channel = _rabbitMqClientService.Connect();
        var messageBodyString = JsonSerializer.Serialize(productImageCreatedEvent);
        var messageBodyByte = Encoding.UTF8.GetBytes(messageBodyString);
        var properties = channel.CreateBasicProperties();
        properties.Persistent = true;
        channel.BasicPublish(exchange: RabbitMQClientService.ExchangeName,
            routingKey: RabbitMQClientService.RoutingWatermark, basicProperties: properties,
            body: messageBodyByte);
    }
}