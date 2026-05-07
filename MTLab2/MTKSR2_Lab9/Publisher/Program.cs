using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;
using Message;
using ConsoleCol;

var builder = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        services.AddSingleton<PublisherState>();
        
        services.AddMassTransit(x =>
        {
            x.AddConsumer<ReplyAConsumer>();
            x.AddConsumer<ReplyBConsumer>();
            x.AddConsumer<ConfigConsumer>();
            
            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(new Uri("rabbitmq://goose.rmq2.cloudamqp.com/mdioawae"), h =>
                {
                    h.Username("mdioawae");
                    h.Password("NlLrFmnTMFixhtblSy6CrrchT33yZhYS");
                });

                cfg.ReceiveEndpoint("publisher_queue", ep =>
                {
                    ep.UseMessageRetry(r => r.Immediate(5));
                    ep.ConfigureConsumer<ReplyAConsumer>(context);
                    ep.ConfigureConsumer<ReplyBConsumer>(context);
                    ep.ConfigureConsumer<ConfigConsumer>(context);
                });
            });
        });

        services.AddHostedService<PublisherService>();
    });

var host = builder.Build();

ConsoleCol.ConsoleCol.WriteLine("[P] Publisher started", ConsoleCol.ConsoleCol.Colors.BrightYellow);
await host.RunAsync();

public class PublisherState
{
    public int Num { get; set; } = 1;
    public bool Active { get; set; } = false;
    public int Sent { get; set; } = 0;
    public int OkA { get; set; } = 0;
    public int OkB { get; set; } = 0;
    public int TotalA { get; set; } = 0;
    public int TotalB { get; set; } = 0;
}

public class ReplyAConsumer : IConsumer<ReplyA>
{
    private readonly PublisherState _state;

    public ReplyAConsumer(PublisherState state) => _state = state;

    public async Task Consume(ConsumeContext<ReplyA> ctx)
    {
        ConsoleCol.ConsoleCol.WriteLine($"[P] Got ReplyA from {ctx.Message.Sender}", ConsoleCol.ConsoleCol.Colors.BrightGreen);
        await HandleReply(ctx.Message);
    }

    private async Task HandleReply(object msg)
    {
        for (int i = 0; i < 5; i++)
        {
            if (msg is ReplyA) _state.TotalA++;
            if (msg is ReplyB) _state.TotalB++;
            if (new Random().NextDouble() < 0.33)
            {
                ConsoleCol.ConsoleCol.WriteLine("[P] Error handling reply!", ConsoleCol.ConsoleCol.Colors.BrightRed);
                await Task.Delay(100);
                continue;
            }

            if (msg is ReplyA) _state.OkA++;
            if (msg is ReplyB) _state.OkB++;
            break;
        }
    }
}

public class ReplyBConsumer : IConsumer<ReplyB>
{
    private readonly PublisherState _state;

    public ReplyBConsumer(PublisherState state) => _state = state;

    public async Task Consume(ConsumeContext<ReplyB> ctx)
    {
        ConsoleCol.ConsoleCol.WriteLine($"[P] Got ReplyB from {ctx.Message.Sender}", ConsoleCol.ConsoleCol.Colors.BrightCyan);
        await HandleReply(ctx.Message);
    }

    private async Task HandleReply(object msg)
    {
        for (int i = 0; i < 5; i++)
        {
            if (msg is ReplyA) _state.TotalA++;
            if (msg is ReplyB) _state.TotalB++;
            if (new Random().NextDouble() < 0.33)
            {
                ConsoleCol.ConsoleCol.WriteLine("[P] Error handling reply!", ConsoleCol.ConsoleCol.Colors.BrightRed);
                await Task.Delay(100);
                continue;
            }

            if (msg is ReplyA) _state.OkA++;
            if (msg is ReplyB) _state.OkB++;
            break;
        }
    }
}

public class ConfigConsumer : IConsumer<EncryptedConfig>
{
    private readonly PublisherState _state;

    public ConfigConsumer(PublisherState state) => _state = state;

    public Task Consume(ConsumeContext<EncryptedConfig> ctx)
    {
        try
        {
            var cfg = EncryptionHelper.Decrypt(ctx.Message);
            _state.Active = cfg.Active;
            ConsoleCol.ConsoleCol.WriteLine($"[P] Decrypted Config: active = {_state.Active}", ConsoleCol.ConsoleCol.Colors.BrightMagenta);
        }
        catch (Exception ex)
        {
            ConsoleCol.ConsoleCol.WriteLine($"[P] Decryption error: {ex.Message}", ConsoleCol.ConsoleCol.Colors.BrightRed);
        }
        return Task.CompletedTask;
    }
}

public class PublisherService : BackgroundService
{
    private readonly IBusControl _bus;
    private readonly PublisherState _state;

    public PublisherService(IBusControl bus, PublisherState state)
    {
        _bus = bus;
        _state = state;
    }

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        var pubTask = Task.Run(async () =>
        {
            while (!ct.IsCancellationRequested)
            {
                if (_state.Active)
                {
                    var msg = new PublishMsg { Num = _state.Num };
                    await _bus.Publish(msg, ct);
                    ConsoleCol.ConsoleCol.WriteLine($"[P] Sent PublishMsg {_state.Num}", ConsoleCol.ConsoleCol.Colors.BrightYellow);
                    _state.Sent++;
                    _state.Num++;
                }
                await Task.Delay(1000, ct);
            }
        }, ct);

        var interactiveTask = Task.Run(() =>
        {
            while (!ct.IsCancellationRequested)
            {
                try
                {
                    var key = Console.ReadKey(true).KeyChar;
                    if (key == 's')
                    {
                        ConsoleCol.ConsoleCol.WriteLine("\n[P] Stats:", ConsoleCol.ConsoleCol.Colors.Orange);
                        ConsoleCol.ConsoleCol.WriteLine($"- ReplyA: {_state.TotalA}, ReplyB: {_state.TotalB}", ConsoleCol.ConsoleCol.Colors.BrightGreen);
                        ConsoleCol.ConsoleCol.WriteLine($"- OkA: {_state.OkA}, OkB: {_state.OkB}", ConsoleCol.ConsoleCol.Colors.BrightCyan);
                        ConsoleCol.ConsoleCol.WriteLine($"- Sent: {_state.Sent}", ConsoleCol.ConsoleCol.Colors.BrightYellow);
                    }
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }
        }, ct);

        await Task.WhenAny(pubTask, interactiveTask);
    }
}
