// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Infrastructure.MongoDb;
using Notifo.Infrastructure.SqlServer;

namespace Notifo.Domain.Topics.MongoDb
{
    public sealed class SqlServerTopic : SqlServerEntity<Topic>
    {
        public static string CreateId(string appId, string path)
        {
            return $"{appId}_{path}";
        }
    }
}
