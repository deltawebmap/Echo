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
using LibDeltaSystem.Db.System.Entities;
using Microsoft.AspNetCore.Http;

namespace EchoContent.Http.World
{
    public class DinoListRequest : EchoTribeDeltaService
    {
        public DinoListRequest(DeltaConnection conn, HttpContext e) : base(conn, e)
        {
        }

        public override async Task OnRequest()
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
                used_classnames = await DecodePOSTBody<List<string>>();

            //Find
            var filterBuilder = Builders<DbDino>.Filter;
            var filter = filterBuilder.Eq("is_tamed", true) & GetServerTribeFilter<DbDino>();
            var response = await Program.conn.content_dinos.FindAsync(filter, new FindOptions<DbDino, DbDino>
            {
                Limit = limit,
                Skip = page * limit
            });
            var responseList = await response.ToListAsync();

            //Get number of documents
            long count = await Program.conn.content_dinos.CountDocumentsAsync(filter, new CountOptions
            {
                MaxTime = TimeSpan.FromSeconds(2)
            });

            //Find dino prefs
            var prefs = await Program.conn.MassGetDinoPrefs(server, responseList);

            //Create responses
            List<ResponseDino> responseDinos = new List<ResponseDino>();
            foreach(var d in responseList)
            {
                responseDinos.Add(new ResponseDino
                {
                    dino = d,
                    prefs = prefs[d.dino_id],
                    dino_id = d.dino_id.ToString()
                });
            }

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
                dinos = responseDinos,
                dino_entries = entries,
                registered_classnames = used_classnames,
                total = count
            };

            //Write
            await WriteJSON(r);
        }

        class ResponseData
        {
            public string next;
            public int limit;
            public int page;
            public long total;
            public List<ResponseDino> dinos;
            public Dictionary<string, DinosaurEntry> dino_entries;
            public List<string> registered_classnames;
        }

        class ResponseDino
        {
            public DbDino dino;
            public SavedDinoTribePrefs prefs;
            public string dino_id;
        }
    }
}
