using System;
using System.Collections.Generic;
using System.Text;

namespace EchoContent.Entities.Inventory
{
    public class WebInventoryItem
    {
        public string classname;
        public int stack_size;
        public bool is_blueprint;
        public bool is_engram;
        public string item_id;
        public float saved_durability;
        public string type; //GENERIC, CRYOPOD
        public WebInventoryItemExtraBase extras;
    }
}
