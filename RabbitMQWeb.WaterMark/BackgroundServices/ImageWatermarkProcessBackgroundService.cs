using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQWeb.WaterMark.Services;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace RabbitMQWeb.WaterMark.BackgroundServices;

public class ImageWatermarkProcessBackgroundService : BackgroundService
{
    private readonly RabbitMQClientService _rabbitMqClientService;
    private readonly ILogger<ImageWatermarkProcessBackgroundService> _logger;
    private IModel _channel;

    public ImageWatermarkProcessBackgroundService(RabbitMQClientService rabbitMqClientService,
        ILogger<ImageWatermarkProcessBackgroundService> logger)
    {
        _rabbitMqClientService = rabbitMqClientService;
        _logger = logger;
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
        _channel.BasicConsume(RabbitMQClientService.QueueName, false, consumer);
        consumer.Received += Consumer_Received;
        return Task.CompletedTask;
    }

    private Task Consumer_Received(object sender, BasicDeliverEventArgs @event)
    {
        Task.Delay(10000).Wait();
        try
        {
            var productImageCreatedEvent =
                JsonSerializer.Deserialize<productImageCreatedEvent>(Encoding.UTF8.GetString(@event.Body.ToArray()));
            var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images",
                productImageCreatedEvent.ImageName);
            var siteName = "www.ghostNote.com";

            using Image image = Image.Load(path);
            Font font = SystemFonts.CreateFont("Arial", 10);
            using var image2 = image.Clone(ctx => ctx.ApplyScalingWaterMark(font, siteName, Color.WhiteSmoke, 5));
            image2.Save(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "watermarks",
                productImageCreatedEvent.ImageName));

            _logger.LogInformation($"received message : {productImageCreatedEvent}");
            _channel.BasicAck(@event.DeliveryTag, false);
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
        }


        return Task.CompletedTask;
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        return base.StopAsync(cancellationToken);
    }
}