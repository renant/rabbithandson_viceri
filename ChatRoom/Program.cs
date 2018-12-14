using System;
using System.Text;
using System.Threading;
using EasyNetQ;
using EasyNetQ.Topology;
using Newtonsoft.Json;

namespace ChatRoom
{
    class Program
    {
        private static Guid _userId = Guid.NewGuid();
        private static string _myQueueName;

        static void Main(string[] args)
        {
            _myQueueName = $"messages_{_userId}";

            using (var bus = RabbitHutch.CreateBus("amqp://llyajxqz:D9Za_TFzDeYpWVsMXTd2yIH3rdhaGRlk@buffalo.rmq.cloudamqp.com/llyajxqz").Advanced)
            {
                var exchange = bus.ExchangeDeclare("my_exchange_fanout", ExchangeType.Fanout);
                var myQueueMessages = bus.QueueDeclare(_myQueueName);
                bus.Bind(exchange, myQueueMessages, "");

                new Thread(() =>
                {

                    bus.Consume(myQueueMessages, (body, properties, info) =>
                    {
                        var response = Encoding.UTF8.GetString(body);
                        var message = JsonConvert.DeserializeObject<Message>(response);
                        //print message
                    });

                }).Start();
            }
        }
    }
}
