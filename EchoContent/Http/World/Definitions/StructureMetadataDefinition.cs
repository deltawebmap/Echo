using LibDeltaSystem;
using LibDeltaSystem.WebFramework;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace EchoContent.Http.World.Definitions
{
    public class StructureMetadataDefinition : DeltaWebServiceDefinition
    {
        public override string GetTemplateUrl()
        {
            return "/structure_metadata.json";
        }

        public override DeltaWebService OpenRequest(DeltaConnection conn, HttpContext e)
        {
            return new StructureMetadataRequest(conn, e);
        }
    }
}
