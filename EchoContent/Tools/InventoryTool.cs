using EchoContent.Entities.Inventory;
using LibDeltaSystem;
using LibDeltaSystem.Db.Content;
using LibDeltaSystem.Db.System;
using LibDeltaSystem.Entities.ArkEntries.Dinosaur;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EchoContent.Tools
{
    public static class InventoryTool
    {
        public static async Task<WebInventory> GetWebInventory(List<DbItem> items, DeltaPrimalDataPackage package, DbServer server, int tribeId)
        {
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

            //Convert items
            List<WebInventoryItem> export = new List<WebInventoryItem>();
            foreach(var i in items)
            {
                //Create base
                WebInventoryItem item = new WebInventoryItem
                {
                    classname = i.classname,
                    is_blueprint = i.is_blueprint,
                    is_engram = i.is_engram,
                    item_id = i.item_id.ToString(),
                    saved_durability = i.saved_durability,
                    stack_size = i.stack_size,
                    type = "GENERIC",
                    extras = new WebInventoryItemExtraBase()
                };

                //Check if this is a different type
                if(i.custom_data_name == "CRYOPOD")
                {
                    //Attempt to lookup this dino
                    DbDino d = await DbDino.GetDinosaurByID(Program.conn, ulong.Parse(i.custom_data_value), server);
                    DinosaurEntry dd = null;
                    if (d != null)
                        dd = package.GetDinoEntry(d.classname);
                    if(dd != null)
                    {
                        item.type = "CRYOPOD";
                        item.extras = new WebInventoryItemExtraCryopod
                        {
                            id = d.dino_id.ToString(),
                            img = dd.icon.image_url,
                            level = d.level,
                            name = d.tamed_name,
                            species = dd.screen_name,
                            classname = d.classname
                        };
                    }
                }

                //Add
                export.Add(item);
            }

            //Create inventory to return
            return new WebInventory
            {
                inventory_items = export,
                item_class_data = itemData
            };
        }
    }
}
