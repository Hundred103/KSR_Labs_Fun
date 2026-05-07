using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ExtensionClass;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace sender
{
    class MyConsumer : AsyncDefaultBasicConsumer
    {
        public MyConsumer(IChannel channel) : base(channel) { }
        public override Task HandleBasicDeliverAsync(string consumerTag,
        ulong deliveryTag,
        bool redelivered,
        string exchange,
        string routingKey,
        IReadOnlyBasicProperties properties,
        ReadOnlyMemory<byte> body,
        CancellationToken cancellationToken = default)
        {
            var message = Encoding.UTF8.GetString(body.ToArray());
            return Task.CompletedTask;
        }
    }
    class Program
    {
        static async Task Main(string[] args)
        {
            ConsoleCol.WriteLine("sender", ConsoleColor.Red);

            ConnectionFactory factory = new ConnectionFactory()
            {
                UserName = "mdioawae",
                Password = "NlLrFmnTMFixhtblSy6CrrchT33yZhYS",
                HostName = "goose.rmq2.cloudamqp.com",
                VirtualHost = "mdioawae"
            };
            using (IConnection connection = await factory.CreateConnectionAsync())
            using (IChannel channel = await connection.CreateChannelAsync())
            {
                await channel.QueueDeclareAsync("message_queue", false, false, false, null);
                // consume response from consumer 
                string replyQueueName = channel.QueueDeclareAsync().Result.QueueName;
                AsyncEventingBasicConsumer consumer = new AsyncEventingBasicConsumer(channel);
                consumer.ReceivedAsync += async (model, ea) =>
                {
                    var body = ea.Body;
                    var message = Encoding.UTF8.GetString(body.ToArray());
                    var correlationId = ea.BasicProperties.CorrelationId;

                    //lab6
                    ConsoleCol.WriteLine($"Response: {message}", ConsoleColor.Yellow);
                };
                await channel.BasicConsumeAsync(replyQueueName, true, consumer);

                //lab1
                for (int i = 1; i <= 10; i++)
                {
                    var body = Encoding.UTF8.GetBytes($"queued message{i}");
                    BasicProperties properties = new BasicProperties();
                    properties.ReplyTo = replyQueueName;
                    var corrId = Guid.NewGuid().ToString();
                    properties.CorrelationId = corrId;

                    //lab3
                    properties.Headers = new Dictionary<string, object>();
                    properties.Headers.Add("hd1", "message number: " + i);
                    properties.Headers.Add("hd2", "contents: ");
                    await channel.BasicPublishAsync<BasicProperties>(exchange: String.Empty,
                    routingKey: "message_queue",
                    mandatory: false,
                    basicProperties: properties,
                    body: body);
                }
                ConsoleCol.WriteLine("10 messages sent. Press any key to advance and send task 7 messages...", ConsoleColor.Blue);
                Console.ReadKey();
            }

            ConsoleCol.WriteLine("sender lab7", ConsoleColor.Red);
            ConsoleCol.WriteLine("Sending task 7 messages...", ConsoleColor.Blue);
            using (IConnection connection = await factory.CreateConnectionAsync())
            using (IChannel channel = await connection.CreateChannelAsync())
            {
                await channel.ExchangeDeclareAsync("abc", "topic");

                //lab7
                for (int i = 1; i <= 10; i++)
                {
                    var body = Encoding.UTF8.GetBytes($"Message {i}, zadanie 7");

                    if (i % 3 != 0)
                    {
                        await channel.BasicPublishAsync("abc", "abc.def", body);
                    }
                    else
                    {
                        await channel.BasicPublishAsync("abc", "abc.xyz", body);
                    }
                }
                Console.ReadKey();
            }
        }
    }
}