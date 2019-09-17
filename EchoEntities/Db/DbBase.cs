using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Text;

namespace EchoEntities.Db
{
    /// <summary>
    /// Contains some basic data that will always be written to the database
    /// </summary>
    public class DbBase
    {
        /// <summary>
        /// ID used internally that shouldn't be touched by us
        /// </summary>
        [BsonIgnoreIfDefault]
        public ObjectId _id { get; set; }

        /// <summary>
        /// Server this dinosaur belongs to
        /// </summary>
        public string server_id { get; set; }

        /// <summary>
        /// The tribe ID this dinosaur belongs to
        /// </summary>
        public int tribe_id { get; set; }

        /// <summary>
        /// Version control
        /// </summary>
        public uint revision_id { get; set; }
    }
}
