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
using System.Threading;

namespace R.MessageBus.Monitor.Models
{
    public static class Globals 
    {
        public static readonly List<ConsumerEnvironment> Environments = new List<ConsumerEnvironment>();
        public static readonly Dictionary<string, Timer> Timers = new Dictionary<string, Timer>();
        public static Settings Settings { get; set; }
        public static string ErrorExpiry { get; set; }
        public static string HeartbeatExpiry { get; set; }
        public static string AuditExpiry { get; set; }
    }
}