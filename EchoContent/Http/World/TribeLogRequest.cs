using LibDeltaSystem.Db.Content;
using LibDeltaSystem.Db.System;
using LibDeltaSystem.Tools;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EchoContent.Http.World
{
    public static class TribeLogRequest
    {
        public static async Task OnHttpRequest(Microsoft.AspNetCore.Http.HttpContext e, DbServer server, DbUser user, int? tribeId)
        {
            //Get vars
            int limit = 200;
            if (e.Request.Query.ContainsKey("limit"))
                limit = int.Parse(e.Request.Query["limit"]);
            int page = 0;
            if (e.Request.Query.ContainsKey("page"))
                page = int.Parse(e.Request.Query["page"]);

            //Query DB
            var filterBuilder = Builders<DbTribeLogEntry>.Filter;
            var filter = FilterBuilderToolDb.CreateTribeFilter<DbTribeLogEntry>(server, tribeId);
            var results = await Program.conn.content_tribe_log.FindAsync(filter, new FindOptions<DbTribeLogEntry, DbTribeLogEntry>
            {
                Sort = Builders<DbTribeLogEntry>.Sort.Descending("index"),
                Limit = limit,
                Skip = page * limit
            });

            //Get tribe ID string
            string tribeIdString;
            if (tribeId == null)
                tribeIdString = "*";
            else
                tribeIdString = tribeId.Value.ToString();

            //Create a response and write it
            ResponseData response = new ResponseData
            {
                results = await results.ToListAsync(),
                next = Program.ROOT_URL + "/" + server.id + "/tribes/" + tribeIdString + "/logs?limit=" + limit + "&page=" + (page + 1)
            };
            await Program.QuickWriteJsonToDoc(e, response);
        }

        class ResponseData
        {
            public List<DbTribeLogEntry> results;
            public string next;
        }
    }
}
