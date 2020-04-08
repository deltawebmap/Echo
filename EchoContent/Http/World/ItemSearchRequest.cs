using LibDeltaSystem.Db.System;
using LibDeltaSystem.Db.Content;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Driver;
using EchoContent.Exceptions;
using System.Linq;
using MongoDB.Bson;
using LibDeltaSystem;
using LibDeltaSystem.Entities.ArkEntries.Dinosaur;
using LibDeltaSystem.Entities.ArkEntries;
using System.Text.RegularExpressions;
using EchoContent.Tools;
using Microsoft.AspNetCore.Http;

namespace EchoContent.Http.World
{
    public class ItemSearchRequest : EchoTribeDeltaService
    {
        public const int PAGE_SIZE = 25;

        public ItemSearchRequest(DeltaConnection conn, HttpContext e) : base(conn, e)
        {
        }

        public override async Task OnRequest()
        {
            //Find all entries that could match
            string query = e.Request.Query["q"].ToString().ToLower();
            int index = -1;
            int found = 0;
            List<WebArkInventoryItemResult> itemsResponse = new List<WebArkInventoryItemResult>(); //Defines an item on the list. These should be unique
            Dictionary<int, Dictionary<string, WebArkInventoryHolder>> inventories = new Dictionary<int, Dictionary<string, WebArkInventoryHolder>>(); //Defines the actual object an item stack is located in
            List<string> foundClassnames = new List<string>(); //List of classnames we've processed

            //Add defaults
            inventories.Add(0, new Dictionary<string, WebArkInventoryHolder>());
            inventories.Add(1, new Dictionary<string, WebArkInventoryHolder>());

            //Now, find all inventories owning these
            var results = await GetItemsStreamed(query);
            bool finished = false;
            int calls = 0;
            while(await results.MoveNextAsync() && !finished)
            {
                calls++;
                foreach (var i in results.Current)
                {
                    //Check if this is over the page limit
                    if(!foundClassnames.Contains(i.classname))
                    {
                        //Check if we're over the page limit
                        if(foundClassnames.Count >= PAGE_SIZE)
                        {
                            finished = true;
                            break;
                        }

                        //Add to found
                        foundClassnames.Add(i.classname);
                    }
                    
                    //Find or create an item result for this
                    var itemStack = itemsResponse.Where(x => x.item_classname == i.classname).FirstOrDefault();
                    if (itemStack == null)
                    {
                        //Get item entry
                        ItemEntry r = await package.GetItemEntryByClssnameAsnyc(i.classname);
                        if (r == null)
                            continue;

                        //Create item data
                        itemStack = new WebArkInventoryItemResult
                        {
                            item_classname = r.classname,
                            item_displayname = r.name,
                            item_icon = r.icon.image_url,
                            owner_inventories = new List<ArkItemSearchResultsInventory>(),
                            total_count = 0
                        };
                        itemsResponse.Add(itemStack);
                    }
                    List<ArkItemSearchResultsInventory> inventoryRefs = itemStack.owner_inventories; //Defines where an item stack is located, maps to an inventory ID

                    //Check if we already have an inventory reference to this parent
                    var existingRefs = inventoryRefs.Where(x => x.id == i.parent_id && x.type == i.parent_type).ToArray();
                    if (existingRefs.Length == 1)
                    {
                        //We'll add to the stack size here
                        existingRefs[0].count += i.stack_size;
                        itemStack.total_count += i.stack_size;
                        continue;
                    }

                    //Add the inventory holder data
                    if (!inventories[(int)i.parent_type].ContainsKey(i.parent_id))
                    {
                        //Now, we know we don't have an inventory registered yet for this holder
                        //Get the data of the holder we're about to register
                        WebArkInventoryHolder holder = await GetInventoryHolder(i);
                        inventories[(int)i.parent_type].Add(i.parent_id, holder);
                    }

                    //Add the reference data for this object
                    inventoryRefs.Add(new ArkItemSearchResultsInventory
                    {
                        count = i.stack_size,
                        id = i.parent_id,
                        type = i.parent_type
                    });

                    //Add to count
                    itemStack.total_count += i.stack_size;
                }
            }

            //Now that all items have been found, loop through them and sort their inventory refs by stack size
            foreach(var i in itemsResponse)
            {
                i.owner_inventories.Sort(new Comparison<ArkItemSearchResultsInventory>((x, y) =>
                {
                    return y.count.CompareTo(x.count);
                }));
            }

            //Now, produce an output
            WebArkInventoryItemReply output = new WebArkInventoryItemReply
            {
                inventories = inventories,
                items = itemsResponse,
                more = false,
                page_offset = 0,
                query = e.Request.Query["q"],
                total_item_count = 0
            };

            //Write
            await WriteJSON(output);
        }

