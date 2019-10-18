using ArkSaveEditor.Entities.LowLevel.DotArk;
using ArkSaveEditor.Entities.LowLevel.DotArk.ArkProperties;
using EchoReader.Entities;
using LibDeltaSystem.Db.Content;
using LibDeltaSystem.Entities;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Text;

namespace EchoReader.Helpers
{
    public static class ArkInventoryReader
    {
        public static DbItem ReadItem(DotArkGameObject obj, ArkPropertyReader reader)
        {
            //Read some basic properties first
            DbItem item = new DbItem();

            //Read properties
            item.classname = obj.classname.classname;
            item.stack_size = 1;
            if (reader.HasProperty("ItemQuantity"))
                item.stack_size = reader.GetInt32Property("ItemQuantity");

            //Convert ItemID
            ArkStructProps itemIdStruct = (ArkStructProps)((StructProperty)reader.GetSingleProperty("ItemId")).structData;
            byte[] buf = new byte[8];
            BitConverter.GetBytes((UInt32)itemIdStruct.props_string["ItemID1"].data).CopyTo(buf, 0);
            BitConverter.GetBytes((UInt32)itemIdStruct.props_string["ItemID2"].data).CopyTo(buf, 4);
            item.item_id = BitConverter.ToUInt64(buf, 0);

            //Read booleans
            item.is_blueprint = reader.GetBooleanProperty("bIsBlueprint");
            item.is_engram = reader.GetBooleanProperty("bIsEngram");

            //Read props that may not exist
            if (reader.CheckIfValueExists("SavedDurability"))
                item.saved_durability = reader.GetFloatProperty("SavedDurability");
            else
                item.saved_durability = 1;

            if (reader.CheckIfValueExists("CrafterCharacterName"))
                item.crafter_name = reader.GetStringProperty("CrafterCharacterName");
            else
                item.crafter_name = null;

            if (reader.CheckIfValueExists("CrafterTribeName"))
                item.crafter_tribe = reader.GetStringProperty("CrafterTribeName");
            else
                item.crafter_tribe = null;

            if (reader.CheckIfValueExists("LastAutoDurabilityDecreaseTime"))
                item.last_durability_decrease_time = (double)reader.GetDoubleProperty("LastAutoDurabilityDecreaseTime");
            else
                item.last_durability_decrease_time = -1;

            return item;
        }

        private static void ReadItemsFromRefs(List<ObjectProperty> objs, ArkSaveEditor.Deserializer.DotArk.DotArkDeserializer deser, string parent, DbInventoryParentType parentType, int tribeId, string serverId, DeltaContentDbSyncSession<DbItem> sync)
        {
            foreach (var o in objs)
            {
                //Read
                var reader = new ArkPropertyReader(o.gameObjectRef.ReadPropsFromFile(deser));
                var obj = ReadItem(o.gameObjectRef, reader);

                //Get the hash
                string hash = obj.GetHash();

                //Check if update is required
                if (!sync.CheckIfUpdateRequired(obj.item_id.ToString(), hash))
                    continue;

                //Set some database info
                obj.parent_id = parent.ToString();
                obj.parent_type = parentType;
                obj.tribe_id = tribeId;
                obj.server_id = serverId;
                obj.token = obj.item_id.ToString();

                //Add
                sync.UpdateOne(obj, obj.item_id.ToString(), hash).GetAwaiter().GetResult();
            }
        }

        /// <summary>
        /// Saves the inventory to the database
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="reader"></param>
        /// <param name="deser"></param>
        /// <param name="parentId"></param>
        /// <param name="parentType"></param>
        /// <param name="tribeId"></param>
        /// <param name="serverId"></param>
        /// <returns></returns>
        public static void SaveInventory(DotArkGameObject obj, ArkPropertyReader reader, ArkSaveEditor.Deserializer.DotArk.DotArkDeserializer deser, DeltaContentDbSyncSession<DbItem> sync, string parent, DbInventoryParentType parentType, int tribeId, string serverId)
        {
            if (reader.HasProperty("InventoryItems"))
                ReadItemsFromRefs(((ArrayProperty<ObjectProperty>)reader.GetSingleProperty("InventoryItems")).items, deser, parent, parentType, tribeId, serverId, sync);
        }
    }
}
