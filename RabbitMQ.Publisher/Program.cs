// See https://aka.ms/new-console-template for more information

using System.Text;
using RabbitMQ.Client;

var factory = new ConnectionFactory();
factory.Uri = new Uri("amqp://guest:guest@localhost:5672");
using var connection = factory.CreateConnection();
var channel = connection.CreateModel();

channel.ExchangeDeclare("logs-direct", durable: true, type: ExchangeType.Direct);

Enum.GetNames(typeof(LogNames)).ToList().ForEach(x =>
{
    var routeKey = $"route-{x}";
    var queueName = $"direct-queue-{x}";
    channel.QueueDeclare(queueName, true, false, false, null);
    channel.QueueBind(queueName, "logs-direct", routeKey, null);
});

Enumerable.Range(1, 50).ToList().ForEach(x =>
{
    LogNames log = (LogNames) new Random().Next(1, 5);

    string logMessage = $"log-type: {log}";
    var messageBody = Encoding.UTF8.GetBytes(logMessage);
    var routeKey = $"route-{log}";

    channel.BasicPublish("logs-direct", routeKey, null, messageBody);

    Console.WriteLine($"Log gönderildi: {logMessage}");
});

Console.ReadLine();


public enum LogNames
{
    Critical = 1,
    Error = 2,
    Warning = 3,
    Info = 4
}