using System.Collections.Generic;
using System.Web.Http;
using R.MessageBus.Monitor.Interfaces;
using R.MessageBus.Monitor.Models;

namespace R.MessageBus.Monitor.Controllers
{
    public class ServiceMessageController : ApiController
    {
        private readonly IServiceMessageRepository _serviceMessageRepository;

        public ServiceMessageController(IServiceMessageRepository serviceMessageRepository)
        {
            _serviceMessageRepository = serviceMessageRepository;
        }

        [AcceptVerbs("GET")]
        [Route("serviceMessages")]
        public IList<ServiceMessage> FindServiceMessages()
        {
            return _serviceMessageRepository.Find();
        }
    }
}
