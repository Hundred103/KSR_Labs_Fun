using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;
using Message;
using ExtensionClass;

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

ConsoleCol.WriteLine("[P] Publisher started", ConsoleCol.Colors.BrightYellow);
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
        ConsoleCol.WriteLine($"[P] Got ReplyA from {ctx.Message.Sender}", ConsoleCol.Colors.BrightGreen);
        _state.TotalA++;
        for (int i = 0; i < 3; i++)
        {
            try
            {
                if (new Random().NextDouble() < 0.5)
                {
                    throw new Exception($"Random failure while processing ReplyA on attempt {i + 1}!");
                }
                _state.OkA++;
                break;
            }
            catch (Exception e)
            {
                ConsoleCol.WriteLine($"[P] Error handling replyA! Attempt {i + 1}/3", ConsoleCol.Colors.BrightRed);
                await ctx.Publish(new ReplyAErr
                {
                    OriginalSender = ctx.Message.Sender,
                    AttemptNumber = i+1,
                    ErrorMessage = e.Message
                });
                await Task.Delay(100);
            }
        }
    }
}

public class ReplyBConsumer : IConsumer<ReplyB>
{
    private readonly PublisherState _state;

    public ReplyBConsumer(PublisherState state) => _state = state;

    public async Task Consume(ConsumeContext<ReplyB> ctx)
    {
        ConsoleCol.WriteLine($"[P] Got ReplyB from {ctx.Message.Sender}", ConsoleCol.Colors.BrightCyan);
        _state.TotalB++;
        for (int i = 0; i < 3; i++)
        {
            try
            {
                if (new Random().NextDouble() < 0.5)
                {
                    throw new Exception($"Random failure while processing ReplyB on attempt {i + 1}!");
                }
                _state.OkB++;
                break;
            }
            catch (Exception e)
            {
                ConsoleCol.WriteLine($"[P] Error handling replyB! Attempt {i + 1}/3", ConsoleCol.Colors.BrightRed);
                await ctx.Publish(new ReplyBErr
                {
                    OriginalSender = ctx.Message.Sender,
                    AttemptNumber = i + 1,
                    ErrorMessage = e.Message
                });
                await Task.Delay(100);
            }
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
            ConsoleCol.WriteLine($"[P] Decrypted Config: active = {_state.Active}", ConsoleCol.Colors.BrightMagenta);
        }
        catch (Exception ex)
        {
            ConsoleCol.WriteLine($"[P] Decryption error: {ex.Message}", ConsoleCol.Colors.BrightRed);
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
                    ConsoleCol.WriteLine($"[P] Sent PublishMsg {_state.Num}", ConsoleCol.Colors.BrightYellow);
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
                        ConsoleCol.WriteLine("\n[P] Stats:", ConsoleCol.Colors.Orange);
                        ConsoleCol.WriteLine($"- ReplyA: {_state.TotalA}, ReplyB: {_state.TotalB}", ConsoleCol.Colors.BrightGreen);
                        ConsoleCol.WriteLine($"- OkA: {_state.OkA}, OkB: {_state.OkB}", ConsoleCol.Colors.BrightCyan);
                        ConsoleCol.WriteLine($"- Sent: {_state.Sent}", ConsoleCol.Colors.BrightYellow);
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