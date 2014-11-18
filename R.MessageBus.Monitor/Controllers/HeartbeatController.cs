using System;
using System.Web.Http;
using R.MessageBus.Monitor.Interfaces;
using R.MessageBus.Monitor.Models;

namespace R.MessageBus.Monitor.Controllers
{
    public class HeartbeatController : ApiController
    {
        private readonly IHeartbeatRepository _heartbeatRepository;

        public HeartbeatController(IHeartbeatRepository heartbeatRepository)
        {
            _heartbeatRepository = heartbeatRepository;
        }

        [AcceptVerbs("GET")]
        [Route("heartbeats")]
        public QueryResult<Heartbeat> Find(string endPoint, string location, DateTime from, DateTime to, int pageSize = 50, int pageNumber = 1)
        {
            return _heartbeatRepository.Find(endPoint, location, from, to, pageSize, pageNumber);
        }
    }
}
