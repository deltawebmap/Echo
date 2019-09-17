using System;
using System.Collections.Generic;
using System.Text;

namespace EchoReader.Entities
{
    /// <summary>
    /// Ark server saved to disk.
    /// </summary>
    public class ArkServerSerialized
    {
        /// <summary>
        /// The ARK server ID
        /// </summary>
        public string _id { get; set; }

        /// <summary>
        /// Content uploaded
        /// </summary>
        public List<ArkUploadedFile> files { get; set; }

        /// <summary>
        /// Used for version control
        /// </summary>
        public uint revision_id { get; set; }
    }
}
