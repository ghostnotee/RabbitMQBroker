// See https://aka.ms/new-console-template for more information

using System.Text;
using RabbitMQ.Client;

var factory = new ConnectionFactory();
factory.Uri = new Uri("amqp://guest:guest@localhost:5672");
using var connection = factory.CreateConnection();
var channel = connection.CreateModel();

channel.ExchangeDeclare("logs-fanout", durable: true, type: ExchangeType.Fanout);


Enumerable.Range(1, 50).ToList().ForEach(x =>
{
    string log = $"log {x}";
    var messageBody = Encoding.UTF8.GetBytes(log);

    channel.BasicPublish("logs-fanout", "", null, messageBody);

    Console.WriteLine($"Mesaj gönderildi: {log}");
});

Console.ReadLine();