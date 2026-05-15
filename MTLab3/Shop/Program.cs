using MassTransit;
using MassTransit.Saga;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Automatonymous;
using ExtensionClass;
using Message;

var repo = new InMemorySagaRepository<RejestracjaZamowienie>();
var saga = new RejestracjaSklep();

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

                cfg.UseInMemoryScheduler();

                cfg.ReceiveEndpoint("saga",
                    ep => ep.StateMachineSaga(saga, repo));
            });
        });
    });

var host = builder.Build();
await host.StartAsync();
ConsoleCol.WriteLine("Shop open", ConsoleColor.Cyan);

while(Console.ReadKey().Key != ConsoleKey.Q)
{
    // wait for q
}
ConsoleCol.WriteLine("\nQuitting", ConsoleColor.Cyan);
await host.StopAsync();

public class RejestracjaZamowienie : SagaStateMachineInstance
{
    public Guid CorrelationId { get; set; }
    public string CurrentState { get; set; }
    public string Login { get; set; }
    public int Ilosc { get; set; }
    public Guid? TimeoutId { get; set; }
}
public class RejestracjaSklep : MassTransitStateMachine<RejestracjaZamowienie>
{
    public State PendingConfirmation { get; private set; }
    public State ConfirmedByCustomer { get; private set; }
    public State ConfirmedByWarehouse { get; private set; }

    public Event<StartZamowienia> StartZamowienia { get; private set; }
    public Event<Potwierdzenie> Potwierdzenie { get; private set; }
    public Event<BrakPotwierdzenia> BrakPotwierdzenia { get; private set; }
    public Event<OdpowiedzWolne> OdpowiedzWolne { get; private set; }
    public Event<OdpowiedzWolneNegatywna> OdpowiedzWolneNegatywna { get; set; }
    public Schedule<RejestracjaZamowienie, TimeoutMessage> TO { get; set; }

    public RejestracjaSklep()
    {
        InstanceState(x => x.CurrentState);

        Event(() => StartZamowienia,
            x => x.CorrelateBy(
                    s => s.Login,
                    ctx => ctx.Message.Login
                ).SelectId(context => Guid.NewGuid())
            );

        Schedule(() => TO,
                x => x.TimeoutId,
                x => { x.Delay = TimeSpan.FromSeconds(10); }
            );

        Initially(

            When(StartZamowienia)
            .Schedule(TO, ctx => new TimeoutMessage() { CorrelationId = ctx.Instance.CorrelationId })
            .Then(ctx => ctx.Instance.Login = ctx.Data.Login)
            .Then(ctx => ctx.Instance.Ilosc = ctx.Data.Ilosc)
            .ThenAsync(context => { return ConsoleCol.WriteLineAsync($"Order for {context.Data.Login} in quantity {context.Data.Ilosc}", ConsoleColor.Cyan); })
            .Respond(ctx => { return new PytanieoPotwierdzenie() { CorrelationId = ctx.Instance.CorrelationId, Login = ctx.Instance.Login }; })
            .Respond(ctx => { return new PytanieoWolne() { CorrelationId = ctx.Instance.CorrelationId, Ilosc = ctx.Instance.Ilosc }; })
            .TransitionTo(PendingConfirmation)
            );

        During(PendingConfirmation,

            When(TO.Received)
            .ThenAsync(context => { return ConsoleCol.WriteLineAsync($"TIMEOUT: user {context.Instance.Login} order {context.Data.CorrelationId}", ConsoleColor.Red); })
            .Respond(ctx => { return new OdrzucenieZamowienia() { CorrelationId = ctx.Instance.CorrelationId, Login = ctx.Instance.Login, Ilosc = ctx.Instance.Ilosc }; })
            .Finalize(),

            When(Potwierdzenie)
            .ThenAsync(context => { return ConsoleCol.WriteLineAsync($"{context.Instance.Login} confirmed order {context.Data.CorrelationId}", ConsoleColor.Green); })
            .Unschedule(TO)
            .TransitionTo(ConfirmedByCustomer),

            When(BrakPotwierdzenia)
            .ThenAsync(context => { return ConsoleCol.WriteLineAsync($"{context.Instance.Login} did not confirm order {context.Data.CorrelationId}", ConsoleColor.Red); })
            .Respond(ctx => { return new OdrzucenieZamowienia() { CorrelationId = ctx.Instance.CorrelationId, Login = ctx.Instance.Login, Ilosc = ctx.Instance.Ilosc }; })
            .Finalize(),

            When(OdpowiedzWolne)
            .ThenAsync(context => { return ConsoleCol.WriteLineAsync($"Processing order {context.Data.CorrelationId}", ConsoleColor.Cyan); })
            .TransitionTo(ConfirmedByWarehouse),

            When(OdpowiedzWolneNegatywna)
            .ThenAsync(context => { return ConsoleCol.WriteLineAsync($"Warehouse cannot process order {context.Data.CorrelationId}", ConsoleColor.Red); })
            .Respond(ctx => { return new OdrzucenieZamowienia() { CorrelationId = ctx.Instance.CorrelationId, Login = ctx.Instance.Login, Ilosc = ctx.Instance.Ilosc }; })
            .Finalize()
            );

        During(ConfirmedByCustomer,

            When(OdpowiedzWolne)
            .ThenAsync(context => { return ConsoleCol.WriteLineAsync($"Processing order {context.Data.CorrelationId}", ConsoleColor.Cyan); })
            .Respond(ctx => { return new AkceptacjaZamowienia() { CorrelationId = ctx.Instance.CorrelationId, Login = ctx.Instance.Login, Ilosc = ctx.Instance.Ilosc }; })
            .Finalize(),

            When(OdpowiedzWolneNegatywna)
            .ThenAsync(context => { return ConsoleCol.WriteLineAsync($"Order {context.Data.CorrelationId} cannot be fulfilled", ConsoleColor.Red); })
            .Respond(ctx => { return new OdrzucenieZamowienia() { CorrelationId = ctx.Instance.CorrelationId, Login = ctx.Instance.Login, Ilosc = ctx.Instance.Ilosc }; })
            .Finalize()
            );

        During(ConfirmedByWarehouse,

            When(TO.Received)
            .ThenAsync(context => { return ConsoleCol.WriteLineAsync($"TIMEOUT: user {context.Instance.Login} order {context.Data.CorrelationId}", ConsoleColor.Red); })
            .Respond(ctx => { return new OdrzucenieZamowienia() { CorrelationId = ctx.Instance.CorrelationId, Login = ctx.Instance.Login, Ilosc = ctx.Instance.Ilosc }; })
            .Finalize(),

            When(Potwierdzenie)
            .ThenAsync(context => { return ConsoleCol.WriteLineAsync($"{context.Instance.Login} confirmed order {context.Data.CorrelationId}", ConsoleColor.Green); })
            .Respond(ctx => { return new AkceptacjaZamowienia() { CorrelationId = ctx.Instance.CorrelationId, Login = ctx.Instance.Login, Ilosc = ctx.Instance.Ilosc }; })
            .Unschedule(TO)
            .Finalize(),

            When(BrakPotwierdzenia)
            .ThenAsync(context => { return ConsoleCol.WriteLineAsync($"{context.Instance.Login} did not confirm order {context.Data.CorrelationId}", ConsoleColor.Red); })
            .Respond(ctx => { return new OdrzucenieZamowienia() { CorrelationId = ctx.Instance.CorrelationId, Login = ctx.Instance.Login, Ilosc = ctx.Instance.Ilosc }; })
            .Finalize()
            );

        SetCompletedWhenFinalized();
    }
}
