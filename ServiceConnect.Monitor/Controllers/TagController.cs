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

using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using ServiceConnect.Monitor.Interfaces;

namespace ServiceConnect.Monitor.Controllers
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
                tags = tags.Where(x => x.Name.ToLower().Contains(query.ToLower())).ToList();

            return tags.OrderBy(x => x.Name).Select(x => x.Name).ToList();
        }
    }
}
