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
using Amazon.Auth.AccessControlPolicy.ActionIdentifiers;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;
using Notifo.Infrastructure.MongoDb;

namespace Notifo.Infrastructure.SqlServer
{
    public class SqlServerStore<T> : SqlServerRepository<T> where T : SqlServerEntity
    {
        protected DbContext DbContext
        {
            get;
        }

        public SqlServerStore(DbContext dbContext)
            : base(dbContext)
        {
            DbContext = dbContext;
            Set = dbContext.Set<T>();
        }

        protected async Task<T?> GetDocumentAsync(string id, CancellationToken ct = default)
        {
            Guard.NotNullOrEmpty(id, nameof(id));

            var existing =
                await Set.Where(x => x.DocId == id)
                    .FirstOrDefaultAsync(ct);

            return existing;
        }

        protected async Task UpsertDocumentAsync(string id, T value, string? oldEtag = null, CancellationToken ct = default)
        {
            Guard.NotNullOrEmpty(id, nameof(id));
            Guard.NotNull(value, nameof(value));

            T found = default(T)!;
            if (!string.IsNullOrWhiteSpace(oldEtag))
                found = await Set.SingleOrDefaultAsync(ff => ff.DocId == id && ff.Etag == oldEtag, ct);
            else
                found = await Set.SingleOrDefaultAsync(ff => ff.DocId == id, ct);

            DbContext.Entry(found).State = found == null ? EntityState.Added : EntityState.Modified;
        }

        public async Task DeleteAsync(string id, CancellationToken ct = default)
        {
            Guard.NotNullOrEmpty(id, nameof(id));
            var found = await Set.SingleOrDefaultAsync(x => x.DocId == id, ct);
            Set.Remove(found);
        }
    }
}
