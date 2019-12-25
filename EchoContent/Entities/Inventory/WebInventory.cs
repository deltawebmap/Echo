using LibDeltaSystem.Entities.ArkEntries.Dinosaur;
using System;
using System.Collections.Generic;
using System.Text;

namespace EchoContent.Entities.Inventory
{
    public class WebInventory
    {
        public List<WebInventoryItem> inventory_items;
        public Dictionary<string, ItemEntry> item_class_data = new Dictionary<string, ItemEntry>();
    }
}
