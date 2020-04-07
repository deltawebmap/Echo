using LibDeltaSystem.Db.System;
using LibDeltaSystem.Db.Content;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Driver;
using EchoContent.Exceptions;
using LibDeltaSystem.Entities.ArkEntries.Dinosaur;
using LibDeltaSystem;
using EchoContent.Entities.Inventory;
using LibDeltaSystem.Entities.ArkEntries;
using LibDeltaSystem.Tools;
using Microsoft.AspNetCore.Http;

namespace EchoContent.Http.World
{
    public class DinoInfoRequest : EchoTribeDeltaService
    {
        public ulong dino_id;
        
        public DinoInfoRequest(DeltaConnection conn, HttpContext e) : base(conn, e)
        {
        }

        public override async Task<bool> SetArgs(Dictionary<string, string> args)
        {
            if (!await base.SetArgs(args))
                return false;
            if(!ulong.TryParse(args["DINO"], out dino_id))
            {
                await WriteString("This is not a valid dinosaur ID", "text/plain", 400);
                return false;
            }
            return true;
        }

        public override async Task OnRequest()
        {
            //Get the dino
            DbDino dino = await GetDinosaur(dino_id);

            //Get dino prefs
            var prefs = dino.prefs;

            //Get dinosaur entry
            DinosaurEntry dinoEntry = await package.GetDinoEntryByClssnameAsnyc(dino.classname);

            //Find all inventory items
            List<DbItem> items = await GetItems(dino);
            WebInventory inventory = await Tools.InventoryTool.GetWebInventory(items, package, server, tribeId);

            //Respond with dinosaur data
            ResponseDino response = new ResponseDino
            {
                dino = dino,
                inventory = inventory,
                dino_entry = dinoEntry,
                prefs = prefs,
                dino_id = dino.dino_id.ToString()
            };

            //Write
            await WriteJSON(response);
        }

        private async Task<DbDino> GetDinosaur(ulong id)
        {
            var filterBuilder = Builders<DbDino>.Filter;
            var filter = filterBuilder.Eq("is_tamed", true) & FilterBuilderToolDb.CreateTribeFilter<DbDino>(server, tribeId) & filterBuilder.Eq("dino_id", id);
            var response = await Program.conn.content_dinos.FindAsync(filter);
            var dino = await response.FirstOrDefaultAsync();
            if (dino == null)
                throw new StandardError("Dinosaur not found.", "This dinosaur ID is invalid.", 404);
            return dino;
        }

        private async Task<List<DbItem>> GetItems(DbDino dino)
        {
            var filterBuilder = Builders<DbItem>.Filter;
            var filter = FilterBuilderToolDb.CreateTribeFilter<DbItem>(server, tribeId) & filterBuilder.Eq("parent_id", dino.dino_id) & filterBuilder.Eq("parent_type", DbInventoryParentType.Dino);
            var response = await Program.conn.content_items.FindAsync(filter);
            var items = await response.ToListAsync();
            return items;
        }

        class ResponseDino
        {
            public DbDino dino;
            public WebInventory inventory;
            public DbArkDinosaurStats max_stats;
            public DinosaurEntry dino_entry;
            public LibDeltaSystem.Db.System.Entities.SavedDinoTribePrefs prefs;
            public string dino_id;
        }
    }
}
