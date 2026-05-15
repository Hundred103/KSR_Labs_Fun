using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using MassTransit;
using ExtensionClass;
using Message;

var inbox = new Inbox();

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
                cfg.ReceiveEndpoint("klienta",
                    ep => ep.Instance(inbox));
            });
        });
    });

var host = builder.Build();
var bus = host.Services.GetRequiredService<IBusControl>();
await host.StartAsync();

ConsoleCol.WriteLine(ClientData.Login, ConsoleColor.Blue);
bool quit = false;
while (!quit)
{
    if (Console.KeyAvailable)
    {
        switch (Console.ReadKey().Key)
        {
            case ConsoleKey.B:
                ConsoleCol.WriteLine("\nEnter quantity", ConsoleColor.Blue);

                int quantity = 0;
                try
                {
                    quantity = Convert.ToInt32(Console.ReadLine());
                    await bus.Publish(new StartZamowienia() { Login = ClientData.Login, Ilosc = quantity });
                }
                catch (Exception)
                {
                    ConsoleCol.WriteLine("You did not enter a number! Operation cancelled", ConsoleColor.Red);
                }
                break;
            case ConsoleKey.Q:
                ConsoleCol.WriteLine("\nQuitting", ConsoleColor.Blue);
                quit = true;
                break;
        }
    }
    else
    {
        await Task.Delay(10);
    }
}

await host.StopAsync();


static class ClientData
{
    public const string Login = "KlientA";
}
class Inbox : IConsumer<IPytanieoPotwierdzenie>, IConsumer<IAkceptacjaZamowienia>, IConsumer<IOdrzucenieZamowienia>
{
    public Task Consume(ConsumeContext<IPytanieoPotwierdzenie> context)
    {
        if (context.Message.Login == ClientData.Login)
        {
            ConsoleCol.WriteLine($"Accept order {context.Message.CorrelationId}? [t = yes, anything else = no]", ConsoleColor.Blue);
            var accept = Console.ReadKey().Key == ConsoleKey.T;
            Console.WriteLine();

            return accept
                ? context.RespondAsync(new Potwierdzenie() { CorrelationId = context.Message.CorrelationId })
                : context.RespondAsync(new BrakPotwierdzenia() { CorrelationId = context.Message.CorrelationId });
        }

        return Task.CompletedTask;
    }

    public Task Consume(ConsumeContext<IAkceptacjaZamowienia> context)
    {
        if (context.Message.Login == ClientData.Login)
            return ConsoleCol.WriteLineAsync($"Accepted {context.Message.CorrelationId} for quantity {context.Message.Ilosc}", ConsoleColor.Green);

        return Task.CompletedTask;
    }

    public Task Consume(ConsumeContext<IOdrzucenieZamowienia> context)
    {
        if (context.Message.Login == ClientData.Login)
            return ConsoleCol.WriteLineAsync($"Declined {context.Message.CorrelationId} for quantity {context.Message.Ilosc}", ConsoleColor.Red);

        return Task.CompletedTask;
    }
}


