using System.Collections.Generic;
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
        [Route("services")]
        public IList<Service> FindServices()
        {
            return _serviceRepository.Find();
        }
    }
}
