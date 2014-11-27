using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using R.MessageBus.Monitor.Interfaces;
using R.MessageBus.Monitor.Models;

namespace R.MessageBus.Monitor.Controllers
{
    public class ServiceController : ApiController
    {
        private readonly IServiceRepository _serviceRepository;
        private readonly ITagRepository _tagRepository;

        public ServiceController(IServiceRepository serviceRepository, ITagRepository tagRepository)
        {
            _serviceRepository = serviceRepository;
            _tagRepository = tagRepository;
        }

        [AcceptVerbs("GET")]
        [Route("services")]
        public IList<Service> FindServices(List<string> tags)
        {
            var services = _serviceRepository.Find().Where(x => tags == null || tags.Any(y => x.Tags.Contains(y))).OrderBy(x => x.Name).ToList();
            foreach (Service service in services)
            {
                if (service.LastHeartbeat < DateTime.Now.Subtract(new TimeSpan(0, 0, 35)))
                {
                    service.Status = "Red";
                }
                else
                {
                    service.Status = "Green"; 
                }
            }

            return services;
        }

        [AcceptVerbs("GET")]
        [Route("service")]
        public Service FindService(string name, string location)
        {
            return _serviceRepository.Find(name, location);
        }

        [AcceptVerbs("PUT")]
        [Route("service")]
        public Service UpdateService(Service model)
        {
            var tags = _tagRepository.Find().Select(x => x.Name).ToList();
            var modelTags = model.Tags;
            if (modelTags != null)
            {
                foreach (string modelTag in modelTags)
                {
                    if (!tags.Contains(modelTag))
                    {
                        _tagRepository.Insert(modelTag);
                    }
                }
            }

            var service = _serviceRepository.Find(model.Name, model.InstanceLocation);
            service.Tags = model.Tags;

            _serviceRepository.Update(service);
            return model;
        }

        [AcceptVerbs("GET")]
        [Route("endpoints")]
        public IList<Service> FindEndpoints(List<string> tags)
        {
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
            }).Where(x => tags == null || tags.Any(y => x.Tags.Contains(y))).OrderBy(x => x.Name).ToList();
        }

        private string GetStatus(List<Service> services)
        {
            if (services.All(x => x.LastHeartbeat < DateTime.Now.Subtract(new TimeSpan(0, 0, 35))))
            {
                return "Red";
            }

            if (services.Any(x => x.LastHeartbeat < DateTime.Now.Subtract(new TimeSpan(0, 0, 35))))
            {
                return "Yellow";
            }            
            
            return "Green";
        }
    }
}
