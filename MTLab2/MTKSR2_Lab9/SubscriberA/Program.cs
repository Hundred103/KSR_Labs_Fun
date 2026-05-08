using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using Message;
using ExtensionClass;

var builder = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        services.AddMassTransit(x =>
        {
            x.AddConsumer<PublMsgConsumerA>();
            x.AddConsumer<ReplyAErrConsumer>();

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
                    ep.ConfigureConsumer<ReplyAErrConsumer>(context);
                });
            });
        });
    });

var host = builder.Build();

ConsoleCol.WriteLine("[A] Subscriber A started", ConsoleCol.Colors.BrightGreen);
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
                ConsoleCol.WriteLine($"[A] Reply to {n}", ConsoleCol.Colors.BrightGreen);
            }
            catch
            {
                ConsoleCol.WriteLine("[A] Exception in reply to Publisher!", ConsoleCol.Colors.BrightRed);
            }
        }
    }
}

public class ReplyAErrConsumer : IConsumer<ReplyAErr>
{
    public Task Consume(ConsumeContext<ReplyAErr> context)
    {
        var err = context.Message;
        ConsoleCol.WriteLine($"[A] Received error info from Publisher! Attempt: {err.AttemptNumber}, Details: {err.ErrorMessage}", ConsoleColor.Red);
        return Task.CompletedTask;
    }
}