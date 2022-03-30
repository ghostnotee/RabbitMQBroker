// See https://aka.ms/new-console-template for more information

using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

var factory = new ConnectionFactory();
factory.Uri = new Uri("amqp://guest:guest@localhost:5672");
using var connection = factory.CreateConnection();
var channel = connection.CreateModel();

var randomQueueName = channel.QueueDeclare().QueueName;
channel.QueueBind(randomQueueName,"logs-fanout","",null);

channel.BasicQos(0, 1, false);
var consumer = new EventingBasicConsumer(channel);
channel.BasicConsume(randomQueueName, false, consumer);

Console.WriteLine("Loglar dinleniyor...");

consumer.Received += (_, e) =>
{
    var message = Encoding.UTF8.GetString(e.Body.ToArray());
    Thread.Sleep(1500);
    Console.WriteLine("Gelen mesaj: " + message);
    channel.BasicAck(e.DeliveryTag, false);
};

Console.ReadLine();