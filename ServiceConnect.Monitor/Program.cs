using System;
using System.Collections.Generic;
using System.ServiceProcess;
using System.Threading;
using Microsoft.AspNet.SignalR;
using Microsoft.Owin.Hosting;
using ServiceConnect.Monitor.Handlers;
using ServiceConnect.Monitor.Hubs;
using ServiceConnect.Monitor.Interfaces;
using ServiceConnect.Monitor.Models;
using ServiceConnect.Monitor.Repositories;
using StructureMap;
using StructureMap.Graph;
using Environment = System.Environment;

namespace ServiceConnect.Monitor
{
    class Program
    {
        public class Service : ServiceBase
        {
            public Service()
            {
                ServiceName = ServiceName;
            }

            protected override void OnStart(string[] args)
            {
                Program.Start(args);
            }

            protected override void OnStop()
            {
                Program.Stop();
            }
        }

        static void Main(string[] args)
        {
            if (!Environment.UserInteractive)
                using (var service = new Service())
                    ServiceBase.Run(service);
            else
            {
                Start(args);

                Console.WriteLine();
                Console.WriteLine("Press any key to stop...");
                Console.ReadKey(true);

                Stop();
            }
        }

        private static IContainer _container;
        private static IDisposable _webApp;

        private static void Start(string[] args)
        {
            _container = new Container(x =>
            {
                x.For<IMongoRepository>().Singleton().Use<MongoRepository>()
                    .Ctor<string>("mongoConnectionString").Is(System.Configuration.ConfigurationManager.AppSettings["MongoDBConnectionString"])
                    .Ctor<string>("mongoUsername").Is(System.Configuration.ConfigurationManager.AppSettings["MongoDBUsername"])
                    .Ctor<string>("mongoPassword").Is(System.Configuration.ConfigurationManager.AppSettings["MongoDBPassword"])
                    .Ctor<string>("certBase64").Is(System.Configuration.ConfigurationManager.AppSettings["MongoCertBase64"])
                    .Ctor<string>("databaseName").Is(System.Configuration.ConfigurationManager.AppSettings["PersistanceDatabaseName"]);

                x.For<IAuditRepository>().Use<AuditRepository>()
                    .Ctor<string>("auditCollecitonName").Is(System.Configuration.ConfigurationManager.AppSettings["PersistanceCollectionNameAudit"])
                    .Ctor<string>("serviceMessagesCollectionName").Is(System.Configuration.ConfigurationManager.AppSettings["ServiceMessagesCollectionName"]);

                x.For<IErrorRepository>().Use<ErrorRepository>()
                    .Ctor<string>("errorCollectionName").Is(System.Configuration.ConfigurationManager.AppSettings["PersistanceCollectionNameError"]);

                x.For<IHeartbeatRepository>().Use<HeartbeatRepository>()
                    .Ctor<string>("heartbeatCollectionName").Is(System.Configuration.ConfigurationManager.AppSettings["PersistanceCollectionNameHeartbeat"])
                    .Ctor<string>("servicesCollectionName").Is(System.Configuration.ConfigurationManager.AppSettings["ServiceDetailsCollectionName"]);

                x.For<IServiceRepository>().Use<ServiceRepository>()
                    .Ctor<string>("servicesCollectionName").Is(System.Configuration.ConfigurationManager.AppSettings["ServiceDetailsCollectionName"]);

                x.For<IServiceMessageRepository>().Use<ServiceMessageRepository>()
                    .Ctor<string>("serviceMessagesCollectionName").Is(System.Configuration.ConfigurationManager.AppSettings["ServiceMessagesCollectionName"]);

                x.For<IHeartbeatRepository>().Use<HeartbeatRepository>()
                    .Ctor<string>("heartbeatCollectionName").Is(System.Configuration.ConfigurationManager.AppSettings["PersistanceCollectionNameHeartbeat"])
                    .Ctor<string>("serviceCollectionName").Is(System.Configuration.ConfigurationManager.AppSettings["ServiceDetailsCollectionName"]);

                x.For<ITagRepository>().Use<TagRepository>()
                    .Ctor<string>("tagCollectionName").Is(System.Configuration.ConfigurationManager.AppSettings["TagsCollectionName"]);

                x.For<ISettingsRepository>().Use<SettingsRepository>()
                    .Ctor<string>("settingsCollectionName").Is(System.Configuration.ConfigurationManager.AppSettings["SettingsCollectionName"]);

                x.Scan(scan =>
                {
                    scan.TheCallingAssembly();
                    scan.WithDefaultConventions();
                });
            });

            var webAppConfig = new AuditConfig(_container);

            _webApp = WebApp.Start("http://*:" + System.Configuration.ConfigurationManager.AppSettings["Port"], app => webAppConfig.Configuration(app));

            var auditHub = GlobalHost.ConnectionManager.GetHubContext<AuditHub>();
            var errorHub = GlobalHost.ConnectionManager.GetHubContext<ErrorHub>();
            var heartbeatHub = GlobalHost.ConnectionManager.GetHubContext<HeartbeatHub>();

            var settingsRepository  = _container.GetInstance<ISettingsRepository>();
            var settings = settingsRepository.Get().GetAwaiter().GetResult();
            if (settings == null)
            {
                settings = new Settings
                {
                    Id = Guid.NewGuid(),
                    Environments = new List<Models.Environment>(),
                    KeepAuditsFor = "7",
                    KeepErrorsFor = "150",
                    KeepHeartbeatsFor = "7"
                };
                settingsRepository.Update(settings);
            }

            Globals.Settings = settings;
            
            foreach (var environment in settings.Environments)
            {
                // Two distinct connections one for receiving and one for sending that way publishing isn't delayed if the socket is busy pulling messages
                var receivingConnection = new Connection(environment);
                var sendingConnection = new Connection(environment);

                var consumerEnvironment = new ConsumerEnvironment
                {
                    Server = environment.Server,
                    AuditMessageHandler = new AuditMessageHandler(_container.GetInstance<IAuditRepository>(), auditHub),
                    ErrorMessageHandler = new ErrorMessageHandler(_container.GetInstance<IErrorRepository>(), errorHub),
                    HeartbeatMessageHandler = new HearbeatMessageHandler(_container.GetInstance<IHeartbeatRepository>(), heartbeatHub),
                    ReceivingConnection = receivingConnection,
                    SendingConnection = sendingConnection,
                    AuditConsumer = new ConsumerPool(receivingConnection),
                    ErrorConsumer = new ConsumerPool(receivingConnection),
                    HeartbeatConsumer = new ConsumerPool(receivingConnection),
                    Producer = new Producer(sendingConnection),
                    VirtualHost = environment.VirtualHost
                };
                string forwardErrorQueue = null;
                string forwardAuditQueue = null;
                string forwardHeartbeatQueue = null;

                if (settings.ForwardAudit)
                    forwardAuditQueue = settings.ForwardAuditQueue;

                if (settings.ForwardErrors)
                    forwardErrorQueue = settings.ForwardErrorQueue;

                if (settings.ForwardHeartbeats)
                    forwardHeartbeatQueue = settings.ForwardHeartbeatQueue;

                consumerEnvironment.AuditConsumer.StartConsuming(consumerEnvironment.AuditMessageHandler.Execute, environment.AuditQueue, forwardAuditQueue);
                consumerEnvironment.ErrorConsumer.StartConsuming(consumerEnvironment.ErrorMessageHandler.Execute, environment.ErrorQueue, forwardErrorQueue);
                consumerEnvironment.HeartbeatConsumer.StartConsuming(consumerEnvironment.HeartbeatMessageHandler.Execute, environment.HeartbeatQueue, forwardHeartbeatQueue);

                Globals.Environments.Add(consumerEnvironment);
            }

            Globals.AuditExpiry = settings.KeepAuditsFor;
            var timer = new Timer(AuditCallback, settings, 0, 3600000);
            Globals.Timers["Audit"] = timer;
            Globals.ErrorExpiry = settings.KeepErrorsFor;
            timer = new Timer(ErrorCallback, settings, 0, 3600000);
            Globals.Timers["Error"] = timer;
            Globals.HeartbeatExpiry = settings.KeepHeartbeatsFor;
            timer = new Timer(HeartbeatCallback, settings, 0, 3600000);
            Globals.Timers["Heartbeat"] = timer;

            _container.GetInstance<IAuditRepository>().EnsureIndex();
            _container.GetInstance<IErrorRepository>().EnsureIndex();
            _container.GetInstance<IHeartbeatRepository>().EnsureIndex();
            _container.GetInstance<IServiceMessageRepository>().EnsureIndex();
            _container.GetInstance<IServiceRepository>().EnsureIndex();
        }

