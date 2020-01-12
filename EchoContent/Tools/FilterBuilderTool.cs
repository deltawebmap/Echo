using LibDeltaSystem.Db.System;
using LibDeltaSystem.Tools;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Text;

namespace EchoContent.Tools
{
    public static class FilterBuilderTool
    {
        public static FilterDefinition<T> CreateTribeFilter<T>(DbServer server, int? tribeId)
        {
            return FilterBuilderToolDb.CreateTribeFilter<T>(server, tribeId);
        }
    }
}