        private async Task<WebArkInventoryHolder> GetInventoryHolder(DbItem i)
        {
            switch(i.parent_type)
            {
                case DbInventoryParentType.Dino:
                    return await GetDinoInventoryHolder(i);
                case DbInventoryParentType.Structure:
                    return await GetStructureInventoryHolder(i);
                default:
                    return null;
            }
        }

        private async Task<WebArkInventoryDino> GetDinoInventoryHolder(DbItem i)
        {
            //Get dino
            DbDino dino = await DbDino.GetDinosaurByID(Program.conn, ulong.Parse(i.parent_id), server);
            if (dino == null)
                return null;

            //Get the dino entry
            DinosaurEntry entry = package.GetDinoEntry(dino.classname);
            if (entry == null)
                return null;

            //Add dino data to inventory
            return new WebArkInventoryDino
            {
                displayClassName = entry.screen_name,
                displayName = dino.tamed_name,
                id = dino.dino_id.ToString(),
                img = entry.icon.image_thumb_url,
                level = dino.level
            };
        }

        private async Task<WebArkInventoryStructure> GetStructureInventoryHolder(DbItem i)
        {
            //Get structure
            DbStructure structure = await DbStructure.GetStructureByID(Program.conn, int.Parse(i.parent_id), server);
            if (structure == null)
                return null;

            //Attempt to get info about this structure
            string name = structure.classname;
            string subname = structure.classname;
            string icon = ""; //TODO
            if(structure.TryGetItemEntry(Program.conn, package, out ItemEntry entry))
            {
                name = entry.name;
                icon = entry.icon.image_thumb_url;
            }

            //Switch the names if there is a custom name applied
            bool useCustomName = structure.custom_name != null;
            if (useCustomName)
                useCustomName = structure.custom_name.Length > 0;
            if (useCustomName)
            {
                subname = name;
                name = structure.custom_name;
            }

            //Add dino data to inventory
            return new WebArkInventoryStructure
            {
                displayClassName = subname,
                displayName = name,
                id = structure.structure_id.ToString(),
                img = icon,
                item_count = structure.max_item_count,
                location = structure.location
            };
        }

        private static async Task<DbDino> GetDinosaur(string id, DbServer server)
        {
            var filterBuilder = Builders<DbDino>.Filter;
            var filter = filterBuilder.Eq("_id", ObjectId.Parse(id));
            var response = await Program.conn.content_dinos.FindAsync(filter);
            var dino = await response.FirstOrDefaultAsync();
            if (dino == null)
                throw new StandardError("Dinosaur not found.", "This dinosaur ID is invalid.", 404);
            return dino;
        }

        private async Task<IAsyncCursor<DbItem>> GetItemsStreamed(string query, int limit = int.MaxValue)
        {
            var sortBuilder = Builders<DbItem>.Sort;
            var filterBuilder = Builders<DbItem>.Filter;
            var filter = GetServerTribeFilter<DbItem>() & filterBuilder.Regex("entry_display_name", $"(?i)({Regex.Escape(query)})");
            var response = await Program.conn.content_items.FindAsync(filter, new FindOptions<DbItem, DbItem>
            {
                Limit = limit,
                Sort = sortBuilder.Ascending("classname"),
                BatchSize = 30,
            });
            return response;
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
            public string id; //parent inventory id
        }

        public class WebArkInventoryHolder
        {
            //Represents a holder of a inventory
            public string id;
            public string displayName;
            public string displayClassName;
            public string img;
        }

        public class WebArkInventoryDino : WebArkInventoryHolder
        {
            public int level;
        }

        public class WebArkInventoryStructure : WebArkInventoryHolder
        {
            public int item_count;
            public DbLocation location;
        }
    }
}
