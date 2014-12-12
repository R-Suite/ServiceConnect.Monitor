using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using MongoDB.Bson;
using R.MessageBus.Monitor.Interfaces;
using R.MessageBus.Monitor.Models;

namespace R.MessageBus.Monitor.Controllers
{
    public class ErrorController : ApiController
    {
        private readonly IErrorRepository _errorRepository;
        private readonly IServiceRepository _serviceRepository;

        public ErrorController(IErrorRepository errorRepository, IServiceRepository serviceRepository)
        {
            _errorRepository = errorRepository;
            _serviceRepository = serviceRepository;
        }

        [AcceptVerbs("GET")]
        [Route("errors")]
        public IList<Error> FindErrors(Guid correlationId)
        {
            return _errorRepository.Find(correlationId);
        }

        [AcceptVerbs("GET")]
        [Route("errors")]
        public IList<Error> FindErrors(DateTime from, DateTime to, string tags = null)
        {
            List<string> tagList = null;
            if (!string.IsNullOrEmpty(tags))
            {
                tagList = tags.Split(',').ToList();
            }

            var errors = _errorRepository.Find(from, to);

            if (tagList != null && tagList.Count > 0)
            {
                var results = new List<Error>();

                var services = _serviceRepository.Find();

                foreach (Error error in errors)
                {
                    bool match = services.Any(service => (error.SourceAddress == service.Name || error.DestinationAddress == service.Name) && service.Tags != null && service.Tags.Any(tagList.Contains));
                    if (match)
                    {
                        results.Add(error);
                    }
                }

                return results;
            }
            
            return errors;
        }

        [AcceptVerbs("GET")]
        [Route("error/{id}")]
        public Error Get(string id)
        {
            return _errorRepository.Get(new ObjectId(id));
        }
    }
}
