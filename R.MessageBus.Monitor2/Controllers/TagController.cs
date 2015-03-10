using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using R.MessageBus.Monitor.Interfaces;

namespace R.MessageBus.Monitor.Controllers
{
    public class TagController : ApiController
    {
        private readonly ITagRepository _tagRepository;
        public TagController(ITagRepository tagRepository)
        {
            _tagRepository = tagRepository;
        }

        [AcceptVerbs("GET")]
        [Route("tags")]
        public IList<string> FindTags(string query = "")
        {
            var tags = _tagRepository.Find();
            if (!string.IsNullOrEmpty(query))
            {
                tags = tags.Where(x => x.Name.ToLower().Contains(query.ToLower())).ToList();
            }
            return tags.OrderBy(x => x.Name).Select(x => x.Name).ToList();
        }
    }
}
