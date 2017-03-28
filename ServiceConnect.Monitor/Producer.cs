using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Security;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Newtonsoft.Json;
using RabbitMQ.Client;
using Environment = ServiceConnect.Monitor.Models.Environment;

namespace ServiceConnect.Monitor
{
    public class Producer
    {
        private IModel _model;
        private IConnection _connection;
        private readonly Object _lock = new Object();
        private readonly ConnectionFactory _connectionFactory;

        public Producer(Environment environment)
        {           
            _connectionFactory = new ConnectionFactory
            {
                HostName = environment.Server,
                VirtualHost = "/",
                Protocol = Protocols.DefaultProtocol,
                Port = AmqpTcpEndpoint.UseDefaultPort
            };

            if (environment.SslEnabled)
            {
                _connectionFactory.Ssl = new SslOption
                {
                    Enabled = true,
                    AcceptablePolicyErrors = SslPolicyErrors.None,
                    ServerName = environment.Server,
                    CertPassphrase = environment.CertPassword,
                    Certs = new X509Certificate2Collection { new X509Certificate2(Convert.FromBase64String(environment.CertBase64), environment.CertPassword) },
                    CertificateSelectionCallback = null,
                    CertificateValidationCallback = null
                };
                _connectionFactory.Port = AmqpTcpEndpoint.DefaultAmqpSslPort;
            }

            if (!string.IsNullOrEmpty(environment.Username))
                _connectionFactory.UserName = environment.Username;

            if (!string.IsNullOrEmpty(environment.Password))
                _connectionFactory.Password = environment.Password;

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
                var basicProperties = _model.CreateBasicProperties();
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