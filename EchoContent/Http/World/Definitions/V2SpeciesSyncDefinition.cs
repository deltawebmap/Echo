using LibDeltaSystem;
using LibDeltaSystem.WebFramework;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace EchoContent.Http.World.Definitions
{
    public class V2SpeciesSyncDefinition : DeltaWebServiceDefinition
    {
        public override string GetTemplateUrl()
        {
            return "/species.json";
        }

        public override DeltaWebService OpenRequest(DeltaConnection conn, HttpContext e)
        {
            return new V2SpeciesSyncRequest(conn, e);
        }
    }
}
