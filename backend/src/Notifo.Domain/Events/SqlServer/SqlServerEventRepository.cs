// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Notifo.Domain.Counters;
using Notifo.Domain.Events.MongoDb;
using Notifo.Infrastructure;
using Notifo.Infrastructure.SqlServer;

namespace Notifo.Domain.Events.SqlServer
{
    public sealed class SqlServerEventRepository : SqlServerStore<SqlServerEvent>, IEventRepository
    {
        public SqlServerEventRepository(DbContext dbContext)
            : base(dbContext)
        {
        }

        public Task StoreAsync(List<((string AppId, string EventId) Key, CounterMap Counters)> counters, CancellationToken ct)
        {
            throw new NotImplementedException();
        }

        public async Task<IResultList<Event>> QueryAsync(string appId, EventQuery query, CancellationToken ct)
        {
            var dbQuery = Set.Where(ff => ff.Doc.AppId == appId);

            if (!string.IsNullOrWhiteSpace(query.Query))
            {
                // todo: support regex
                var regex = query.Query;

                dbQuery = dbQuery.Where(ff => ff.SearchText.Contains(regex) || ff.Doc.Topic.Contains(regex));
            }

            var taskForItems = dbQuery.OrderByDescending(x => x.Doc.Created).ToListAsync(ct);
            var taskForCount = dbQuery.CountAsync(ct);

            await Task.WhenAll(taskForItems, taskForCount);

            return ResultList.Create(taskForCount.Result, taskForItems.Result.Select(x => x.ToEvent()));
        }

        public Task InsertAsync(Event @event, CancellationToken ct)
        {
            // todo: duplicate key exception handling
            var document = SqlServerEvent.FromEvent(@event);
            DbContext.Entry(document).State = EntityState.Added;
            return Task.CompletedTask;
        }
    }
}