        private static void AuditCallback(object state)
        {
            var period = Globals.AuditExpiry;
            if (!string.IsNullOrEmpty(period) && period != "Forever")
                _container.GetInstance<IAuditRepository>().Remove(DateTime.Now.AddDays(Convert.ToInt32(period) * -1));
        }

        private static void ErrorCallback(object state)
        {
            var period = Globals.ErrorExpiry;
            if (!string.IsNullOrEmpty(period) && period != "Forever")
                _container.GetInstance<IErrorRepository>().Remove(DateTime.Now.AddDays(Convert.ToInt32(period) * -1));
        }

        private static void HeartbeatCallback(object state)
        {
            var period = Globals.HeartbeatExpiry;
            if (!string.IsNullOrEmpty(period) && period != "Forever")
                _container.GetInstance<IHeartbeatRepository>().Remove(DateTime.Now.AddDays(Convert.ToInt32(period) * -1));
        }

        private static void Stop()
        {
            foreach (var environment in Globals.Environments)
            {
                environment.AuditMessageHandler.Dispose();
                environment.ErrorMessageHandler.Dispose();
                environment.HeartbeatMessageHandler.Dispose();
                environment.AuditConsumer.Dispose();
                environment.ErrorConsumer.Dispose();
                environment.HeartbeatConsumer.Dispose();
                environment.Producer.Dispose();
                environment.ReceivingConnection.Dispose();
                environment.SendingConnection.Dispose();
            }

            foreach (var timer in Globals.Timers)
                timer.Value.Dispose();

            _webApp.Dispose();

            _container.Dispose();
        }
    }
}