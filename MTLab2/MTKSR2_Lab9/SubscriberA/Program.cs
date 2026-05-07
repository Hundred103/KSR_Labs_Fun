using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using Message;

var builder = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        services.AddMassTransit(x =>
        {
            x.AddConsumer<PublMsgConsumerA>();
            
            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(new Uri("rabbitmq://goose.rmq2.cloudamqp.com/mdioawae"), h =>
                {
                    h.Username("mdioawae");
                    h.Password("NlLrFmnTMFixhtblSy6CrrchT33yZhYS");
                });

                cfg.ReceiveEndpoint("subscriber_a_queue", ep =>
                {
                    ep.ConfigureConsumer<PublMsgConsumerA>(context);
                });
            });
        });
    });

var host = builder.Build();

Console.WriteLine("[A] Subscriber A started");
await host.RunAsync();

public class PublMsgConsumerA : IConsumer<PublishMsg>
{
    public async Task Consume(ConsumeContext<PublishMsg> context)
    {
        var n = context.Message.Num;
        if (n % 2 == 0)
        {
            try
            {
                await context.RespondAsync(new ReplyA { Sender = "subscriber A" });
                Console.WriteLine($"[A] Reply to {n}");
            }
            catch
            {
                Console.WriteLine("[A] Exception in reply to Publisher!");
            }
        }
    }
}
