using EchoContent.Entities.Inventory;
using EchoContent.Exceptions;
using LibDeltaSystem;
using LibDeltaSystem.Db.Content;
using LibDeltaSystem.Db.System;
using LibDeltaSystem.Entities.ArkEntries;
using LibDeltaSystem.Entities.ArkEntries.Dinosaur;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EchoContent.Http.World
{
    public static class StructureInfoRequest
    {
        public static async Task OnHttpRequest(Microsoft.AspNetCore.Http.HttpContext e, DbServer server, DbUser user, int? tribeId, ArkMapEntry mapInfo, DeltaPrimalDataPackage package)
        {
            //Get dino ID from URL
            string structureIdString = e.Request.Path.ToString().Split('/')[5];
            if (!int.TryParse(structureIdString, out int id))
                throw new StandardError("This is an invalid structure ID.", "Could not parse as int.", 400);

            //Get structure
            DbStructure structure = await DbStructure.GetStructureByID(Program.conn, id, server);
            if (structure == null)
                throw new StandardError("This structure does not exist.", "This ID is not valid.", 400);
            if (structure.tribe_id != tribeId && tribeId.HasValue)
                throw new StandardError("This structure does not belong to you.", "You may not access this structure.", 400);

            //Get structure items and inventory
            List<DbItem> items = await structure.GetItems(Program.conn, server);
            WebInventory inventory = await Tools.InventoryTool.GetWebInventory(items, package, server, tribeId);

            //Attempt to get info about this structure
            string name = structure.classname;
            string subname = structure.classname;
            string icon = ""; //TODO
            if (structure.TryGetItemEntry(Program.conn, package, out ItemEntry entry))
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

            //Create response
            ResponseStructure response = new ResponseStructure
            {
                id = structure.structure_id,
                inventory = inventory,
                location = structure.location,
                current_item_count = structure.current_item_count,
                max_item_count = structure.max_item_count,
                max_health = structure.max_health,
                current_health = structure.current_health,
                custom_name = structure.custom_name,
                tribe_id = structure.tribe_id,
                name = name,
                subname = subname,
                icon = icon
            };

            //Send
            await Program.QuickWriteJsonToDoc(e, response);
        }

        class ResponseStructure
        {
            public int id;
            public WebInventory inventory;
            public DbLocation location;
            public int current_item_count;
            public int max_item_count;
            public float max_health;
            public float current_health;
            public string custom_name;
            public int tribe_id;
            public string name;
            public string subname;
            public string icon;
        }
    }
}
