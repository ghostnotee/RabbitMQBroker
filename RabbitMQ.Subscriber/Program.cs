// See https://aka.ms/new-console-template for more information

using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

var factory = new ConnectionFactory();
factory.Uri = new Uri("amqp://guest:guest@localhost:5672");
using var connection = factory.CreateConnection();
var channel = connection.CreateModel();

channel.ExchangeDeclare("logs-topic", durable: true, type: ExchangeType.Topic);
channel.BasicQos(0, 1, false);
var consumer = new EventingBasicConsumer(channel);

var queueName = channel.QueueDeclare().QueueName;
//var routeKey = "Critical.#";
var routekey = "*.Error.*";
channel.QueueBind(queueName, "logs-topic",routekey);

channel.BasicConsume(queueName, false, consumer);

Console.WriteLine("Loglar dinleniyor...");

consumer.Received += (_, e) =>
{
    var message = Encoding.UTF8.GetString(e.Body.ToArray());
    Thread.Sleep(500);
    Console.WriteLine("Gelen mesaj: " + message);
    //File.AppendAllText("log-critical.txt", message + "\n");
    channel.BasicAck(e.DeliveryTag, false);
};

Console.ReadLine();