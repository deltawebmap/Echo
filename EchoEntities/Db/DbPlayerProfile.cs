using System;
using System.Collections.Generic;
using System.Text;

namespace EchoEntities.Db
{
    public class DbPlayerProfile : DbBase
    {
        /// <summary>
        /// Player Steam name
        /// </summary>
        public string name { get; set; }

        /// <summary>
        /// In game name
        /// </summary>
        public string ig_name { get; set; }

        /// <summary>
        /// Ark player ID
        /// </summary>
        public ulong ark_id { get; set; }

        /// <summary>
        /// The Steam ID used for this character
        /// </summary>
        public string steam_id { get; set; }
    }
}
