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

namespace EchoContent.Http
{
    public abstract class V2SyncDeltaService<T, O> : EchoTribeDeltaService
    {
        public V2SyncDeltaService(DeltaConnection conn, HttpContext e) : base(conn, e)
        {
        }

        public override async Task OnRequest()
        {
            //Parse epoch
            DateTime epoch = DateTime.MinValue;
            if(e.Request.Query.ContainsKey("last_epoch"))
            {
                if(!long.TryParse(e.Request.Query["last_epoch"], out long epochTime))
                {
                    await WriteString("last_epoch is not a valid epoch!", "text/plain", 400);
                    return;
                }
                epoch = new DateTime(epochTime);
            }

            //Find
            string returnEpoch = DateTime.UtcNow.Ticks.ToString();
            var filter = GetFilterDefinition(epoch);
            var response = await GetMongoCollection().FindAsync(filter);
            var responseList = await response.ToListAsync();

            //Create response template
            ResponseData r = new ResponseData
            {
                adds = new List<O>(),
                epoch = returnEpoch
            };

            //Convert
            foreach (var l in responseList)
                r.adds.Add(ConvertToOutputType(l));

            //Write
            await WriteJSON(r);
        }

        public abstract IMongoCollection<T> GetMongoCollection();

        public abstract FilterDefinition<T> GetFilterDefinition(DateTime epoch);

        public abstract O ConvertToOutputType(T data);

        class ResponseData
        {
            public List<O> adds;
            public string epoch;
        }
    }
}
