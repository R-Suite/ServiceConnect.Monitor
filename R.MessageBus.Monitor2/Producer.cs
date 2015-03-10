using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Newtonsoft.Json;
using RabbitMQ.Client;

namespace R.MessageBus.Monitor
{
    public class Producer
    {
        private IModel _model;
        private IConnection _connection;
        private readonly Object _lock = new Object();
        private readonly ConnectionFactory _connectionFactory;
        private readonly string _host;

        public Producer(string host, string username, string password)
        {           
            _host = host;            

            _connectionFactory = new ConnectionFactory
            {
                HostName = _host,
                VirtualHost = "/",
                Protocol = Protocols.FromEnvironment(),
                Port = AmqpTcpEndpoint.UseDefaultPort
            };

            if (!string.IsNullOrEmpty(username))
            {
                _connectionFactory.UserName = username;
            }

            if (!string.IsNullOrEmpty(password))
            {
                _connectionFactory.Password = password;
            }

            CreateConnection();
        }

        private void CreateConnection()
        {
            _connection = _connectionFactory.CreateConnection();
            _model = _connection.CreateModel();
        }

        public void Send(string endPoint, string message, IDictionary<string, string> headers)
        {
            var bytes = Encoding.UTF8.GetBytes(message);

            lock (_lock)
            {
                IBasicProperties basicProperties = _model.CreateBasicProperties();
                basicProperties.SetPersistent(true);

                basicProperties.Headers = headers.ToDictionary(x => x.Key, x => (object)x.Value);
                basicProperties.MessageId = basicProperties.Headers["MessageId"].ToString(); // keep track of retries

                _model.BasicPublish(string.Empty, endPoint, basicProperties, bytes);
            }
        }        

        public void Dispose()
        {            
            if (_connection != null)
            {
                _connection.Close();
                _connection.Dispose();
            }
            if (_model != null)
            {
                _model.Abort();
                _model.Dispose();
            }
        }
    }
}