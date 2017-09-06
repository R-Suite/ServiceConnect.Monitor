using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RabbitMQ.Client;

namespace ServiceConnect.Monitor
{
    public class Producer : IDisposable
    {
        private readonly object _lock = new object();

        private readonly IModel _model;
        
        public Producer(Connection connection)
        {
            _model = connection.CreateModel();
        }

        public void Send(string endPoint, string message, IDictionary<string, string> headers)
        {
            var bytes = Encoding.UTF8.GetBytes(message);

            lock (_lock)
            {
                var basicProperties = _model.CreateBasicProperties();
                basicProperties.Persistent = true;

                basicProperties.Headers = headers.ToDictionary(x => x.Key, x => (object)x.Value);
                basicProperties.MessageId = basicProperties.Headers["MessageId"].ToString(); // keep track of retries

                _model.BasicPublish(string.Empty, endPoint, basicProperties, bytes);
            }
        }        

        public void Dispose()
        {
            lock (_lock)
                if (_model != null)
                    _model.Close();
        }
    }
}