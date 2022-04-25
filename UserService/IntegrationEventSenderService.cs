﻿using RabbitMQ.Client;
using System.Text;
using System.Threading;
using UserService.Data;

namespace UserService;

public class IntegrationEventSenderService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private CancellationTokenSource _wakeupCancellationTokenSource = new CancellationTokenSource();

    public IntegrationEventSenderService(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
        using var scope = scopeFactory.CreateScope();
        using var dbContext = scope.ServiceProvider.GetRequiredService<UserServiceContext>();
        dbContext.Database.EnsureCreated();
    }

    public void StartPublishingOutstandingIntegrationEvents()
    {
        _wakeupCancellationTokenSource.Cancel();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await PublishOutstandingIntegrationEvents(stoppingToken);
        }
    }

    private async Task PublishOutstandingIntegrationEvents(CancellationToken stoppingToken)
    {
        try
        {
            var factory = new ConnectionFactory();
            var connection = factory.CreateConnection();
            var channel = connection.CreateModel();
            channel.ConfirmSelect();
            IBasicProperties prop = channel.CreateBasicProperties();
            prop.DeliveryMode = 2;

            while (!stoppingToken.IsCancellationRequested)
            {
                {
                    using var scope = _scopeFactory.CreateScope();
                    using var dbContext = scope.ServiceProvider.GetRequiredService<UserServiceContext>();
                    var events = dbContext.IntegrationEventOutbox.OrderBy(x => x.ID).ToList();

                    foreach (var e in events)
                    {
                        var body = Encoding.UTF8.GetBytes(e.Data);
                        channel
                            .BasicPublish(
                                exchange: "user",
                                routingKey: e.Event,
                                basicProperties: null,
                                body: body
                            );
                        channel.WaitForConfirmsOrDie(new TimeSpan(0, 0, 5));
                        Console.WriteLine($"Published: {e.Event} {e.Data}");
                        dbContext.Remove(e);
                        dbContext.SaveChanges();
                    }
                }

                using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(_wakeupCancellationTokenSource.Token, stoppingToken);

                try
                {
                    await Task.Delay(Timeout.Infinite, stoppingToken);
                }
                catch(OperationCanceledException)
                {
                    if(_wakeupCancellationTokenSource.Token.IsCancellationRequested)
                    {
                        Console.WriteLine("Publish requested");
                        var tmp = _wakeupCancellationTokenSource;
                        _wakeupCancellationTokenSource = new CancellationTokenSource();
                        tmp.Dispose();
                    }
                    else if(stoppingToken.IsCancellationRequested)
                    {
                        Console.WriteLine("Shutting down.");
                    }
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
        }
    }
}
