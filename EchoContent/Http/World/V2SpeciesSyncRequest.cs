using LibDeltaSystem;
using LibDeltaSystem.Db.ArkEntries;
using LibDeltaSystem.Entities.ArkEntries.Dinosaur;
using LibDeltaSystem.WebFramework;
using Microsoft.AspNetCore.Http;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EchoContent.Http.World
{
    public class V2SpeciesSyncRequest : V2SyncDeltaService<DbArkEntry<DinosaurEntry>, DinosaurEntry>
    {
        public V2SpeciesSyncRequest(DeltaConnection conn, HttpContext e) : base(conn, e)
        {
        }

        public override DinosaurEntry ConvertToOutputType(DbArkEntry<DinosaurEntry> data)
        {
            return data.data;
        }

        public override FilterDefinition<DbArkEntry<DinosaurEntry>> GetFilterDefinition(DateTime epoch)
        {
            var filterBuilder = Builders<DbArkEntry<DinosaurEntry>>.Filter;
            return filterBuilder.Gt("time", epoch);
        }

        public override IMongoCollection<DbArkEntry<DinosaurEntry>> GetMongoCollection()
        {
            return conn.arkentries_dinos;
        }

        public override async Task<bool> OnPreRequest()
        {
            //Replaces the built in one from earlier
            return true;
        }

        public override async Task<bool> SetArgs(Dictionary<string, string> args)
        {
            //Replaces the built in one from earlier
            return true;
        }
    }
}
