// See https://aka.ms/new-console-template for more information

using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using Shared;

var factory = new ConnectionFactory();
factory.Uri = new Uri("amqp://guest:guest@localhost:5672");
using var connection = factory.CreateConnection();
var channel = connection.CreateModel();

channel.ExchangeDeclare("headers-exchange", durable: true, type: ExchangeType.Headers);

Dictionary<string, object> headers = new Dictionary<string, object>();
headers.Add("format", "pdf");
headers.Add("shape", "a4");

var properties = channel.CreateBasicProperties();
properties.Headers = headers;
properties.Persistent = true;

var product = new Product {Id = 1, Name = "Bilgisayar", Price = 21000, Stock = 35};
var productJsonString = JsonSerializer.Serialize(product);

channel.BasicPublish("headers-exchange", string.Empty, properties,
    Encoding.UTF8.GetBytes(productJsonString));

Console.WriteLine("Mesaj gönderilmiştir...");
Console.ReadLine();