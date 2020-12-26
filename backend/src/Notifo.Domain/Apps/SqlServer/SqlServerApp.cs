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

namespace Notifo.Domain.Apps.SqlServer
{
    public class SqlServerApp : SqlServerEntity<App>
    {
        public Guid Id
        {
            get;
            set;
        }

        public List<string> ContributorIds
        {
            get;
            set;
        }

        public List<string> ApiKeys
        {
            get;
            set;
        }

        public static SqlServerApp FromApp(App app)
        {
            var id = app.Id;

            var result = new SqlServerApp
            {
                Id = Guid.NewGuid(),
                DocId = id,
                Doc = app,
                Etag = GenerateEtag()
            };

            if (app.Contributors?.Count > 0)
            {
                result.ContributorIds = app.Contributors.Keys.ToList();
            }

            if (app.ApiKeys?.Count > 0)
            {
                result.ApiKeys = app.ApiKeys.Keys.ToList();
            }

            return result;
        }

        public App ToApp()
        {
            return Doc;
        }
    }
}
