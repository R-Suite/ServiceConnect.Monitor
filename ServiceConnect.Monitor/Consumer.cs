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
        private readonly string _host;
        private readonly string _username;
        private readonly string _password;
        private static readonly ILog Logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private IConnection _connection;
        private IModel _model;
        private ConsumerEventHandler _consumerEventHandler;
        private string _queueName;
        private bool _connectionClosed;
        private string _forwardQueue;

        public Consumer(string host, string username, string password)
        {
            _host = host;
            _username = username;
            _password = password;
        }

        /// <summary>
        /// Event fired on HandleBasicDeliver
        /// </summary>
        /// <param name="consumer"></param>
        /// <param name="args"></param>
        public void Event(IBasicConsumer consumer, BasicDeliverEventArgs args)
        {
            var headers = new Dictionary<string, string>();
            foreach (KeyValuePair<string, object> header in args.BasicProperties.Headers)
            {
                try
                {
                    headers[header.Key] = Encoding.UTF8.GetString((byte[]) header.Value);
                }
                catch{}
            }

            string message = Encoding.UTF8.GetString(args.Body);

            _consumerEventHandler(message, headers, _host);

            if (!string.IsNullOrEmpty(_forwardQueue))
            {
                _model.BasicPublish(string.Empty, _forwardQueue, args.BasicProperties, args.Body);
            }

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

            var connectionFactory = new ConnectionFactory
            {
                HostName = _host,
                Protocol = Protocols.DefaultProtocol,
                Port = AmqpTcpEndpoint.UseDefaultPort,
                RequestedHeartbeat = 30
            };

            if (!string.IsNullOrEmpty(_username))
            {
                connectionFactory.UserName = _username;
            }

            if (!string.IsNullOrEmpty(_password))
            {
                connectionFactory.Password = _password;
            }

            _connection = connectionFactory.CreateConnection();

            _model = _connection.CreateModel();

            var queueName = ConfigureQueue(_queueName);

            if (_forwardQueue != null)
            {
                ConfigureQueue(_forwardQueue);
            }

            var consumer = new EventingBasicConsumer(_model);
            consumer.Received += Event;
            consumer.Shutdown += ConsumerShutdown;
            _model.BasicConsume(queueName, false, consumer);
        }

        public void SetForwardQueue(string queue)
        {
            _forwardQueue = queue;
            if (_forwardQueue != null)
            {
                ConfigureQueue(_forwardQueue);
            }
        }

        private void ConsumerShutdown(object sender, ShutdownEventArgs e)
        {
            if (_connectionClosed)
            {
                return;
            }

            Retry.Do(CreateConsumer, ex => Logger.Error("Error connecting to queue - {0}", ex), new TimeSpan(0, 0, 0, 10));
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

        public void StopConsuming()
        {
            Dispose();
        }

        public void Dispose()
        {
            if (_connection != null)
            {
                _connectionClosed = true;
                _connection.Close(500);
            }
            if (_model != null)
                _model.Abort();
        }
    }
}