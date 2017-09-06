// ******************
//  Ruffer LLP 2017
//  Created by jirish
//  09 2017
// ******************

using System;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using log4net;
using RabbitMQ.Client;
using Environment = ServiceConnect.Monitor.Models.Environment;

namespace ServiceConnect.Monitor
{
    public class Connection : IDisposable
    {
        private static readonly ILog Logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private IConnection _connection;

        public Environment Environment { get; private set; }

        private bool _isConnectionClosed;

        public Connection(Environment environment)
        {
            Environment = environment;
            _isConnectionClosed = true;
        }

        private void CreateConnection()
        {
            var connectionFactory = new ConnectionFactory
            {
                HostName = Environment.Server,
                Protocol = Protocols.DefaultProtocol,
                Port = AmqpTcpEndpoint.UseDefaultPort,
                RequestedHeartbeat = 30
            };
            
            if (Environment.SslEnabled)
            {
                connectionFactory.Ssl = new SslOption
                {
                    Enabled = true,
                    AcceptablePolicyErrors = SslPolicyErrors.None,
                    ServerName = Environment.Server,
                    CertPassphrase = Environment.CertPassword,
                    Certs = new X509Certificate2Collection {new X509Certificate2(Convert.FromBase64String(Environment.CertBase64), Environment.CertPassword)},
                    CertificateSelectionCallback = null,
                    CertificateValidationCallback = null
                };
                connectionFactory.Port = AmqpTcpEndpoint.DefaultAmqpSslPort;
            }

            if (!string.IsNullOrEmpty(Environment.Username))
                connectionFactory.UserName = Environment.Username;
            if (!string.IsNullOrEmpty(Environment.Password))
                connectionFactory.Password = Environment.Password;

            _connection = connectionFactory.CreateConnection();

            _isConnectionClosed = false;
        }

        public IModel CreateModel()
        {
            if (_connection == null)
                CreateConnection();

            return _connection.CreateModel();
        }

        public void Dispose()
        {
            if (_isConnectionClosed) return;
            if (_connection == null) return;
            _isConnectionClosed = true;
            _connection.Close(500);
        }
    }
}