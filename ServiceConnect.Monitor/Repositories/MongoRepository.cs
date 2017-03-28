using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using MongoDB.Driver;
using ServiceConnect.Monitor.Interfaces;

namespace ServiceConnect.Monitor.Repositories
{
    public class MongoRepository : IMongoRepository
    {
        public MongoDatabase Database { get; private set; }

        public MongoRepository(
            string mongoConnectionString,
            string mongoUsername,
            string mongoPassword,
            string certBase64,
            string databaseName)
        {
            MongoClient client;

            if (mongoConnectionString.IndexOf("mongodb:", StringComparison.OrdinalIgnoreCase) != -1)
            {
                client = new MongoClient(mongoConnectionString);
            }
            else
            {
                var mongoNodes = mongoConnectionString.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                var settings = new MongoClientSettings
                {
                    UseSsl = true,
                    Credentials = new List<MongoCredential>
                    {
                        MongoCredential.CreateCredential("admin", mongoUsername, mongoPassword)
                    },
                    ConnectionMode = ConnectionMode.Automatic,
                    WriteConcern = new WriteConcern { W = WriteConcern.WMajority.W, WTimeout = TimeSpan.FromSeconds(20) },
                    Servers = mongoNodes.Select(x => new MongoServerAddress(x)),
                    SslSettings = new SslSettings
                    {
                        ClientCertificates = new List<X509Certificate>
                        {
                            new X509Certificate2(Convert.FromBase64String(certBase64))
                        },
                        ClientCertificateSelectionCallback = (sender, host, certificates, certificate, issuers) => certificates[0],
                        CheckCertificateRevocation = false
                    }
                };

                client = new MongoClient(settings);
            }

            var server = client.GetServer();
            Database = server.GetDatabase(databaseName);
        }
    }
}