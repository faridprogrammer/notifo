using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using Notifo.Infrastructure;
using Notifo.Infrastructure.SqlServer;

namespace Notifo.Domain.Log.SqlServer
{
    public class SqlServerLogRepository: SqlServerStore<SqlServerLogEntity>, ILogRepository
    {
        public SqlServerLogRepository(DbContext dbContext)
            : base(dbContext)
        {
        }

        public async Task<IResultList<LogEntry>> QueryAsync(string appId, LogQuery query, CancellationToken ct = default)
        {
            var dbQuery = Set.Where(ff => ff.Doc.AppId == appId);
            // todo: regex query support
            if (!string.IsNullOrWhiteSpace(query.Query))
            {
                dbQuery = dbQuery.Where(ff => ff.Doc.Message.Contains(query.Query));
            }

            var taskForItems = dbQuery.ToListAsync(ct);
            var taskForCount = dbQuery.CountAsync(ct);

            await Task.WhenAll(
                taskForItems,
                taskForCount);

            return ResultList.Create(taskForCount.Result, taskForItems.Result.Select(x => x.ToEntry()));        }

        public Task UpdateAsync(IEnumerable<(string AppId, string Message, int Count)> updates, Instant now, CancellationToken ct)
        {
            var writes = new List<WriteModel<MongoDbLogEntry>>();

            foreach (var (appId, message, count) in updates)
            {
                var docId = MongoDbLogEntry.CreateId(appId, message);

                var update =
                    Update
                        .SetOnInsert(x => x.DocId, docId)
                        .SetOnInsert(x => x.Entry.FirstSeen, now)
                        .SetOnInsert(x => x.Entry.AppId, appId)
                        .SetOnInsert(x => x.Entry.Message, message)
                        .Set(x => x.Entry.LastSeen, now)
                        .Inc(x => x.Entry.Count, count);

                var model = new UpdateOneModel<MongoDbLogEntry>(Filter.Eq(x => x.DocId, docId), update)
                {
                    IsUpsert = true
                };

                writes.Add(model);
            }

            return Collection.BulkWriteAsync(writes, cancellationToken: ct);        }
    }
}
