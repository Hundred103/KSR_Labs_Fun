using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MassTransit;
using IMessage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ReceiverC
{
    class Program
    {
        public static Task Handler(ConsumeContext<IWiadomosc2> ctx) =>
               Console.Out.WriteLineAsync(
                   $"message 2 1: {ctx.Headers.GetAll().Where(elem => elem.Key == "head1").First().Value}" +
                   $"\nmessage 2 2: {ctx.Headers.GetAll().Where(elem => elem.Key == "head2").First().Value}" +
                   $"\n - {ctx.Message.message2}");
        static async Task Main(string[] args)
        {
            var builder = Host.CreateApplicationBuilder(args);

            builder.Services.AddMassTransit(x =>
            {
                x.UsingRabbitMq((context, cfg) => {
                    cfg.Host(new Uri("rabbitmq://goose.rmq2.cloudamqp.com/mdioawae"), h => {
                        h.Username("mdioawae");
                        h.Password("NlLrFmnTMFixhtblSy6CrrchT33yZhYS");
                    });
                    cfg.ReceiveEndpoint("recvqC", ec =>
                    {
                        ec.Handler<IWiadomosc2>(Handler);
                    });
                });
            });

            var host = builder.Build();

            await host.StartAsync();
            Console.WriteLine("ReceiverC");
            Console.ReadKey();
            await host.StopAsync();
        }
    }
}
