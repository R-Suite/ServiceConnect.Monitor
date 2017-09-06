using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using log4net;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace ServiceConnect.Monitor
{
    public delegate Task ConsumerEventHandler(string message, IDictionary<string, string> headers, string host);

    public class Consumer : IDisposable
    {
        private static readonly ILog Logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly Connection _connection;

        private IModel _model;
        private AsyncEventingBasicConsumer _consumer;

        private ConsumerEventHandler _consumerEventHandler;

        private string _queueName;
        private string _forwardQueue;

        private bool _isDisposed;

        public Consumer(Connection connection)
        {
            _connection = connection;
        }

        /// <summary>
        /// Event fired on HandleBasicDeliver
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private async Task ConsumerOnReceived(object sender, BasicDeliverEventArgs args)
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

            await _consumerEventHandler(message, headers, _connection.Environment.Server);

            if (!string.IsNullOrEmpty(_forwardQueue))
                if (!_isDisposed)
                    _model.BasicPublish(string.Empty, _forwardQueue, args.BasicProperties, args.Body);

            if (!_isDisposed)
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

            _model.BasicQos(0, 50, false);

            var queueName = ConfigureQueue(_queueName);

            if (_forwardQueue != null)
                ConfigureQueue(_forwardQueue);

            _consumer = new AsyncEventingBasicConsumer(_model);
            _consumer.Received += ConsumerOnReceived;
            _model.BasicConsume(queueName, false, _consumer);
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
            _consumer.Received -= ConsumerOnReceived;
            _isDisposed = true;
            if (_model != null)
                _model.Abort();
        }
    }
}