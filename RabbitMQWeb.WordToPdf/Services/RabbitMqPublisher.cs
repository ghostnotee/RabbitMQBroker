using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using RabbitMQWeb.WordToPdf.Models;

namespace RabbitMQWeb.WordToPdf.Services;

public class RabbitMqPublisher
{
    private readonly RabbitMqClientService _rabbitMqClientService;


    public RabbitMqPublisher(RabbitMqClientService rabbitMqClientService)
    {
        _rabbitMqClientService = rabbitMqClientService;
    }

    public void Publish(WordToPdfModel wordToPdfModel)
    {
        MessageWordToPdf messageWordToPdf = new();
        var channel = _rabbitMqClientService.Connect();

        using (MemoryStream ms = new MemoryStream())
        {
            wordToPdfModel.File.CopyTo(ms);
            messageWordToPdf.WordByte = ms.ToArray();
        }

        messageWordToPdf.Email = wordToPdfModel.Email;
        messageWordToPdf.FileName = Path.GetFileNameWithoutExtension(wordToPdfModel.File.Name);
        string messageString = JsonSerializer.Serialize(messageWordToPdf);
        byte[] messageByte = Encoding.UTF8.GetBytes(messageString);
        IBasicProperties properties = channel.CreateBasicProperties();
        properties.Persistent = true;
        channel.BasicPublish(exchange: RabbitMqClientService.ExchangeName,
            routingKey: RabbitMqClientService.RoutingKey, basicProperties: properties, body: messageByte);
    }
}