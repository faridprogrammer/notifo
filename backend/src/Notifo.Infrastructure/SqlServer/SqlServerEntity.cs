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

namespace Notifo.Infrastructure.SqlServer
{
    public abstract class SqlServerEntity
    {
        public string DocId
        {
            get;
            set;
        }

        public string Etag { get; set; }

        public static string GenerateEtag()
        {
            return Guid.NewGuid().ToString().Replace("-", string.Empty);
        }
    }

    public abstract class SqlServerEntity<T> : SqlServerEntity
    {
        public T Doc { get; set; }
    }
}
