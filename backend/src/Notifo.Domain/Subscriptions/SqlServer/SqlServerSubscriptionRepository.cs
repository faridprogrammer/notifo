// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Notifo.Infrastructure;
using Notifo.Infrastructure.SqlServer;

namespace Notifo.Domain.Subscriptions.SqlServer
{
    public sealed class SqlServerSubscriptionRepository : SqlServerRepository<SqlServerSubscription>, ISubscriptionRepository
    {
        public SqlServerSubscriptionRepository(DbContext dbContext)
            : base(dbContext)
        {
        }

        public async IAsyncEnumerable<Subscription> QueryAsync(string appId, TopicId topic, string? userId, CancellationToken ct = default)
        {
            var query = CreatePrefixFilter(appId, userId, topic, false);

            var find = query.OrderBy(x => x.UserId);

            var lastSubscription = (SqlServerSubscription?)null;

            foreach (var subscription in await find.ToListAsync(ct))
            {
                if (topic.Id.StartsWith(subscription.TopicPrefix, StringComparison.OrdinalIgnoreCase))
                {
                    if (string.Equals(subscription.UserId, lastSubscription?.UserId, StringComparison.OrdinalIgnoreCase))
                    {
                        if (subscription.TopicPrefix.Length > lastSubscription!.TopicPrefix.Length)
                        {
                            lastSubscription = subscription;
                        }
                    }
                    else
                    {
                        if (lastSubscription != null)
                        {
                            yield return lastSubscription.ToSubscription();
                        }

                        lastSubscription = subscription;
                    }
                }
            }

            if (lastSubscription != null)
            {
                yield return lastSubscription.ToSubscription();
            }
        }

        private IQueryable<SqlServerSubscription> CreatePrefixFilter(string appId, string? userId, TopicId topic, bool withUser)
        {
            var query = Set.Where(subscription => subscription.AppId == appId);

            if (withUser)
            {
                query = query.Where(subscription => subscription.UserId == userId);
            }
            else
            {
                query = query.Where(subscription => subscription.UserId != userId);
            }

            var index = 0;

            foreach (var part in topic.GetParts())
            {
                var fieldName = $"ta.{index}";

                // todo how to store parts

                //filters.Add(
                //    Filter.Or(
                //        Filter.Eq(fieldName, part),
                //        Filter.Exists(fieldName, false)));

                index++;
            }

            return query;
        }

        public async Task<IResultList<Subscription>> QueryAsync(string appId, SubscriptionQuery query, CancellationToken ct = default)
        {
            var dbQuery = Set.Where(subscription => subscription.AppId == appId);

            if (!string.IsNullOrWhiteSpace(query.UserId))
            {
                dbQuery = dbQuery.Where(subscription => subscription.UserId == query.UserId);
            }

            if (!string.IsNullOrWhiteSpace(query.Query))
            {
                // todo: support regex
                dbQuery = dbQuery.Where(subscription => subscription.TopicPrefix.Contains(query.Query));
            }

            var taskForItems = dbQuery.ToListAsync(ct);
            var taskForCount = dbQuery.CountAsync(ct);

            await Task.WhenAll(
                taskForItems,
                taskForCount);

            return ResultList.Create(taskForCount.Result, taskForItems.Result.Select(x => x.ToSubscription()));
        }

        public async Task<Subscription?> GetAsync(string appId, string userId, TopicId prefix, CancellationToken ct = default)
        {
            var topicPrefix = prefix.Id;

            var document =
                await Set.Where(x => x.AppId == appId && x.UserId == userId && x.TopicPrefix == topicPrefix)
                    .FirstOrDefaultAsync(ct);

            return document?.ToSubscription();
        }

        public async Task<Subscription> SubscribeAsync(string appId, SubscriptionUpdate update, CancellationToken ct = default)
        {
            var document = SqlServerSubscription.FromSubscription(appId, update);

            if (Set.Any(ff => ff.DocId == document.DocId))
            {
                var found = await Set.SingleOrDefaultAsync(ff => ff.DocId == document.DocId, ct);
                found.TopicArray = document.TopicArray;
                found.TopicPrefix = document.TopicPrefix;
                found.TopicSettings = document.TopicSettings;
                DbContext.Entry(found).State = EntityState.Modified;
            }

            DbContext.Entry(document).State = EntityState.Added;

            return document.ToSubscription();
        }

        public Task SubscribeWhenNotFoundAsync(string appId, SubscriptionUpdate update, CancellationToken ct = default)
        {
            var document = SqlServerSubscription.FromSubscription(appId, update);
            Set.Add(document);
            return Task.CompletedTask;
        }

        public async Task UnsubscribeAsync(string appId, string userId, TopicId prefix, CancellationToken ct = default)
        {
            var id = SqlServerSubscription.CreateId(appId, userId, prefix);

            var found = await Set.Where(ff => ff.DocId == id).SingleOrDefaultAsync(ct);
            DbContext.Entry(found).State = EntityState.Deleted;
        }

        public async Task UnsubscribeByPrefixAsync(string appId, string userId, TopicId prefix, CancellationToken ct = default)
        {
            var query = CreatePrefixFilter(appId, userId, prefix, true);

            var found = await query.ToListAsync(ct);

            foreach (var sqlServerSubscription in found)
            {
                DbContext.Entry(sqlServerSubscription).State = EntityState.Deleted;
            }
        }
    }
}
