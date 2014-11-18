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

        public ServiceController(IServiceRepository serviceRepository)
        {
            _serviceRepository = serviceRepository;
        }

        [AcceptVerbs("GET")]
        [Route("services/{name}")]
        public IList<Service> FindServices(string name)
        {
            return _serviceRepository.FindByName(name);
        }

        [AcceptVerbs("GET")]
        [Route("endpoints")]
        public IList<Service> FindEndpoints()
        {
            return _serviceRepository.Find().GroupBy(x => x.Name).Select(x => new Service
            {
                In = x.First().In,
                Out = x.First().Out,
                LastHeartbeat = x.OrderBy(y => y.LastHeartbeat).First().LastHeartbeat,
                Status = GetStatus(x.ToList()),
                Name = x.First().Name
            }).OrderBy(x => x.Name).ToList();
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
