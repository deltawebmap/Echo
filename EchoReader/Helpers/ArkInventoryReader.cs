﻿using ArkSaveEditor.Entities.LowLevel.DotArk;
using ArkSaveEditor.Entities.LowLevel.DotArk.ArkProperties;
using EchoEntities.Db;
using EchoReader.Entities;
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

        private static List<DbItem> ReadItemsFromRefs(List<ObjectProperty> objs, ArkSaveEditor.Deserializer.DotArk.DotArkDeserializer deser, ObjectId parent, DbInventoryParentType parentType, int tribeId, string serverId, uint revisionId)
        {
            List<DbItem> items = new List<DbItem>();
            foreach (var o in objs)
            {
                //Read
                var reader = new ArkPropertyReader(o.gameObjectRef.ReadPropsFromFile(deser));
                var obj = ReadItem(o.gameObjectRef, reader);

                //Set some database info
                obj.parent_id = parent.ToString();
                obj.parent_type = parentType;
                obj.tribe_id = tribeId;
                obj.server_id = serverId;
                obj.revision_id = revisionId;

                //Add
                items.Add(obj);
            }
            return items;
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
        public static void SaveInventory(DotArkGameObject obj, ArkPropertyReader reader, ArkSaveEditor.Deserializer.DotArk.DotArkDeserializer deser, ObjectId parent, DbInventoryParentType parentType, int tribeId, string serverId, uint revisionId)
        {
            //Get inventory items
            List<DbItem> inventoryItems;
            if (!reader.HasProperty("InventoryItems"))
                return;
            else
                inventoryItems = ReadItemsFromRefs(((ArrayProperty<ObjectProperty>)reader.GetSingleProperty("InventoryItems")).items, deser, parent, parentType, tribeId, serverId, revisionId);

            //Stop if there are no actions
            if (inventoryItems.Count == 0)
                return;

            //Insert all
            Program.content_items.InsertMany(inventoryItems);
        }
    }
}
