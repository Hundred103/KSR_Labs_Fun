using System;
using System.Linq;
using System.Threading.Tasks;
using MassTransit;
using IMessage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ReceiverA
{
    class Program
    {
        public static Task Handler(ConsumeContext<IWiadomosc1> ctx) =>
               Console.Out.WriteLineAsync(
                   $"message 1 1: {ctx.Headers.GetAll().Where(elem => elem.Key == "head1").First().Value}" +
                   $"\nmessage 1 2: {ctx.Headers.GetAll().Where(elem => elem.Key == "head2").First().Value}" +
                   $"\n - {ctx.Message.message1}");
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
                    cfg.ReceiveEndpoint("recvqA", ec => {
                        ec.Handler<IWiadomosc1>(Handler);
                    });
                });
            });

            var host = builder.Build();

            await host.StartAsync();
            Console.WriteLine("ReceiverA");
            Console.ReadKey();
            await host.StopAsync();
        }
    }
}
