// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IoC.cs" company="Web Advanced">
// Copyright 2012 Web Advanced (www.webadvanced.com)
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0

// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


using System.Web.Configuration;
using R.MessageBus.Monitor.Interfaces;
using R.MessageBus.Monitor.Repositories;
using StructureMap;
using StructureMap.Graph;
namespace R.MessageBus.Monitor.DependencyResolution {
    public static class IoC {
        public static IContainer Initialize() {
            ObjectFactory.Initialize(x =>
            {
                x.For<IAuditRepository>().Use<AuditRepository>().Ctor<string>("mongoConnectionString").Is(WebConfigurationManager.AppSettings["MongoConnectionString"]);
                x.For<IErrorRepository>().Use<ErrorRepository>().Ctor<string>("mongoConnectionString").Is(WebConfigurationManager.AppSettings["MongoConnectionString"]);
                x.For<IHeartbeatRepository>().Use<HeartbeatRepository>().Ctor<string>("mongoConnectionString").Is(WebConfigurationManager.AppSettings["MongoConnectionString"]);
                x.For<IServiceRepository>().Use<ServiceRepository>().Ctor<string>("mongoConnectionString").Is(WebConfigurationManager.AppSettings["MongoConnectionString"]);
                x.For<IServiceMessageRepository>().Use<ServiceMessageRepository>().Ctor<string>("mongoConnectionString").Is(WebConfigurationManager.AppSettings["MongoConnectionString"]);
                x.For<IHeartbeatRepository>().Use<HeartbeatRepository>().Ctor<string>("mongoConnectionString").Is(WebConfigurationManager.AppSettings["MongoConnectionString"]);
                x.For<ITagRepository>().Use<TagRepository>().Ctor<string>("mongoConnectionString").Is(WebConfigurationManager.AppSettings["MongoConnectionString"]);
                x.For<ISettingsRepository>().Use<SettingsRepository>().Ctor<string>("mongoConnectionString").Is(WebConfigurationManager.AppSettings["MongoConnectionString"]);
                x.Scan(scan =>
                {
                    scan.TheCallingAssembly();
                    scan.WithDefaultConventions();
                });
            });
            return ObjectFactory.Container;
        }
    }
}