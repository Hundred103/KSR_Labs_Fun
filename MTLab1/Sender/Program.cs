using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using MassTransit;
using IMessage;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Sender
{
    class Wiadomosc1 : IWiadomosc1
    {
        public string message1 { get; set; }
    }
    class Wiadomosc2 : IWiadomosc2
    {
        public string message2 { get; set; }
    }
    class Wiadomosc3 : IWiadomosc3
    {
        public string message1 { get; set; }
        public string message2 { get; set; }
    }
    class Program
    {
        static async Task Main(string[] args)
        {

            var builder = Host.CreateApplicationBuilder(args);

            builder.Logging.ClearProviders();
            builder.Logging.AddConsole();
            builder.Logging.SetMinimumLevel(LogLevel.Warning);

            builder.Services.AddMassTransit(x =>
            {
                x.UsingRabbitMq((context, cfg) => {
                    cfg.Host(new Uri("rabbitmq://goose.rmq2.cloudamqp.com/mdioawae"), h => {
                        h.Username("mdioawae");
                        h.Password("NlLrFmnTMFixhtblSy6CrrchT33yZhYS");
                    });
                });
            });

            var host = builder.Build();


            await host.StartAsync();

            var publishEndpoint = host.Services.GetRequiredService<IPublishEndpoint>();

            Console.WriteLine("Sender");

            while (Console.ReadKey().Key != ConsoleKey.Escape)
            {
                for (int i = 0; i < 10; i++)
                {
                    await publishEndpoint.Publish(new Wiadomosc1() { message1 = $"message1 {i}" },
                        ctx =>
                        {
                            ctx.Headers.Set("head1", $"a{i}");
                            ctx.Headers.Set("head2", $"a{i}");
                        });
                    await publishEndpoint.Publish(new Wiadomosc2() { message2 = $"message2 nr{i}" },
                        ctx =>
                        {
                            ctx.Headers.Set("head1", $"c{i}");
                            ctx.Headers.Set("head2", $"c{i}");
                        });
                    await publishEndpoint.Publish(new Wiadomosc3() { message1 = $"message1 nr{i} v2", message2 = $"message2 nr{i} v2" },
                        ctx =>
                        {
                            ctx.Headers.Set("head1", $"b{i}");
                            ctx.Headers.Set("head2", $"b{i}");
                        });
                }
            }
            await host.StopAsync();
        }
    }
}
