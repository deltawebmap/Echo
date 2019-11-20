﻿using ArkSaveEditor.Entities;
using LibDeltaSystem.Db.System;
using LibDeltaSystem.Db.Content;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Driver;
using ArkSaveEditor;
using EchoContent.Exceptions;
using ArkSaveEditor.ArkEntries;
using LibDeltaSystem.Entities.ArkEntries.Dinosaur;
using LibDeltaSystem;

namespace EchoContent.Http.World
{
    public static class DinoInfoRequest
    {
        public static async Task OnHttpRequest(Microsoft.AspNetCore.Http.HttpContext e, DbServer server, DbUser user, int tribeId, ArkMapData mapInfo, DeltaPrimalDataPackage package)
        {
            //Get dino ID from URL
            string dinoIdString = e.Request.Path.ToString().Split('/')[5];
            if (!ulong.TryParse(dinoIdString, out ulong dinoId))
                throw new StandardError("This is an invalid dinosaur ID.", "Could not parse as ulong.", 400);

            //Get the dino
            DbDino dino = await GetDinosaur(dinoId, server, tribeId);

            //Get dino prefs
            var prefs = await dino.GetPrefs(Program.conn);

            //Get dinosaur entry
            DinosaurEntry dinoEntry = package.GetDinoEntry(dino.classname);

            //Find all inventory items
            List<DbItem> items = await GetItems(dino, server, tribeId);

            //Look up item classes
            Dictionary<string, ItemEntry> itemData = new Dictionary<string, ItemEntry>();
            foreach (var i in items)
            {
                //Get classname to tes
                string classname = i.classname;

                //Check if we already have item data for this
                if (itemData.ContainsKey(classname))
                    continue;

                //Search for it
                ItemEntry entry = package.GetItemEntry(classname);
                if (entry == null)
                    continue;

                //Add it
                itemData.Add(classname, entry);
            }

            //Respond with dinosaur data
            ResponseDino response = new ResponseDino
            {
                dino = dino,
                inventory_items = items,
                item_class_data = itemData,
                dino_entry = dinoEntry,
                prefs = prefs,
                max_stats = dino.max_stats,
                dino_id = dino.dino_id.ToString()
            };

            //Write
            await Program.QuickWriteJsonToDoc(e, response);
        }

        private static async Task<DbDino> GetDinosaur(ulong id, DbServer server, int tribeId)
        {
            var filterBuilder = Builders<DbDino>.Filter;
            var filter = filterBuilder.Eq("is_tamed", true) & filterBuilder.Eq("server_id", server.id) & filterBuilder.Eq("tribe_id", tribeId) & filterBuilder.Eq("dino_id", id);
            var response = await server.conn.content_dinos.FindAsync(filter);
            var dino = await response.FirstOrDefaultAsync();
            if (dino == null)
                throw new StandardError("Dinosaur not found.", "This dinosaur ID is invalid.", 404);
            return dino;
        }

        private static async Task<List<DbItem>> GetItems(DbDino dino, DbServer server, int tribeId)
        {
            var filterBuilder = Builders<DbItem>.Filter;
            var filter = filterBuilder.Eq("server_id", server.id) & filterBuilder.Eq("tribe_id", tribeId) & filterBuilder.Eq("parent_id", dino.dino_id) & filterBuilder.Eq("parent_type", DbInventoryParentType.Dino);
            var response = await server.conn.content_items.FindAsync(filter);
            var items = await response.ToListAsync();
            return items;
        }

        class ResponseDino
        {
            public DbDino dino;
            public List<DbItem> inventory_items;
            public Dictionary<string, ItemEntry> item_class_data = new Dictionary<string, ItemEntry>();
            public DbArkDinosaurStats max_stats;
            public DinosaurEntry dino_entry;
            public LibDeltaSystem.Db.System.Entities.SavedDinoTribePrefs prefs;
            public string dino_id;
        }
    }
}
