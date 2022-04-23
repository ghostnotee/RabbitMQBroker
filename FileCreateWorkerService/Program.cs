using FileCreateWorkerService;
using FileCreateWorkerService.Models;
using FileCreateWorkerService.Services;
using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((hostContext, services) =>
    {
        services.AddHostedService<Worker>().AddDbContext<AdventureWorks2019Context>(options =>
            {
                options.UseSqlServer(hostContext.Configuration.GetConnectionString("SqlServer"));
            })
            .AddSingleton(sp => new ConnectionFactory()
            {
                Uri = new Uri(hostContext.Configuration.GetConnectionString("RabbitMQ")), DispatchConsumersAsync = true
            }).AddSingleton<RabbitMqClientService>();
    })
    .Build();

await host.RunAsync();