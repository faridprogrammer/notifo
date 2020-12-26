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
using Notifo.Infrastructure.SqlServer;

namespace Notifo.Domain.Media.SqlServer
{
    public sealed class SqlServerMedia : SqlServerEntity<Media>
    {
        public static string CreateId(string appId, string fileName)
        {
            return $"{appId}_{fileName}";
        }

        public static SqlServerMedia FromMedia(Media media)
        {
            var id = CreateId(media.AppId, media.FileName);

            var result = new SqlServerMedia
            {
                DocId = id,
                Doc = media,
                Etag = GenerateEtag(),
            };

            return result;
        }

        public Media ToMedia()
        {
            return Doc;
        }
    }
}
