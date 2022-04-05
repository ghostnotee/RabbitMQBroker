using System.Drawing;
using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQWeb.WaterMark.Services;

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
        consumer.Received += ConsumerOnReceived;
        return Task.CompletedTask;
    }

    private Task ConsumerOnReceived(object sender, BasicDeliverEventArgs @event)
    {
        try
        {
            var productImageCreatedEvent =
                JsonSerializer.Deserialize<productImageCreatedEvent>(Encoding.UTF8.GetString(@event.Body.ToArray()));
            var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images",
                productImageCreatedEvent.ImageName);
            var siteName = "www.ghostNote.com";
            using var image = Image.FromFile(path);
            using var graphic = Graphics.FromImage(image);
            var font = new Font(FontFamily.GenericMonospace, 32, FontStyle.Bold, GraphicsUnit.Pixel);
            var textSize = graphic.MeasureString(siteName, font);
            var color = Color.FromArgb(128, 255, 255, 255);
            var brush = new SolidBrush(color);
            var position = new Point(image.Width - ((int) textSize.Width + 30),
                image.Height - ((int) textSize.Height + 30));

            graphic.DrawString(siteName, font, brush, position);
            image.Save(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "watermarks",
                productImageCreatedEvent.ImageName));

            image.Dispose();
            graphic.Dispose();
            
            _channel.BasicAck(@event.DeliveryTag,false);
        }
        catch (Exception e)
        {
            throw;
        }

        return Task.CompletedTask;
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        return base.StopAsync(cancellationToken);
    }
}