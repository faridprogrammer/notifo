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
using Notifo.Infrastructure;
using Notifo.Infrastructure.MongoDb;
using Notifo.Infrastructure.SqlServer;

namespace Notifo.Domain.Templates.MongoDb
{
    public sealed class SqlServerTemplateRepository : SqlServerStore<SqlServerTemplate>, ITemplateRepository
    {
        public SqlServerTemplateRepository(DbContext dbContext)
            : base(dbContext)
        {
        }

        public async Task<IResultList<Template>> QueryAsync(string appId, TemplateQuery query, CancellationToken ct)
        {
            var filters = Set.Where(ff => ff.Doc.AppId == appId);

            if (!string.IsNullOrWhiteSpace(query.Query))
            {
                // todo regex support
                filters = filters.Where(ff => ff.Doc.Code.Contains(query.Query));
            }

            var taskForItems = filters.ToListAsync(ct);
            var taskForCount = filters.CountAsync(ct);

            await Task.WhenAll(
                taskForItems,
                taskForCount);

            return ResultList.Create(taskForCount.Result, taskForItems.Result.Select(x => x.ToTemplate()));
        }

        public async Task<(Template? Template, string? Etag)> GetAsync(string appId, string code, CancellationToken ct)
        {
            var docId = MongoDbTemplate.CreateId(appId, code);

            var document = await GetDocumentAsync(docId, ct);

            return (document?.ToTemplate(), document?.Etag);
        }

        public Task UpsertAsync(Template template, string? oldEtag, CancellationToken ct)
        {
            var document = SqlServerTemplate.FromTemplate(template);

            return UpsertDocumentAsync(document.DocId, document, oldEtag, ct);
        }

        public async Task DeleteAsync(string appId, string id, CancellationToken ct)
        {
            var docId = MongoDbTemplate.CreateId(appId, id);
            var found = await Set.SingleOrDefaultAsync(ff => ff.DocId == docId, ct);
            if (found == null)
                return;
            DbContext.Entry(found).State = EntityState.Deleted;
        }

    }
}
