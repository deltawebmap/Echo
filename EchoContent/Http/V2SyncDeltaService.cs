using LibDeltaSystem;
using LibDeltaSystem.Db.Content;
using LibDeltaSystem.Entities.CommonNet;
using LibDeltaSystem.Tools;
using Microsoft.AspNetCore.Http;
using MongoDB.Driver;
using Newtonsoft.Json;
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

        public static readonly DateTime masterEpoch = new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public override async Task OnRequest()
        {
            //Parse epoch
            DateTime epoch = masterEpoch;
            if(e.Request.Query.ContainsKey("last_epoch"))
            {
                if(!long.TryParse(e.Request.Query["last_epoch"], out long epochTime))
                {
                    await WriteString("last_epoch is not a valid epoch!", "text/plain", 400);
                    return;
                }
                epoch = masterEpoch.AddSeconds(epochTime);
            }

            //Find
            int returnEpoch = (int)(DateTime.UtcNow - masterEpoch).TotalSeconds;
            var filter = GetFilterDefinition(epoch);
            var response = await GetMongoCollection().FindAsync(filter);
            var responseList = await response.ToListAsync();

            //Get requested format
            string format = "json";
            if (e.Request.Query.ContainsKey("format"))
                format = e.Request.Query["format"];

            //Write
            await WriteResponse(responseList, new List<T>(), returnEpoch, format);
        }

        public abstract IMongoCollection<T> GetMongoCollection();

        public abstract FilterDefinition<T> GetFilterDefinition(DateTime epoch);

        public abstract O ConvertToOutputType(T data);

        public virtual async Task WriteResponse(List<T> adds, List<T> removes, int epoch, string format)
        {
            //Verify JSON response
            if(format != "json")
            {
                await ExitInvalidFormat("json");
                return;
            }
            await WriteJSONResponse(adds, removes, epoch);
        }

        public async Task WriteJSONResponse(List<T> adds, List<T> removes, int epoch)
        {
            //Create response template
            ResponseData r = new ResponseData
            {
                adds = MassConvertObjects(adds),
                removes = MassConvertObjects(removes),
                epoch = epoch
            };

            //Write
            await WriteJSON(r);
        }

        public async Task ExitInvalidFormat(params string[] formats)
        {
            await WriteString("Invalid format. Valid formats are: " + JsonConvert.SerializeObject(formats), "text/plain", 400);
        }

        public List<O> MassConvertObjects(List<T> objects)
        {
            List<O> o = new List<O>();
            foreach (var l in objects)
                o.Add(ConvertToOutputType(l));
            return o;
        }

        class ResponseData
        {
            public List<O> adds;
            public List<O> removes;
            public int epoch;
        }
    }
}
