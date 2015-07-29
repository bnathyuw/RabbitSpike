using System;
using System.Text;
using System.Threading;
using System.Web.Script.Serialization;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace RabbitSpike.Subscriber
{
    internal interface IHandler
    {
        void Handle(Message message);
    }

    internal class Runner : IDisposable
    {
        private const string ExchangeName = "x";
        private readonly ConnectionFactory _connectionFactory;
        private readonly JavaScriptSerializer _javaScriptSerializer;
        private readonly IConnection _connection;
        private readonly IModel _model;
        private readonly QueueingBasicConsumer _consumer;
        private readonly string _labelName;
        private readonly IHandler _handler;
        private Thread _thread;
        private bool _keepGoing = true;

        public Runner(string labelName, IHandler handler)
        {
            _labelName = labelName;
            _handler = handler;
            _connectionFactory = new ConnectionFactory {HostName = "localhost", VirtualHost = "spike"};
            _javaScriptSerializer = new JavaScriptSerializer();
            _connection = _connectionFactory.CreateConnection();
            _model = _connection.CreateModel();
            _model.ExchangeDeclare(ExchangeName, "direct", true);
            _model.QueueDeclare(QueueName(), true, false, false, null);
            _model.QueueBind(QueueName(), ExchangeName, String.Empty);

            _consumer = new QueueingBasicConsumer(_model);
        }

        public void Run()
        {
            _model.BasicConsume(QueueName(), false, _consumer);
            _thread = new Thread(Consume);
            _thread.Start();
            while (!_thread.IsAlive)
            {
            }
        }

        private void Consume()
        {
            while (_keepGoing)
            {
                BasicDeliverEventArgs basicDeliverEventArgs;
                if (!_consumer.Queue.Dequeue(100, out basicDeliverEventArgs)) continue;

                var contents = Encoding.UTF8.GetString(basicDeliverEventArgs.Body);

                var message = _javaScriptSerializer.Deserialize<Message>(contents);

                _handler.Handle(message);

                _model.BasicAck(basicDeliverEventArgs.DeliveryTag, false);
            }
        }

        private string QueueName()
        {
            return "q_" + _labelName;
        }

        public void Dispose()
        {
            _keepGoing = false;
            while (_thread.ThreadState != ThreadState.Stopped)
            {
            }
            _thread.Interrupt();
            _model.Dispose();
            _connection.Dispose();
        }
    }
}