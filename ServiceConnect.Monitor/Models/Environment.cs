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

namespace ServiceConnect.Monitor.Models
{
    public class Environment
    {
        public string Server { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string ErrorQueue { get; set; }
        public string AuditQueue { get; set; }
        public string HeartbeatQueue { get; set; }
        public bool SslEnabled { get; set; }
        public string CertBase64 { get; set; }
        public string CertPassword { get; set; }

    }
}