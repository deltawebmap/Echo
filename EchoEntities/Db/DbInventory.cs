using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace EchoEntities.Db
{
    /// <summary>
    /// Inventory object stored in the database.
    /// </summary>
    public class DbInventory
    {
        /// <summary>
        /// ID used internally that shouldn't be touched by us
        /// </summary>
        [BsonIgnoreIfDefault]
        public object _id { get; set; }

        /// <summary>
        /// Server this dinosaur belongs to
        /// </summary>
        public string server_id { get; set; }

        /// <summary>
        /// The tribe ID this dinosaur belongs to
        /// </summary>
        public int tribe_id { get; set; }

        /// <summary>
        /// The ID of the parent over this
        /// </summary>
        public string parent_id { get; set; }

        /// <summary>
        /// The parent of this
        /// </summary>
        public DbInventoryParentType parent_type { get; set; }

        /// <summary>
        /// The actual items
        /// </summary>
        public List<DbItem> items { get; set; }
    }

    public enum DbInventoryParentType
    {
        Dino
    }
}
