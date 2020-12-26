// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson.Serialization.Attributes;
using Notifo.Domain.Subscriptions.MongoDb;
using Notifo.Infrastructure.SqlServer;

namespace Notifo.Domain.Subscriptions.SqlServer
{
    public sealed class SqlServerSubscription
    {
        public string DocId { get; set; }

        [Required]
        public string AppId { get; set; }

        [Required]
        public string UserId { get; set; }

        [Required]
        public string[] TopicArray { get; set; }

        [Required]
        public string TopicPrefix { get; set; }

        [Required]
        public NotificationSettings TopicSettings { get; set; }

        public static string CreateId(string appId, string userId, string topicPrefix)
        {
            return $"{appId}_{userId}_{topicPrefix}";
        }

        public static SqlServerSubscription FromSubscription(string appId, SubscriptionUpdate update)
        {
            var docId = CreateId(appId, update.UserId, update.TopicPrefix);

            var result = new SqlServerSubscription
            {
                DocId = docId,
                AppId = appId,
                TopicArray = new TopicId(update.TopicPrefix).GetParts(),
                TopicPrefix = update.TopicPrefix,
                TopicSettings = update.TopicSettings,
                UserId = update.UserId
            };

            return result;
        }

        public Subscription ToSubscription()
        {
            return new Subscription
            {
                AppId = AppId,
                TopicPrefix = TopicPrefix,
                TopicSettings = TopicSettings,
                UserId = UserId
            };
        }
    }
}
