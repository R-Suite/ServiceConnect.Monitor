using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using R.MessageBus.Monitor.Interfaces;
using R.MessageBus.Monitor.Models;

namespace R.MessageBus.Monitor.Controllers
{
    public class AuditController : ApiController
    {
        private readonly IAuditRepository _auditRepository;
        private readonly IServiceRepository _serviceRepository;

        public AuditController(IAuditRepository auditRepository, IServiceRepository serviceRepository)
        {
            _auditRepository = auditRepository;
            _serviceRepository = serviceRepository;
        }

        [AcceptVerbs("GET")]
        [Route("audits")]
        public IList<Audit> FindAudits(List<string> tags, DateTime from, DateTime to)
        {
            var audits = _auditRepository.Find(from, to);
            
            if (tags != null && tags.Count > 0)
            {
                var results = new List<Audit>();

                var services = _serviceRepository.Find();

                foreach (Audit audit in audits)
                {
                    bool match = services.Any(service => audit.MessageType == service.Name && service.Tags != null && service.Tags.Any(tags.Contains));
                    if (match)
                    {
                        results.Add(audit);
                    }
                }

                return results;
            }
            
            return audits;
        }
    }
}
