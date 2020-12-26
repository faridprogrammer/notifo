// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Amazon.S3.Model;
using Microsoft.EntityFrameworkCore;
using MongoDB.Bson;
using MongoDB.Driver;
using NodaTime;
using Notifo.Domain.Counters;
using Notifo.Infrastructure;
using Notifo.Infrastructure.MongoDb;
using Notifo.Infrastructure.SqlServer;

namespace Notifo.Domain.Topics.MongoDb
{
    public sealed class SqlServerTopicRepository : SqlServerRepository<SqlServerTopic>, ITopicRepository
    {
        private readonly IClock clock;

        public SqlServerTopicRepository(DbContext dbContext, IClock clock)
            : base(dbContext)
        {
            this.clock = clock;
        }

        public async Task<IResultList<Topic>> QueryAsync(string appId, TopicQuery query, CancellationToken ct)
        {
            var filters = Set.Where(ff => ff.Doc.AppId == appId);

            if (!string.IsNullOrWhiteSpace(query.Query))
            {
                // todo regex support
                filters = filters.Where(ff => ff.Doc.Path.Contains(query.Query));
            }

            var taskForItems = filters.OrderByDescending(x => x.Doc.LastUpdate).ToListAsync(ct);
            var taskForCount = filters.CountAsync(ct);

            await Task.WhenAll(
                taskForItems,
                taskForCount);

            return ResultList.Create(taskForCount.Result, taskForItems.Result.Select(x => x.Doc));
        }

        public Task StoreAsync(List<((string AppId, string Path) Key, CounterMap Counters)> counters, CancellationToken ct)
        {
            var writes = new List<WriteModel<MongoDbTopic>>();

            var now = clock.GetCurrentInstant();

            foreach (var ((appId, path), appCounters) in counters)
            {
                var docId = MongoDbTopic.CreateId(appId, path);

                var updates = new List<UpdateDefinition<MongoDbTopic>>
                {
                    DbLoggerCategory.Update.Set(x => x.Doc.LastUpdate, now),
                    DbLoggerCategory.Update.SetOnInsert(x => x.Doc.AppId, appId),
                    DbLoggerCategory.Update.SetOnInsert(x => x.Doc.Path, path)
                };

                foreach (var (key, value) in appCounters)
                {
                    updates.Add(DbLoggerCategory.Update.Inc($"Counters.{key}", value));
                }

                var model = new UpdateOneModel<MongoDbTopic>(Filter.Eq(x => x.DocId, docId), DbLoggerCategory.Update.Combine(updates));

                writes.Add(model);
            }

            return Collection.BulkWriteAsync(writes, cancellationToken: ct);
        }
    }
}
