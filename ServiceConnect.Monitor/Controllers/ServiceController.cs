//Copyright (C) 2015  Timothy Watson, Jakub Pachansky

//This program is free software; you can redistribute it and/or
//modify it under the terms of the GNU General Public License
//as published by the Free Software Foundation; either version 2
//of the License, or (at your option) any later version.

//This program is distributed in the hope that it will be useful,
//but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//GNU General Public License for more details.

//You should have received a copy of the GNU General Public License
//along with this program; if not, write to the Free Software
//Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using MongoDB.Bson;
using ServiceConnect.Monitor.Interfaces;
using ServiceConnect.Monitor.Models;

namespace ServiceConnect.Monitor.Controllers
{
    public class ServiceController : ApiController
    {
        private readonly IServiceRepository _serviceRepository;
        private readonly ITagRepository _tagRepository;
        private readonly IHeartbeatRepository _heartbeatRepository;

        public ServiceController(IServiceRepository serviceRepository, ITagRepository tagRepository, IHeartbeatRepository heartbeatRepository)
        {
            _serviceRepository = serviceRepository;
            _tagRepository = tagRepository;
            _heartbeatRepository = heartbeatRepository;
        }

        [AcceptVerbs("GET")]
        [Route("services")]
        public IList<Service> FindServices(string tags = null)
        {
            List<string> tagList = null;
            if (!string.IsNullOrEmpty(tags))
                tagList = tags.Split(',').ToList();

            var services = _serviceRepository.Find().Where(x => tagList == null || tagList.Any(y => x.Tags != null && x.Tags.Contains(y))).OrderBy(x => x.Name).ToList();
            foreach (var service in services)
                service.Status = service.LastHeartbeat < DateTime.Now.Subtract(new TimeSpan(0, 0, 35)) ? "Red" : "Green";

            return services;
        }

        [AcceptVerbs("GET")]
        [Route("service")]
        public Service FindService(string name, string location)
        {
            return _serviceRepository.Find(name, location);
        }

        [AcceptVerbs("PUT")]
        [Route("service/{id}")]
        public Service UpdateService(Service model)
        {
            var tags = _tagRepository.Find().Select(x => x.Name).ToList();
            var modelTags = model.Tags;
            if (modelTags != null)
                foreach (var modelTag in modelTags)
                    if (!tags.Contains(modelTag))
                        _tagRepository.Insert(modelTag);

            var service = _serviceRepository.Find(model.Name, model.InstanceLocation);
            service.Tags = model.Tags;

            _serviceRepository.Update(service);
            return model;
        }

        [AcceptVerbs("DELETE")]
        [Route("service/{id}")]
        public void DeleteService(string id)
        {
            var service = _serviceRepository.Get(new ObjectId(id));
            _serviceRepository.Delete(new ObjectId(id));
            _heartbeatRepository.Remove(service.Name, service.InstanceLocation);
        }

        [AcceptVerbs("GET")]
        [Route("endpoints")]
        public IList<Service> FindEndpoints(string tags = null)
        {
            List<string> tagList = null;
            if (!string.IsNullOrEmpty(tags))
                tagList = tags.Split(',').ToList();

            return _serviceRepository.Find().GroupBy(x => x.Name).Select(x => new Service
            {
                In = x.First().In,
                Out = x.First().Out,
                LastHeartbeat = x.OrderBy(y => y.LastHeartbeat).First().LastHeartbeat,
                Status = GetStatus(x.ToList()),
                Name = x.First().Name,
                InstanceLocation = string.Join(", ", x.Select(y => y.InstanceLocation)),
                ConsumerType = x.First().ConsumerType,
                Language = x.First().Language,
                Tags = x.Where(y => y.Tags != null).SelectMany(y => y.Tags).Distinct().ToList()
            }).Where(x => tagList == null || tagList.Any(y => x.Tags != null && x.Tags.Contains(y))).OrderBy(x => x.Name).ToList();
        }

        private string GetStatus(List<Service> services)
        {
            if (services.All(x => x.LastHeartbeat < DateTime.Now.Subtract(new TimeSpan(0, 0, 35))))
                return "Red";

            if (services.Any(x => x.LastHeartbeat < DateTime.Now.Subtract(new TimeSpan(0, 0, 35))))
                return "Yellow";

            return "Green";
        }
    }
}
