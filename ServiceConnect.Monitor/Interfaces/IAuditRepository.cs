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
using System.Threading.Tasks;
using MongoDB.Bson;
using ServiceConnect.Monitor.Models;

namespace ServiceConnect.Monitor.Interfaces
{
    public interface IAuditRepository
    {
        Task InsertAudit(Audit model);
        Task<List<Audit>> Find(DateTime @from, DateTime to);
        Task EnsureIndex();
        Task<Audit> Get(ObjectId objectId);
        Task Remove(DateTime before);
        Task<List<Audit>> Find(Guid correlationId);
    }
}