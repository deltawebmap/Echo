using System;
using System.Collections.Generic;
using System.Text;

namespace EchoEntities.Db
{
    /// <summary>
    /// Contians item data. Can be stored directly in the table
    /// </summary>
    public class DbItem : DbBase
    {
        /// <summary>
        /// The ID of the parent over this
        /// </summary>
        public string parent_id { get; set; }

        /// <summary>
        /// The parent of this
        /// </summary>
        public DbInventoryParentType parent_type { get; set; }

        /// <summary>
        /// The name of the item, for example PrimalItemResource_Hide_C. Never null
        /// </summary>
        public string classname { get; set; }

        /// <summary>
        /// Ark item id, maybe for net? Never null
        /// </summary>
        public ulong item_id { get; set; }

        /// <summary>
        /// Number of items in this stack.
        /// </summary>
        public int stack_size { get; set; }

        /// <summary>
        /// Last time the durability was decreased. Never null
        /// </summary>
        public double last_durability_decrease_time { get; set; }

        /// <summary>
        /// If this is a in-game Ark blueprint (not to be confused with an UE blueprint)
        /// </summary>
        public bool is_blueprint { get; set; }

        /// <summary>
        /// If this is an engram. Appears in Argentavis inventory. Not really sure why this is here...
        /// </summary>
        public bool is_engram { get; set; }

        /// <summary>
        /// Durability of this item, ranging from 0-1
        /// </summary>
        public float saved_durability { get; set; }

        /// <summary>
        /// Name of the user that crafted this item, if it was crafted
        /// </summary>
        public string crafter_name { get; set; }

        /// <summary>
        /// Name of the tribe that crafted this item, if it was crafted
        /// </summary>
        public string crafter_tribe { get; set; }
    }
}
