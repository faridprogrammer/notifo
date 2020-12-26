using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Notifo.Infrastructure.SqlServer;

namespace Notifo.Domain.Log.SqlServer
{
    public sealed class SqlServerLogEntity : SqlServerEntity<LogEntry>
    {
        public static string CreateId(string appId, string message)
        {
            return $"{appId}_{message}";
        }

        public static SqlServerLogEntity FromMedia(LogEntry entry, string etag)
        {
            var id = CreateId(entry.AppId, entry.Message);

            var result = new SqlServerLogEntity { DocId = id, Doc = entry, Etag = etag };

            return result;
        }

        public LogEntry ToEntry()
        {
            return Doc;
        }
    }
}
