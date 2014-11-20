using System;
using System.Collections.Generic;
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
        public List<Heartbeat> Find(string name, string location, DateTime from, DateTime to)
        {
            return _heartbeatRepository.Find(name, location, from, to);
        }
    }
}
