using System;
using System.Collections.Generic;
using System.Text;
using log4net;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace ServiceConnect.Monitor
{
    public delegate void ConsumerEventHandler(string message, IDictionary<string, string> headers, string host);

    public class Consumer : IDisposable
    {
        private static readonly ILog Logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly Connection _connection;
        private IModel _model;

        private ConsumerEventHandler _consumerEventHandler;

        private string _queueName;
        private string _forwardQueue;

        public Consumer(Connection connection)
        {
            _connection = connection;
        }

        /// <summary>
        /// Event fired on HandleBasicDeliver
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void ConsumerOnReceived(object sender, BasicDeliverEventArgs args)
        {
            var headers = new Dictionary<string, string>();
            foreach (var header in args.BasicProperties.Headers)
            {
                try
                {
                    headers[header.Key] = Encoding.UTF8.GetString((byte[]) header.Value);
                }
                catch { }
            }

            var message = Encoding.UTF8.GetString(args.Body);

            _consumerEventHandler(message, headers, _connection.Environment.Server);

            if (!string.IsNullOrEmpty(_forwardQueue))
                _model.BasicPublish(string.Empty, _forwardQueue, args.BasicProperties, args.Body);

            _model.BasicAck(args.DeliveryTag, false);
        }

        public void StartConsuming(ConsumerEventHandler messageReceived, string queueName, string forwardQueue)
        {
            _consumerEventHandler = messageReceived;
            _queueName = queueName;
            _forwardQueue = forwardQueue;

            CreateConsumer();
        }

        private void CreateConsumer()
        {
            Logger.Info(string.Format("Connecting to queue - {0}", _queueName));

            _model = _connection.CreateModel();

            var queueName = ConfigureQueue(_queueName);

            if (_forwardQueue != null)
                ConfigureQueue(_forwardQueue);

            var consumer = new EventingBasicConsumer(_model);
            consumer.Received += ConsumerOnReceived;
            _model.BasicConsume(queueName, false, consumer);
        }

        public void SetForwardQueue(string queue)
        {
            _forwardQueue = queue;
            if (_forwardQueue != null)
                ConfigureQueue(_forwardQueue);
        }

        private string ConfigureQueue(string queue)
        {
            try
            {
                _model.QueueDeclare(queue, true, false, false, null);
            }
            catch (Exception ex)
            {
                Logger.Warn(string.Format("Error declaring queue - {0}", ex.Message));
            }
            return queue;
        }

        public void Dispose()
        {
            if (_model != null)
                _model.Close();
        }
    }
}