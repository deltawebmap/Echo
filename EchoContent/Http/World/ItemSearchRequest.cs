using ArkSaveEditor.Entities;
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
using System.Linq;
using MongoDB.Bson;
using LibDeltaSystem;
using LibDeltaSystem.Entities.ArkEntries.Dinosaur;

namespace EchoContent.Http.World
{
    public static class ItemSearchRequest
    {
        public const int PAGE_SIZE = 25;

        public static async Task OnHttpRequest(Microsoft.AspNetCore.Http.HttpContext e, DbServer server, DbUser user, int tribeId, ArkMapData mapInfo, DeltaPrimalDataPackage package)
        {
            //Find all entries that could match
            string query = e.Request.Query["q"].ToString().ToLower();
            int index = -1;
            int found = 0;
            List<WebArkInventoryItemResult> itemsResponse = new List<WebArkInventoryItemResult>();
            Dictionary<int, Dictionary<string, WebArkInventoryHolder>> inventories = new Dictionary<int, Dictionary<string, WebArkInventoryHolder>>();
            inventories.Add(0, new Dictionary<string, WebArkInventoryHolder>());
            int allTotalCount = 0;
            foreach (var r in package.item_entries)
            {
                //Add to index
                index++;
                
                //Check if this matches
                if (!r.name.ToLower().Contains(query) && query.Length != 0)
                    continue;

                //Check if we're beyond page size
                if (found > PAGE_SIZE)
                    break;

                //Get items for this
                List<DbItem> items = await GetItems(server, tribeId, r.classname);

                //Stop if there were no items found
                if (items.Count == 0)
                    continue;
                found++;

                //Now, find all inventories owning this
                List<ArkItemSearchResultsInventory> inventoryRefs = new List<ArkItemSearchResultsInventory>();
                int totalCount = 0;
                foreach(var i in items)
                {
                    //Add to total
                    totalCount += i.stack_size;
                    
                    //Check if we already have data for this. If we do, just add to the count
                    var existingRefs = inventoryRefs.Where(x => x.id == i.parent_id && x.type == i.parent_type).ToArray();
                    if(existingRefs.Length == 1)
                    {
                        existingRefs[0].count += i.stack_size;
                        continue;
                    }
                    
                    //Get dino
                    DbDino dino = await GetDinosaurByToken(i.parent_id, server);
                    if (dino == null)
                        continue;

                    //Get the dino entry
                    DinosaurEntry entry = package.GetDinoEntry(dino.classname);
                    if (entry == null)
                        continue;

                    //Convert dino
                    inventoryRefs.Add(new ArkItemSearchResultsInventory
                    {
                        count = i.stack_size,
                        id = i.parent_id,
                        type = DbInventoryParentType.Dino
                    });

                    //Add dino data to inventory
                    if (!inventories[(int)i.parent_type].ContainsKey(i.parent_id))
                    {
                        inventories[(int)i.parent_type].Add(i.parent_id, new WebArkInventoryDino
                        {
                            displayClassName = entry.screen_name,
                            displayName = dino.tamed_name,
                            id = dino.dino_id.ToString(),
                            img = entry.icon.image_thumb_url,
                            level = dino.level
                        });
                    }
                }

                //Add inventory result
                itemsResponse.Add(new WebArkInventoryItemResult
                {
                    item_classname = r.classname,
                    item_displayname = r.name,
                    item_icon = r.icon.image_url,
                    owner_inventories = inventoryRefs,
                    total_count = totalCount
                });
                allTotalCount += totalCount;
            }

            //Now, produce an output
            WebArkInventoryItemReply output = new WebArkInventoryItemReply
            {
                inventories = inventories,
                items = itemsResponse,
                more = false,
                page_offset = 0,
                query = e.Request.Query["q"],
                total_item_count = allTotalCount
            };

            //Write
            await Program.QuickWriteJsonToDoc(e, output);
        }

        private static async Task<DbDino> GetDinosaur(string id, DbServer server)
        {
            var filterBuilder = Builders<DbDino>.Filter;
            var filter = filterBuilder.Eq("_id", ObjectId.Parse(id));
            var response = await server.conn.content_dinos.FindAsync(filter);
            var dino = await response.FirstOrDefaultAsync();
            if (dino == null)
                throw new StandardError("Dinosaur not found.", "This dinosaur ID is invalid.", 404);
            return dino;
        }

        private static async Task<DbDino> GetDinosaurByToken(string token, DbServer server)
        {
            var filterBuilder = Builders<DbDino>.Filter;
            var filter = filterBuilder.Eq("token", token);
            var response = await server.conn.content_dinos.FindAsync(filter);
            var dino = await response.FirstOrDefaultAsync();
            if (dino == null)
                throw new StandardError("Dinosaur not found.", "This dinosaur ID is invalid.", 404);
            return dino;
        }

        private static async Task<List<DbItem>> GetItems(DbServer server, int tribeId, string classname)
        {
            var sortBuilder = Builders<DbItem>.Sort;
            var filterBuilder = Builders<DbItem>.Filter;
            var filter = filterBuilder.Eq("server_id", server.id) & filterBuilder.Eq("tribe_id", tribeId) & filterBuilder.Eq("classname", classname);
            var response = await server.conn.content_items.FindAsync(filter, new FindOptions<DbItem, DbItem>
            {
                
            });
            var items = await response.ToListAsync();
            return items;
        }

        class WebArkInventoryItemReply
        {
            public List<WebArkInventoryItemResult> items;
            public bool more; //Do more exist?
            public string query;
            public int page_offset;
            public int total_item_count; //Total inventory count, even if it isn't sent on this page.
            public Dictionary<int, Dictionary<string, WebArkInventoryHolder>> inventories;
        }

        class WebArkInventoryItemResult
        {
            public string item_classname;
            public string item_displayname;
            public string item_icon;
            public int total_count;
            public List<ArkItemSearchResultsInventory> owner_inventories;
        }

        public class ArkItemSearchResultsInventory
        {
            public DbInventoryParentType type;
            public int count;
            public string id;
        }

        public abstract class WebArkInventoryHolder
        {
            //Represents a holder of a inventory
        }

        public class WebArkInventoryDino : WebArkInventoryHolder
        {
            public string id;
            public string displayName;
            public string displayClassName;
            public string img;
            public int level;
        }
    }
}
