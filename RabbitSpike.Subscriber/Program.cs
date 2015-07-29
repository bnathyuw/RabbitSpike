using System;
using System.Text;
using System.Web.Script.Serialization;
using RabbitMQ.Client;

namespace RabbitSpike.Subscriber
{
    static class Program
    {
        private static string _labelName;

        static void Main(string[] args)
        {
            Console.WriteLine("Please enter the label name:");
            _labelName = Console.ReadLine();

            var connectionFactory = new ConnectionFactory {HostName = "localhost", VirtualHost = "spike"};
            var javaScriptSerializer = new JavaScriptSerializer();
            using (var connection = connectionFactory.CreateConnection())
            {
                using (var model = connection.CreateModel())
                {
                    model.ExchangeDeclare("x", "direct", true);
                    model.QueueDeclare("q_" + _labelName, true, false, false, null);
                    model.QueueBind("q_" + _labelName, "x", string.Empty);

                    var consumer = new QueueingBasicConsumer(model);
                    model.BasicConsume("q_" + _labelName, false, consumer);
                    while (true)
                    {
                        var basicDeliverEventArgs = consumer.Queue.Dequeue();

                        var contents = Encoding.UTF8.GetString(basicDeliverEventArgs.Body);

                        var message = javaScriptSerializer.Deserialize<Message>(contents);

                        Handle(message);

                        model.BasicAck(basicDeliverEventArgs.DeliveryTag, false);
                    }
                }
            }
        }

        private static void Handle(Message message)
        {
            if(message.LabelName == _labelName)
                Console.WriteLine(message.Value);
        }
    }

    internal struct Message
    {
        public string LabelName { get; set; }
        public string Value { get; set; }
    }
}
