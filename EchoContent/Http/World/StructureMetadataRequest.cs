using LibDeltaSystem;
using LibDeltaSystem.Entities.DynamicTiles;
using LibDeltaSystem.WebFramework.ServiceTemplates;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EchoContent.Http.World
{
    public class StructureMetadataRequest : BasicDeltaService
    {
        public StructureMetadataRequest(DeltaConnection conn, HttpContext e) : base(conn, e)
        {
        }

        public override async Task OnRequest()
        {
            //Simply write the structure metadata
            await WriteJSON(new MetadataResponse
            {
                metadata = Program.conn.GetStructureMetadata(),
                image_url = "https://icon-assets.deltamap.net/structures.png"
            });
        }

        class MetadataResponse
        {
            public List<StructureMetadata> metadata;
            public string image_url;
        }
    }
}
