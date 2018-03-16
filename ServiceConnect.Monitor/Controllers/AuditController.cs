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
using System.Threading.Tasks;
using System.Web.Http;
using MongoDB.Bson;
using ServiceConnect.Monitor.Interfaces;
using ServiceConnect.Monitor.Models;

namespace ServiceConnect.Monitor.Controllers
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
        public async Task<IList<Audit>> FindAudits(Guid correlationId)
        {
            return await _auditRepository.Find(correlationId);
        }

        [AcceptVerbs("GET")]
        [Route("audits")]
        public async Task<IList<Audit>> FindAudits(DateTime from, DateTime to, string tags = null)
        {
            List<string> tagList = null;
            if (!string.IsNullOrEmpty(tags))
                tagList = tags.Split(',').ToList();

            var audits = await _auditRepository.Find(from, to);

            if (tagList != null && tagList.Count > 0)
            {
                var results = new List<Audit>();

                var services = await _serviceRepository.Find();

                foreach (var audit in audits)
                {
                    var match = services.Any(service => (audit.SourceAddress == service.Name || audit.DestinationAddress == service.Name) && service.Tags != null && service.Tags.Any(tagList.Contains));
                    if (match)
                        results.Add(audit);
                }

                return results;
            }
            
            return audits;
        }

        [AcceptVerbs("GET")]
        [Route("audit/{id}")]
        public async Task<Audit> Get(string id)
        {
            return await _auditRepository.Get(new ObjectId(id));
        }
    }
}
