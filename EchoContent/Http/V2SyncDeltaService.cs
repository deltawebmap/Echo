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
            //Get collection
            var collection = GetMongoCollection();
            var filter = GetFilterDefinition(DateTime.MinValue);
            int returnEpoch = (int)(DateTime.UtcNow - masterEpoch).TotalSeconds;

            //Get requested format
            string format = "json";
            if (e.Request.Query.ContainsKey("format"))
                format = e.Request.Query["format"];

            //Write headers
            long count = await collection.CountDocumentsAsync(filter);
            e.Response.Headers.Add("X-Delta-Sync-TotalItems", count.ToString());

            //Check if this is counts only. If it is, we're done already
            if(format == "counts_only")
            {
                await WriteJSON(new CountsResponseData
                {
                    epoch = returnEpoch,
                    count = count
                });
                return;
            }
            
            //Get URL params
            int skip = GetIntFromQuery("skip", 0);
            int limit = GetIntFromQuery("limit", int.MaxValue);

            //Find
            var response = await GetMongoCollection().FindAsync(filter, new FindOptions<T, T>
            {
                Limit = limit,
                Skip = skip
            });
            var responseList = await response.ToListAsync();

            //Write
            await WriteResponse(responseList, returnEpoch, format);
        }

        public abstract IMongoCollection<T> GetMongoCollection();

        public abstract FilterDefinition<T> GetFilterDefinition(DateTime epoch);

        public abstract O ConvertToOutputType(T data);

        public virtual async Task WriteResponse(List<T> adds, int epoch, string format)
        {
            //Verify JSON response
            if(format != "json")
            {
                await ExitInvalidFormat("json");
                return;
            }
            await WriteJSONResponse(adds, epoch);
        }

        public async Task WriteJSONResponse(List<T> adds, int epoch)
        {
            //Create response template
            ResponseData r = new ResponseData
            {
                content = MassConvertObjects(adds),
                epoch = epoch
            };

            //Write
            await WriteJSON(r);
        }

        public async Task ExitInvalidFormat(params string[] formats)
        {
            await WriteString("Invalid format. Valid formats are: " + JsonConvert.SerializeObject(formats) + ". Or, you can use 'counts_only'.", "text/plain", 400);
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
            public List<O> content;
            public int epoch;
        }

        class CountsResponseData
        {
            public long count;
            public int epoch;
        }
    }
}
