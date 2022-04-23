// See https://aka.ms/new-console-template for more information

using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Shared;

var factory = new ConnectionFactory();
factory.Uri = new Uri("amqp://guest:guest@localhost:5672");
using var connection = factory.CreateConnection();
var channel = connection.CreateModel();

channel.ExchangeDeclare("headers-exchange", durable: true, type: ExchangeType.Headers);
channel.BasicQos(0, 1, false);
var consumer = new EventingBasicConsumer(channel);

var queueName = channel.QueueDeclare().QueueName;

Dictionary<string, object> headers = new Dictionary<string, object>
{
    {"format", "pdf"},
    {"shape", "a4"},
    {"x-match", "all"}
};

channel.QueueBind(queueName, "headers-exchange", String.Empty, headers);

channel.BasicConsume(queueName, false, consumer);

Console.WriteLine("Loglar dinleniyor...");

consumer.Received += (_, e) =>
{
    var message = Encoding.UTF8.GetString(e.Body.ToArray());
    Product? product = JsonSerializer.Deserialize<Product>(message);
    Thread.Sleep(500);
    if (product != null)
        Console.WriteLine($"Gelen mesaj: {product.Id} {product.Name} {product.Price} {product.Stock} ");
    channel.BasicAck(e.DeliveryTag, false);
};

Console.ReadLine();