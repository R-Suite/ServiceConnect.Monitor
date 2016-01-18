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
        public static IDisposable WebAppStart { get; set; }

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

        private static void Start(string[] args)
        {
            ObjectFactory.Initialize(x =>
            {
                x.For<IAuditRepository>().Use<AuditRepository>().Ctor<string>("mongoConnectionString").Is(System.Configuration.ConfigurationManager.AppSettings["ConnectionString"]);
                x.For<IErrorRepository>().Use<ErrorRepository>().Ctor<string>("mongoConnectionString").Is(System.Configuration.ConfigurationManager.AppSettings["ConnectionString"]);
                x.For<IHeartbeatRepository>().Use<HeartbeatRepository>().Ctor<string>("mongoConnectionString").Is(System.Configuration.ConfigurationManager.AppSettings["ConnectionString"]);
                x.For<IServiceRepository>().Use<ServiceRepository>().Ctor<string>("mongoConnectionString").Is(System.Configuration.ConfigurationManager.AppSettings["ConnectionString"]);
                x.For<IServiceMessageRepository>().Use<ServiceMessageRepository>().Ctor<string>("mongoConnectionString").Is(System.Configuration.ConfigurationManager.AppSettings["ConnectionString"]);
                x.For<IHeartbeatRepository>().Use<HeartbeatRepository>().Ctor<string>("mongoConnectionString").Is(System.Configuration.ConfigurationManager.AppSettings["ConnectionString"]);
                x.For<ITagRepository>().Use<TagRepository>().Ctor<string>("mongoConnectionString").Is(System.Configuration.ConfigurationManager.AppSettings["ConnectionString"]);
                x.For<ISettingsRepository>().Use<SettingsRepository>().Ctor<string>("mongoConnectionString").Is(System.Configuration.ConfigurationManager.AppSettings["ConnectionString"]);
                x.Scan(scan =>
                {
                    scan.TheCallingAssembly();
                    scan.WithDefaultConventions();
                });
            });

            WebAppStart = WebApp.Start<AuditConfig>("http://*:" + System.Configuration.ConfigurationManager.AppSettings["Port"]);

            var auditHub = GlobalHost.ConnectionManager.GetHubContext<AuditHub>();
            var errorHub = GlobalHost.ConnectionManager.GetHubContext<ErrorHub>();
            var heartbeatHub = GlobalHost.ConnectionManager.GetHubContext<HeartbeatHub>();

            var settingsRepository  = ObjectFactory.GetInstance<ISettingsRepository>();
            var settings = settingsRepository.Get();
            if (settings == null)
            {
                settings = new Settings
                {
                    Id = Guid.NewGuid(),
                    Environments = new List<Models.Environment>(),
                    KeepAuditsFor = "7",
                    KeepErrorsFor = "Forever",
                    KeepHeartbeatsFor = "7"
                };
                settingsRepository.Update(settings);
            }

            Globals.Settings = settings;
            
            foreach (Models.Environment environment in settings.Environments)
            {
                var consumerEnvironment = new ConsumerEnvironment
                {
                    Server = environment.Server,
                    AuditMessageHandler = new AuditMessageHandler(ObjectFactory.GetInstance<IAuditRepository>(), auditHub),
                    ErrorMessageHandler = new ErrorMessageHandler(ObjectFactory.GetInstance<IErrorRepository>(), errorHub),
                    HeartbeatMessageHandler = new HearbeatMessageHandler(ObjectFactory.GetInstance<IHeartbeatRepository>(), heartbeatHub),
                    AuditConsumer = new Consumer(environment.Server, environment.Username, environment.Password),
                    ErrorConsumer = new Consumer(environment.Server, environment.Username, environment.Password),
                    HeartbeatConsumer = new Consumer(environment.Server, environment.Username, environment.Password),
                    Producer = new Producer(environment.Server, environment.Username, environment.Password)
                };
                string forwardErrorQueue = null;
                string forwardAuditQueue = null;
                string forwardHeartbeatQueue = null;
               
                if (settings.ForwardAudit)
                {
                    forwardAuditQueue = settings.ForwardAuditQueue;
                }
                if (settings.ForwardErrors)
                {
                    forwardErrorQueue = settings.ForwardErrorQueue;
                }
                if (settings.ForwardHeartbeats)
                {
                    forwardHeartbeatQueue = settings.ForwardHeartbeatQueue;
                }

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

            var auditRepository = ObjectFactory.GetInstance<IAuditRepository>();
            auditRepository.EnsureIndex();
            var errorRepository = ObjectFactory.GetInstance<IErrorRepository>();
            errorRepository.EnsureIndex();
            var heartbeatRepository = ObjectFactory.GetInstance<IHeartbeatRepository>();
            heartbeatRepository.EnsureIndex();
            var serviceMessageRepository = ObjectFactory.GetInstance<IServiceMessageRepository>();
            serviceMessageRepository.EnsureIndex();
            var serviceRepository = ObjectFactory.GetInstance<IServiceRepository>();
            serviceRepository.EnsureIndex();
        }

        private static void AuditCallback(object state)
        {
            var period = Globals.AuditExpiry;
            if (!string.IsNullOrEmpty(period) && period != "Forever")
            {
                var repository = ObjectFactory.GetInstance<IAuditRepository>();
                repository.Remove(DateTime.Now.AddDays(Convert.ToInt32(period) * -1));
            }
        }

        private static void ErrorCallback(object state)
        {
            var period = Globals.ErrorExpiry;
            if (!string.IsNullOrEmpty(period) && period != "Forever")
            {
                var repository = ObjectFactory.GetInstance<IErrorRepository>();
                repository.Remove(DateTime.Now.AddDays(Convert.ToInt32(period) * -1));
            }
        }

        private static void HeartbeatCallback(object state)
        {
            var period = Globals.HeartbeatExpiry;
            if (!string.IsNullOrEmpty(period) && period != "Forever")
            {
                var repository = ObjectFactory.GetInstance<IHeartbeatRepository>();
                repository.Remove(DateTime.Now.AddDays(Convert.ToInt32(period) * -1));
            }
        }

        private static void Stop()
        {
            foreach (ConsumerEnvironment environment in Globals.Environments)
            {
                environment.AuditMessageHandler.Dispose();
                environment.ErrorMessageHandler.Dispose();
                environment.HeartbeatMessageHandler.Dispose();
                environment.AuditConsumer.Dispose();
                environment.ErrorConsumer.Dispose();
                environment.HeartbeatConsumer.Dispose();
            }

            foreach (var timer in Globals.Timers)
            {
                timer.Value.Dispose();
            }

            WebAppStart.Dispose();
        }
    }
}