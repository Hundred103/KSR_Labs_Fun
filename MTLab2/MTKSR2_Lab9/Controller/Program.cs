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
            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(new Uri("rabbitmq://goose.rmq2.cloudamqp.com/mdioawae"), h =>
                {
                    h.Username("mdioawae");
                    h.Password("NlLrFmnTMFixhtblSy6CrrchT33yZhYS");
                });
            });
        });
    });

var host = builder.Build();
var bus = host.Services.GetRequiredService<IBusControl>();

ConsoleCol.WriteLine("[C] Controller started", ConsoleCol.Colors.BrightYellow);

var cts = new CancellationTokenSource();
_ = host.RunAsync(cts.Token);

while (true)
{
    var key = Console.ReadKey(true).KeyChar;
    if (key == 's')
    {
        var enc = EncryptionHelper.Encrypt(new Config { Active = true });
        await bus.Publish(enc);
        ConsoleCol.WriteLine("[C] Start (active = true)", ConsoleCol.Colors.BrightGreen);
    }
    if (key == 't')
    {
        var enc = EncryptionHelper.Encrypt(new Config { Active = false });
        await bus.Publish(enc);
        ConsoleCol.WriteLine("[C] Stop (active = false)", ConsoleCol.Colors.BrightRed);
    }
}