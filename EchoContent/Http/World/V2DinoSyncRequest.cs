using LibDeltaSystem;
using LibDeltaSystem.Db.Content;
using LibDeltaSystem.Entities.CommonNet;
using LibDeltaSystem.Tools;
using Microsoft.AspNetCore.Http;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EchoContent.Http.World
{
    public class V2DinoSyncRequest : V2SyncDeltaService<DbDino, NetDino>
    {
        public V2DinoSyncRequest(DeltaConnection conn, HttpContext e) : base(conn, e)
        {
        }

        public override NetDino ConvertToOutputType(DbDino data)
        {
            return NetDino.ConvertDbDino(data);
        }

        public override FilterDefinition<DbDino> GetFilterDefinition(DateTime epoch)
        {
            var filterBuilder = Builders<DbDino>.Filter;
            var filter = filterBuilder.Eq("is_tamed", true) & FilterBuilderToolDb.CreateTribeFilter<DbDino>(server, tribeId);
            return filter;
        }

        public override IMongoCollection<DbDino> GetMongoCollection()
        {
            return conn.content_dinos;
        }
    }
}
