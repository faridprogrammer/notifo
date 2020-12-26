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
using Amazon.S3.Model;
using Microsoft.EntityFrameworkCore;
using Notifo.Domain.Apps.MongoDb;
using Notifo.Domain.Channels.Email;
using Notifo.Domain.Counters;
using Notifo.Infrastructure;
using Notifo.Infrastructure.SqlServer;

namespace Notifo.Domain.Apps.SqlServer
{
    public class SqlServerAppRepository : SqlServerStore<SqlServerApp>, IAppRepository
    {
        public SqlServerAppRepository(DbContext dbContext)
            : base(dbContext)
        {
        }

        public Task StoreAsync(List<(string Key, CounterMap Counters)> counters, CancellationToken ct)
        {
            throw new NotImplementedException();
        }

        public async Task<List<App>> QueryNonConfirmedEmailAddressesAsync(CancellationToken ct)
        {
            var documents =
                await Set.Where(x =>
                        x.Doc.EmailVerificationStatus == EmailVerificationStatus.Failed ||
                        x.Doc.EmailVerificationStatus == EmailVerificationStatus.Pending)
                    .ToListAsync(ct);

            return documents.Select(x => x.ToApp()).ToList();        }

        public async Task<List<App>> QueryAsync(string contributorId, CancellationToken ct)
        {
            var documents =
                await Set.Where(x => x.ContributorIds.Contains(contributorId))
                    .ToListAsync(ct);

            return documents.Select(x => x.ToApp()).ToList();        }

        public async Task<(App? App, string? Etag)> GetByApiKeyAsync(string apiKey, CancellationToken ct)
        {
            var document = await
                Set.Where(x => x.ApiKeys.Contains(apiKey))
                    .FirstOrDefaultAsync(ct);

            return (document?.ToApp(), document?.Etag);
        }

        public async Task<(App? App, string? Etag)> GetByEmailAddressAsync(string emailAddress, CancellationToken ct)
        {
            var document = await
                Set.Where(x => x.Doc.EmailAddress == emailAddress)
                    .FirstOrDefaultAsync(ct);

            return (document?.ToApp(), document?.Etag);
        }

        public async Task<(App? App, string? Etag)> GetAsync(string id, CancellationToken ct)
        {
            var document = await GetDocumentAsync(id, ct);

            return (document?.ToApp(), document?.Etag);        }

        public Task UpsertAsync(App app, string? oldEtag, CancellationToken ct)
        {
            var document = SqlServerApp.FromApp(app);
            return UpsertDocumentAsync(document.DocId, document, oldEtag, ct);
        }
    }
}
