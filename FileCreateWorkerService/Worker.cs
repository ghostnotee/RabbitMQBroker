using System.Data;
using System.Text;
using System.Text.Json;
using ClosedXML.Excel;
using FileCreateWorkerService.Models;
using FileCreateWorkerService.Services;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Shared;
using Product = FileCreateWorkerService.Models.Product;

namespace FileCreateWorkerService;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly RabbitMqClientService _rabbitMqClientService;
    private readonly IServiceProvider _serviceProvider;
    private IModel _channel;

    public Worker(ILogger<Worker> logger, RabbitMqClientService rabbitMqClientService, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _rabbitMqClientService = rabbitMqClientService;
        _serviceProvider = serviceProvider;
    }

    public override Task StartAsync(CancellationToken cancellationToken)
    {
        _channel = _rabbitMqClientService.Connect();
        _channel.BasicQos(0, 1, false);
        return base.StartAsync(cancellationToken);
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var consumer = new AsyncEventingBasicConsumer(_channel);
        _channel.BasicConsume(RabbitMqClientService.QueueName, false, consumer);
        consumer.Received += ConsumerOnReceived;

        return Task.CompletedTask;
    }

    private async Task ConsumerOnReceived(object sender, BasicDeliverEventArgs @event)
    {
        var createExcelMessage =
            JsonSerializer.Deserialize<CreateExcelMessage>(Encoding.UTF8.GetString(@event.Body.ToArray()));

        using var memoryStream = new MemoryStream();

        var workBook = new XLWorkbook();
        var dataSet = new DataSet();
        dataSet.Tables.Add(GetTable("products"));
        workBook.Worksheets.Add(dataSet);
        workBook.SaveAs(memoryStream);

        MultipartFormDataContent multipartFormDataContent = new();
        multipartFormDataContent.Add(new ByteArrayContent(memoryStream.ToArray()), "file",
            Guid.NewGuid().ToString() + ".xlsx");

        var baseUrl = "https://localhost:7230/api/files";
        using (var httpClient = new HttpClient())
        {
            var response = await httpClient.PostAsync($"{baseUrl}?fileId={createExcelMessage.FileId}",
                multipartFormDataContent);
            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation($"File id:{createExcelMessage.FileId} was created by succesful");
                _channel.BasicAck(@event.DeliveryTag, false);
            }
        }
    }

    private DataTable GetTable(string tableName)
    {
        List<Product> products;
        using (var scope = _serviceProvider.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AdventureWorks2019Context>();
            products = context.Products.ToList();
        }

        DataTable table = new() {TableName = tableName};
        table.Columns.Add("ProductId", typeof(int));
        table.Columns.Add("Name", typeof(string));
        table.Columns.Add("ProductNumber", typeof(string));
        table.Columns.Add("Color", typeof(string));

        products.ForEach(p => { table.Rows.Add(p.ProductId, p.Name, p.ProductNumber, p.Color); });

        return table;
    }
}