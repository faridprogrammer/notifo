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
using Microsoft.EntityFrameworkCore;
using Notifo.Infrastructure.Initialization;

namespace Notifo.Infrastructure.SqlServer
{
    public class SqlServerRepository<TEntity> : IInitializable where TEntity : class
    {
        protected DbSet<TEntity> Set
        {
            get;
            set;
        }

        protected DbContext DbContext
        {
            get;
        }

        public SqlServerRepository(DbContext dbContext)
        {
            DbContext = dbContext;
            Set = dbContext.Set<TEntity>();
        }

        public Task InitializeAsync(CancellationToken ct = default)
        {
            return Task.CompletedTask;
        }
    }
}
