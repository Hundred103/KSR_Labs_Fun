using System;
using System.Threading.Tasks;
using MassTransit;
using Message;
using ExtensionClass;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using static MassTransit.MessageHeaders;

var warehouse = new Warehouse();

var builder = Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder(args)
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
                cfg.ReceiveEndpoint("magazyn",
                    ep => ep.Instance(warehouse));
            });
        });
    });

var host = builder.Build();
await host.StartAsync();
ConsoleCol.WriteLine("Warehouse", ConsoleColor.Yellow);
int deliveredProductCount = 0;

bool quit = false;
while (!quit)
{
    switch (Console.ReadKey().Key)
    {
        case ConsoleKey.D:
            ConsoleCol.WriteLine("\nEnter delivered product count", ConsoleColor.Yellow);
            try
            {
                deliveredProductCount = Convert.ToInt32(Console.ReadLine());
                warehouse.Wolne += deliveredProductCount;
                ConsoleCol.WriteLine($"\nWarehouse state \nDELIVERY: {deliveredProductCount} \nFree: {warehouse.Wolne} \nReserved: {warehouse.Zarezerwowane}", ConsoleColor.Yellow);
            }
            catch (Exception)
            {
                ConsoleCol.WriteLine("You did not enter a number! Operation cancelled", ConsoleColor.Red);
            }
            break;
        case ConsoleKey.Q:
            ConsoleCol.WriteLine("\nQuitting", ConsoleColor.Yellow);
            quit = true;
            break;
    }
}
await host.StopAsync();

class Warehouse : IConsumer<IPytanieoWolne>, IConsumer<IAkceptacjaZamowienia>, IConsumer<IOdrzucenieZamowienia>
{
    public int Wolne { get; set; } = 0;
    public int Zarezerwowane { get; set; } = 0;
    public Task Consume(ConsumeContext<IPytanieoWolne> context)
    {
        return Task.Run(() =>
        {

            if (Wolne >= context.Message.Ilosc)
            {
                Wolne -= context.Message.Ilosc;
                Zarezerwowane += context.Message.Ilosc;
                ConsoleCol.WriteLineAsync($"Processing order {context.Message.CorrelationId} for quantity {context.Message.Ilosc}", ConsoleColor.Green);
                context.RespondAsync(new OdpowiedzWolne() { CorrelationId = context.Message.CorrelationId });
            }
            else
            {
                Zarezerwowane += context.Message.Ilosc;
                Wolne -= context.Message.Ilosc;
                ConsoleCol.WriteLineAsync($"Cannot process order {context.Message.CorrelationId} for quantity {context.Message.Ilosc}", ConsoleColor.Red);
                context.RespondAsync(new OdpowiedzWolneNegatywna() { CorrelationId = context.Message.CorrelationId });
            }
        });
    }

    public Task Consume(ConsumeContext<IAkceptacjaZamowienia> context)
    {
        return Task.Run(() =>
        {
            Zarezerwowane -= context.Message.Ilosc;
            ConsoleCol.WriteLine($"Order completed: {context.Message.CorrelationId} for quantity {context.Message.Ilosc} package sent to customer", ConsoleColor.Green);
        });
    }
    public Task Consume(ConsumeContext<IOdrzucenieZamowienia> context)
    {
        return Task.Run(() =>
        {
            Zarezerwowane -= context.Message.Ilosc;
            Wolne += context.Message.Ilosc;
            ConsoleCol.WriteLine($"Order rejected: {context.Message.CorrelationId} for quantity {context.Message.Ilosc} reserved products are available for sale again", ConsoleColor.Red);
        });
    }
}