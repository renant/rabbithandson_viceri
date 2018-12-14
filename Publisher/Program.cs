using System;
using System.Text;
using EasyNetQ;
using EasyNetQ.Topology;

namespace Publisher
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var bus = RabbitHutch.CreateBus("amqp://llyajxqz:D9Za_TFzDeYpWVsMXTd2yIH3rdhaGRlk@buffalo.rmq.cloudamqp.com/llyajxqz").Advanced)
            {
                var exchange = bus.ExchangeDeclare("my_direct_exchange", ExchangeType.Direct);
                var queue = bus.QueueDeclare("first_queue");
                bus.Bind(exchange, queue, "first_queue");

                while(true)
                {
                    Console.WriteLine("Send your message to consumers...");
                    var msg = Console.ReadLine();
                    var body = Encoding.UTF8.GetBytes(msg);
                    bus.Publish(exchange, "first_queue", false, new MessageProperties(), body);
                }
            }
        }
    }
}
