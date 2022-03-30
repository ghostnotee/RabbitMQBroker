// See https://aka.ms/new-console-template for more information

using System.Text;
using RabbitMQ.Client;

var factory = new ConnectionFactory();
factory.Uri = new Uri("amqp://guest:guest@localhost:5672");
using var connection = factory.CreateConnection();
var channel = connection.CreateModel();
channel.QueueDeclare("hello-queue", true, false, false);

string message = "Hello-RabbitMQ";
var messageBody = Encoding.UTF8.GetBytes(message);

channel.BasicPublish(String.Empty,"hello-queue",null,messageBody);

Console.WriteLine("Mesaj gönderildi");
Console.ReadLine();