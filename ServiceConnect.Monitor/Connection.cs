// ******************
//  Ruffer LLP 2017
//  Created by jirish
//  09 2017
// ******************

using System;
using System.Linq;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using log4net;
using Microsoft.Owin.Security;
using RabbitMQ.Client;
using Environment = ServiceConnect.Monitor.Models.Environment;

namespace ServiceConnect.Monitor
{
    public class Connection : IDisposable
    {
        private static readonly ILog Logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public Environment Environment { get; private set; }

        private IConnection _connection;

        public Connection(Environment environment)
        {
            Environment = environment;
        }

        private void CreateConnection()
        {
            var connectionFactory = new ConnectionFactory
            {
                HostName = Environment.Server,
                Protocol = Protocols.DefaultProtocol,
                Port = AmqpTcpEndpoint.UseDefaultPort,
                RequestedHeartbeat = 30,
                DispatchConsumersAsync = true,
                UseBackgroundThreadsForIO = true,
                AutomaticRecoveryEnabled = true,
                TopologyRecoveryEnabled = true
            };

            var hostnames = Environment.Server.Split(',', ';');

            if (Environment.SslEnabled)
            {
                connectionFactory.Ssl = new SslOption
                {
                    Enabled = true,
                    AcceptablePolicyErrors = SslPolicyErrors.None,
                    ServerName = hostnames.First(),
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

            _connection = connectionFactory.CreateConnection(hostnames, "ServiceConnect.Monitor");
        }

        public void Connect()
        {
            if (_connection == null)
                CreateConnection();
        }

        public IModel CreateModel()
        {
            if (_connection == null)
                CreateConnection();

            return _connection.CreateModel();
        }

        public void Dispose()
        {
            if (_connection == null) return;
            _connection.Abort(500);
            _connection = null;
        }
    }
}