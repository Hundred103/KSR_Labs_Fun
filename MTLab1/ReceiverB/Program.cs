using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using MassTransit;
using IMessage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Reflection.Metadata;

namespace ReceiverB
{
    
    class Odbiorca1 : IConsumer<IWiadomosc1>
    {
        int wartosc = 0;
        public Task Consume(ConsumeContext<IWiadomosc1> ctx)
        {
            wartosc += 1;
            return Console.Out.WriteLineAsync(
                $"Odebrano juz {wartosc} wiadomosci 1: " +
                $"message 1 1: {ctx.Headers.GetAll().Where(elem => elem.Key == "head1").First().Value}" +
                $"\nmessage 1 2: {ctx.Headers.GetAll().Where(elem => elem.Key == "head2").First().Value}" +
                $"\n - {ctx.Message.message1}");
        }
    }
    class Odbiorca2 : IConsumer<IWiadomosc2>
    {
        int wartosc = 0;
        public Task Consume(ConsumeContext<IWiadomosc2> ctx)
        {
            wartosc += 1;
            return Console.Out.WriteLineAsync(
                $"Odebrano juz {wartosc} wiadomosci 2: " +
                $"message 2 1: {ctx.Headers.GetAll().Where(elem => elem.Key == "head1").First().Value}" +
                $"\nmessage 2 2: {ctx.Headers.GetAll().Where(elem => elem.Key == "head2").First().Value}" +
                $"\n - {ctx.Message.message2}");
        }
    }
    class Odbiorca3 : IConsumer<IWiadomosc3>
    {
        int wartosc = 0;
        public Task Consume(ConsumeContext<IWiadomosc3> ctx)
        {
            wartosc += 1;
            return Console.Out.WriteLineAsync(
                $"Odebrano juz {wartosc} wiadomosci 3: " +
                $"message 3 1: {ctx.Headers.GetAll().Where(elem => elem.Key == "head1").First().Value}" +
                $"\nmessage 3 2: {ctx.Headers.GetAll().Where(elem => elem.Key == "head2").First().Value}" +
                $"\n - {ctx.Message.message1}" +
                $"\n - {ctx.Message.message2}");
        }
    }

    class Program
    {
        static async Task Main(string[] args)
        {
            //var odb1 = new Odbiorca1();
            //var odb2 = new Odbiorca2();
            var odb3 = new Odbiorca3();
            var builder = Host.CreateApplicationBuilder(args);

            builder.Services.AddMassTransit(x =>
            {
                x.UsingRabbitMq((context, cfg) => {
                    cfg.Host(new Uri("rabbitmq://goose.rmq2.cloudamqp.com/mdioawae"), h => {
                        h.Username("mdioawae");
                        h.Password("NlLrFmnTMFixhtblSy6CrrchT33yZhYS");
                    });
                    //cfg.ReceiveEndpoint("recvqA", ec =>
                    //{
                    //    ec.Instance(odb1);
                    //});
                    cfg.ReceiveEndpoint("recvqB", ec =>
                    {
                        ec.Instance(odb3);
                    });
                    //cfg.ReceiveEndpoint("recvqC", ec =>
                    //{
                    //    ec.Instance(odb2);
                    //});
                });
            });

            var host = builder.Build();

            await host.StartAsync();
            Console.WriteLine("ReceiverB");
            Console.ReadKey();
            await host.StopAsync();
        }
    }
}
