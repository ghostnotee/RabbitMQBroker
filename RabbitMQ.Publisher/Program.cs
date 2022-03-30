// See https://aka.ms/new-console-template for more information

using System.Text;
using RabbitMQ.Client;

var factory = new ConnectionFactory();
factory.Uri = new Uri("amqp://guest:guest@localhost:5672");
using var connection = factory.CreateConnection();
var channel = connection.CreateModel();
channel.QueueDeclare("work-queue", true, false, false);

Enumerable.Range(1, 50).ToList().ForEach(x =>
{
    string message = $"mesajın kendisi {x}";
    var messageBody = Encoding.UTF8.GetBytes(message);

    channel.BasicPublish(String.Empty, "work-queue", null, messageBody);

    Console.WriteLine($"Mesaj gönderildi: {message}");
});

Console.ReadLine();