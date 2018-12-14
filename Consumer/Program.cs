using System;
using System.Text;
using EasyNetQ;

namespace Consumer
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var bus = RabbitHutch.CreateBus("amqp://llyajxqz:D9Za_TFzDeYpWVsMXTd2yIH3rdhaGRlk@buffalo.rmq.cloudamqp.com/llyajxqz").Advanced)
            {
                var queue = bus.QueueDeclare("first_queue");

                bus.Consume(queue, (body, properties, info) => {
                    var receivedMessage = Encoding.UTF8.GetString(body);
                    Console.WriteLine($"Received Message: {receivedMessage}");
                });

                while(true)
                {
                }
            }
        }
    }
}
