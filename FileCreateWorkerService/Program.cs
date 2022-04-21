using FileCreateWorkerService;
using FileCreateWorkerService.Services;
using RabbitMQ.Client;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((hostContext, services) =>
    {
        services.AddHostedService<Worker>()
            .AddSingleton(sp => new ConnectionFactory()
            {
                Uri = new Uri(
                    hostContext.Configuration.GetConnectionString("RabbitMQ")),
                DispatchConsumersAsync = true
            })
            .AddSingleton<RabbitMqClientService>();
    })
    .Build();

await host.RunAsync();