using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using MongoDB.Bson;
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
        public IList<Audit> FindAudits(Guid correlationId)
        {
            return _auditRepository.Find(correlationId);
        }

        [AcceptVerbs("GET")]
        [Route("audits")]
        public IList<Audit> FindAudits(DateTime from, DateTime to, string tags = null)
        {
            List<string> tagList = null;
            if (!string.IsNullOrEmpty(tags))
            {
                tagList = tags.Split(',').ToList();
            }

            var audits = _auditRepository.Find(from, to);

            if (tagList != null && tagList.Count > 0)
            {
                var results = new List<Audit>();

                var services = _serviceRepository.Find();

                foreach (Audit audit in audits)
                {
                    bool match = services.Any(service => (audit.SourceAddress == service.Name || audit.DestinationAddress == service.Name) && service.Tags != null && service.Tags.Any(tagList.Contains));
                    if (match)
                    {
                        results.Add(audit);
                    }
                }

                return results;
            }
            
            return audits;
        }

        [AcceptVerbs("GET")]
        [Route("audit/{id}")]
        public Audit Get(string id)
        {
            return _auditRepository.Get(new ObjectId(id));
        }
    }
}
