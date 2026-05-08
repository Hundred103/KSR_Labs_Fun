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
            x.AddConsumer<PublMsgConsumerB>();
            x.AddConsumer<ReplyBErrConsumer>();

            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(new Uri("rabbitmq://goose.rmq2.cloudamqp.com/mdioawae"), h =>
                {
                    h.Username("mdioawae");
                    h.Password("NlLrFmnTMFixhtblSy6CrrchT33yZhYS");
                });

                cfg.ReceiveEndpoint("subscriber_b_queue", ep =>
                {
                    ep.ConfigureConsumer<PublMsgConsumerB>(context);
                    ep.ConfigureConsumer<ReplyBErrConsumer>(context);
                });
            });
        });
    });

var host = builder.Build();

ConsoleCol.WriteLine("[B] Subscriber B started", ConsoleCol.Colors.BrightCyan);
await host.RunAsync();

public class PublMsgConsumerB : IConsumer<PublishMsg>
{
    public async Task Consume(ConsumeContext<PublishMsg> context)
    {
        var n = context.Message.Num;
        if (n % 3 == 0)
        {
            try
            {
                await context.RespondAsync(new ReplyB { Sender = "subscriber B" });
                ConsoleCol.WriteLine($"[B] Reply to {n}", ConsoleCol.Colors.BrightCyan);
            }
            catch
            {
                ConsoleCol.WriteLine("[B] Exception in reply to Publisher!", ConsoleCol.Colors.BrightRed);
            }
        }
    }
}


public class ReplyBErrConsumer : IConsumer<ReplyBErr>
{
    public Task Consume(ConsumeContext<ReplyBErr> context)
    {
        var err = context.Message;
        ConsoleCol.WriteLine($"[B] Received error info from Publisher! Attempt: {err.AttemptNumber}, Details: {err.ErrorMessage}", ConsoleColor.Red);
        return Task.CompletedTask;
    }
}