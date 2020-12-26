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
using System.Threading.Tasks;
using Microsoft.Extensions.ObjectPool;
using Notifo.Domain.Events.MongoDb;
using Notifo.Infrastructure.SqlServer;

namespace Notifo.Domain.Events.SqlServer
{
    public sealed class SqlServerEvent: SqlServerEntity<Event>
    {
        private static readonly ObjectPool<StringBuilder> StringBuilderPool = ObjectPool.Create<StringBuilder>(new StringBuilderPooledObjectPolicy());

        public string SearchText { get; set; }

        public static string CreateId(string appId, string id)
        {
            return $"{appId}_{id}";
        }

        public static SqlServerEvent FromEvent(Event @event)
        {
            var docId = CreateId(@event.AppId, @event.Id);

            return new SqlServerEvent
            {
                DocId = docId,
                Doc = @event,
                Etag = Guid.NewGuid().ToString(),
                SearchText = BuildSearchText(@event)
            };
        }

        private static string BuildSearchText(Event @event)
        {
            var sb = StringBuilderPool.Get();
            try
            {
                foreach (var text in @event.Formatting.Subject.Values)
                {
                    sb.AppendLine(text);
                }

                if (@event.Formatting.Body != null)
                {
                    foreach (var text in @event.Formatting.Body.Values)
                    {
                        sb.AppendLine(text);
                    }
                }

                return sb.ToString();
            }
            finally
            {
                StringBuilderPool.Return(sb);
            }
        }

        public Event ToEvent()
        {
            return Doc;
        }
    }
}
