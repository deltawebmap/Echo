using LibDeltaSystem.Db.Content;
using LibDeltaSystem.Db.System;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using LibDeltaSystem;
using LibDeltaSystem.Entities.ArkEntries.Dinosaur;
using LibDeltaSystem.Tools;

namespace EchoContent.Http.World
{
    public static class DinoListRequest
    {
        public static async Task OnHttpRequest(Microsoft.AspNetCore.Http.HttpContext e, DbServer server, DbUser user, int? tribeId, DeltaPrimalDataPackage package)
        {
            //Get vars
            int limit = 30;
            if (e.Request.Query.ContainsKey("limit"))
                limit = int.Parse(e.Request.Query["limit"]);
            int page = 0;
            if (e.Request.Query.ContainsKey("page"))
                page = int.Parse(e.Request.Query["page"]);

            //Optionally, we can post an array of classnames we don't need entries for. If that was sent, use it
            List<string> used_classnames = new List<string>();
            if (Program.FindRequestMethod(e) == RequestHttpMethod.post)
                used_classnames = Program.DecodePostBody<List<string>>(e);

            //Find
            var filterBuilder = Builders<DbDino>.Filter;
            var filter = filterBuilder.Eq("is_tamed", true) & FilterBuilderToolDb.CreateTribeFilter<DbDino>(server, tribeId);
            var response = await server.conn.content_dinos.FindAsync(filter, new FindOptions<DbDino, DbDino>
            {
                Limit = limit,
                Skip = page * limit
            });
            var responseList = await response.ToListAsync();

            //Find dino entries
            Dictionary<string, DinosaurEntry> entries = new Dictionary<string, DinosaurEntry>();
            foreach(var d in responseList)
            {
                //If the classname was in the whitelist, skip
                if (used_classnames.Contains(d.classname))
                    continue;

                //Get
                DinosaurEntry entry = await package.GetDinoEntryByClssnameAsnyc(d.classname);
                if (entry == null)
                    continue;

                //Add
                used_classnames.Add(d.classname);
                entries.Add(d.classname, entry);
            }

            //Get tribe ID string
            string tribeIdString;
            if (tribeId == null)
                tribeIdString = "*";
            else
                tribeIdString = tribeId.Value.ToString();

            //Create response
            ResponseData r = new ResponseData
            {
                limit = limit,
                page = page,
                next = Program.ROOT_URL + "/" + server.id + "/tribes/" + tribeIdString + "/dino_stats?limit=" + limit + "&page=" + (page + 1),
                dinos = responseList,
                dino_entries = entries,
                registered_classnames = used_classnames
            };

            //Write
            await Program.QuickWriteJsonToDoc(e, r);
        }

        class ResponseData
        {
            public string next;
            public int limit;
            public int page;
            public List<DbDino> dinos;
            public Dictionary<string, DinosaurEntry> dino_entries;
            public List<string> registered_classnames;
        }
    }
}
