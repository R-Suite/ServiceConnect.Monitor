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

using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;

namespace R.MessageBus.Monitor.Controllers
{
    public class HomeController : ApiController
    {
        private readonly string _path;

        public HomeController()
        {
            _path = new DirectoryInfo(Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location)).Parent.FullName;
        }

        [AcceptVerbs("GET")]
        [Route("Scripts/{*filename}")]
        public HttpResponseMessage Scripts(string filename)
        {
            var response = GetResponse(_path + "/Scripts/" + filename);
            return response;
        }

        private static HttpResponseMessage GetResponse(string filename)
        {
            byte[] resourceByteArray;
            using (var fs = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                resourceByteArray = ReadFully(fs);
            }

            string contentType = MimeTypeMap.GetMimeType(Path.GetExtension(filename));
            var dataStream = new MemoryStream(resourceByteArray);
            var response = new HttpResponseMessage(HttpStatusCode.OK) {Content = new StreamContent(dataStream)};
            response.Content.Headers.ContentType = new MediaTypeHeaderValue(contentType);
            return response;
        }

        [AcceptVerbs("GET")]
        public HttpResponseMessage Index()
        {
            var response = GetResponse(_path + "/Index.html");
            return response;
        }

        public static byte[] ReadFully(Stream input)
        {
            var buffer = new byte[16 * 1024];
            using (var ms = new MemoryStream())
            {
                int read;
                while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }
                return ms.ToArray();
            }
        }
    }
}