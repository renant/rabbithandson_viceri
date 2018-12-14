using System;
using System.Linq;
using System.Text;
using System.Threading;
using EasyNetQ;
using EasyNetQ.Management.Client;
using EasyNetQ.Topology;
using Newtonsoft.Json;

namespace ChatRoom
{
    class Program
    {
        private static readonly Guid _userId = Guid.NewGuid();
        private static ManagementClient _client;
        private static string _myQueueName;

        static void Main(string[] args)
        {
            _client = new ManagementClient("buffalo.rmq.cloudamqp.com", "llyajxqz", "D9Za_TFzDeYpWVsMXTd2yIH3rdhaGRlk");
            _myQueueName = $"messages_{_userId}";

            using (var bus = RabbitHutch.CreateBus("amqp://llyajxqz:D9Za_TFzDeYpWVsMXTd2yIH3rdhaGRlk@buffalo.rmq.cloudamqp.com/llyajxqz").Advanced)
            {
                System.AppDomain.CurrentDomain.ProcessExit += (s, e) =>
                {
                    CleanUp();
                };

                Console.CancelKeyPress += (a, b) =>
                {
                    CleanUp();
                };

                var exchange = bus.ExchangeDeclare("my_exchange_fanout", ExchangeType.Fanout);
                var myQueueMessages = bus.QueueDeclare(_myQueueName);
                bus.Bind(exchange, myQueueMessages, "");

                new Thread(() =>
                {
                    bus.Consume(myQueueMessages, (body, properties, info) =>
                    {
                        var response = Encoding.UTF8.GetString(body);
                        var message = JsonConvert.DeserializeObject<Message>(response);
                        PrintMessage(message);
                    });
                }).Start();

                Console.WriteLine("Hey! Say your name to enter in chat room!");
                Console.Write("Name: ");
                var userName = Console.ReadLine();
                Console.WriteLine("Now at any time enter your message: ");

                while (true)
                {
                    var typedMessage = Console.ReadLine();
                    ClearLine();
                    var json = JsonConvert.SerializeObject(new Message(_userId, userName, typedMessage));
                    var body = Encoding.UTF8.GetBytes(json);
                    bus.Publish(exchange, "", false, new MessageProperties() { }, body);
                }
            }
        }

        private static void CleanUp()
        {
            var deleteQueue = _client.GetQueuesAsync().Result.FirstOrDefault(x => x.Name == _myQueueName);
            _client.DeleteQueueAsync(deleteQueue).Wait();
        }

        private static void ClearLine()
        {
            Console.SetCursorPosition(0, Console.CursorTop - 1);
            Console.Write(new string(' ', Console.WindowWidth));
            Console.SetCursorPosition(0, Console.CursorTop - 1);
        }

        private static void PrintMessage(Message message)
        {
            if (string.IsNullOrWhiteSpace(message.Content)) return;

            if (message.UserId == _userId)
                PrintYourMessage(message);
            else
                PrintOtherMessage(message);
        }

        private static void PrintOtherMessage(Message message)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"{message.UserName}: {message.Content}");
            Console.ResetColor();
        }

        private static void PrintYourMessage(Message message)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Você: {message.Content}");
            Console.ResetColor();
        }
    }
}
