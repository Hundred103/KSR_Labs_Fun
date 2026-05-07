using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ExtensionClass;
using RabbitMQ.Client;

namespace receiver2
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
            if (properties.Headers != null)
            {
                //lab3
                string hed1 = Encoding.UTF8.GetString((byte[])properties.Headers["hd1"]);
                string hed2 = Encoding.UTF8.GetString((byte[])properties.Headers["hd2"]);
                ConsoleCol.WriteLine($"{hed1} {hed2} {message}", ConsoleColor.White);
            }
            else
            {
                //lab7
                ConsoleCol.WriteLine(message, ConsoleColor.Magenta);
            }
            if (properties.CorrelationId != null)
            {
                //lab6
                string bodyString = Encoding.UTF8.GetString(body.Span);
                var response = bodyString + " response";
                ReadOnlyMemory<byte> responseBody = Encoding.UTF8.GetBytes(response);
                var replyProps = new BasicProperties
                {
                    CorrelationId = properties.CorrelationId
                };
                replyProps.CorrelationId = properties.CorrelationId;
                Channel.BasicPublishAsync<BasicProperties>(exchange: String.Empty, routingKey: properties.ReplyTo, mandatory: false, basicProperties: replyProps, body: responseBody);
            }
            //lab5
            System.Threading.Thread.Sleep(2000);
            Channel.BasicAckAsync(deliveryTag, false);
            return Task.CompletedTask;
        }
    }

    class Program
    {
        static async Task Main(string[] args)
        {
            ConsoleCol.WriteLine("receiver2", ConsoleColor.Red);

            var factory = new ConnectionFactory()
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
                AsyncDefaultBasicConsumer consumer = new MyConsumer(channel);
                //lab4
                //lab5
                await channel.BasicQosAsync(0, 1, false);
                await channel.BasicConsumeAsync("message_queue", false, consumer);
                ConsoleCol.WriteLine("Press any key to advance to task7...", ConsoleColor.Blue);
                Console.ReadKey();
            }

            ConsoleCol.WriteLine("receiver2 task7", ConsoleColor.Red);
            using (IConnection connection = await factory.CreateConnectionAsync())
            using (IChannel channel = await connection.CreateChannelAsync())
            {
                AsyncDefaultBasicConsumer consumer = new MyConsumer(channel);
                await channel.ExchangeDeclareAsync("abc", "topic");
                var queue = channel.QueueDeclareAsync().Result.QueueName;
                //lab7
                await channel.QueueBindAsync(queue, "abc", "#.xyz");
                await channel.BasicConsumeAsync(queue, false, consumer);
                Console.ReadKey();
            }
        }
    }
}