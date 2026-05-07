using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using RabbitMQ.Client;
using ExtensionClass;


namespace receiver1
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
            ConsoleCol.WriteLine("receiver1", ConsoleColor.Red);

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
                //lab5
                await channel.BasicQosAsync(0, 1, false);
                await channel.BasicConsumeAsync("message_queue", false, consumer);
                ConsoleCol.WriteLine("Press any key to advance to task7...", ConsoleColor.Blue);
                Console.ReadKey();
            }

            ConsoleCol.WriteLine("receiver1 task7", ConsoleColor.Red);
            using (IConnection connection = await factory.CreateConnectionAsync())
            using (IChannel channel = await connection.CreateChannelAsync())
            {
                AsyncDefaultBasicConsumer consumer = new MyConsumer(channel);
                await channel.ExchangeDeclareAsync("abc", "topic");
                var queue = channel.QueueDeclareAsync().Result.QueueName;
                //lab7
                await channel.QueueBindAsync(queue, "abc", "abc.#");
                await channel.BasicConsumeAsync(queue, false, consumer);
                Console.ReadKey();
            }
        }
    }
}