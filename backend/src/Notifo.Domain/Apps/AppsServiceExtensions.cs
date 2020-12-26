﻿// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Notifo.Domain.Apps;
using Notifo.Domain.Apps.MongoDb;
using Notifo.Domain.Apps.SqlServer;
using Notifo.Domain.Counters;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class AppsServiceExtensions
    {
        public static void AddMyApps(this IServiceCollection services)
        {
            services.AddSingletonAs<SqlServerAppRepository>()
                .As<IAppRepository>();

            services.AddSingletonAs<AppStore>()
                .As<IAppStore>().As<ICounterTarget>();
        }
    }
}
