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
using Notifo.Infrastructure;
using Notifo.Infrastructure.SqlServer;
using Org.BouncyCastle.Math.EC.Rfc7748;
using SharpCompress.Archives;

namespace Notifo.Domain.Media.SqlServer
{
    public sealed class SqlServerMediaRepository : SqlServerStore<SqlServerMedia>, IMediaRepository
    {
        public SqlServerMediaRepository(DbContext dbContext)
            : base(dbContext)
        {
        }

        public async Task<IResultList<Media>> QueryAsync(string appId, MediaQuery query, CancellationToken ct)
        {
            var dbQuery = Set.Where(media => media.Doc.AppId == appId);

            if (!string.IsNullOrWhiteSpace(query.Query))
            {
                // todo: support regex
                dbQuery = dbQuery.Where(media => media.Doc.FileName.Contains(query.Query));
            }

            var taskForItems = dbQuery.ToListAsync(ct);
            var taskForCount = dbQuery.CountAsync(ct);

            await Task.WhenAll(
                taskForItems,
                taskForCount);

            return ResultList.Create(taskForCount.Result, taskForItems.Result.Select(x => x.ToMedia()));
        }

        public async Task<Media?> GetAsync(string appId, string fileName, CancellationToken ct)
        {
            var docId = SqlServerMedia.CreateId(appId, fileName);
            var document = await Set.Where(x => x.DocId == docId).FirstOrDefaultAsync(ct);
            return document?.ToMedia();
        }

        public async Task UpsertAsync(Media media, CancellationToken ct)
        {
            var document = SqlServerMedia.FromMedia(media);
            var found = await Set.SingleOrDefaultAsync(x => x.DocId == document.DocId, ct);
            DbContext.Entry(document).State = found switch
            {
                null => EntityState.Added,
                _ => EntityState.Modified
            };
        }

        public async Task DeleteAsync(string appId, string fileName, CancellationToken ct)
        {
            var docId = SqlServerMedia.CreateId(appId, fileName);
            var found = await Set.SingleOrDefaultAsync(media => media.DocId == docId, ct);
            DbContext.Entry(found).State = EntityState.Deleted;
        }
    }
}
