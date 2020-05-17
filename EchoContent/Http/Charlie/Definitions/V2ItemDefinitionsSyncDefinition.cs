using LibDeltaSystem;
using LibDeltaSystem.WebFramework;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace EchoContent.Http.Charlie.Definitions
{
    public class V2ItemDefinitionsSyncDefinition : DeltaWebServiceDefinition
    {
        public override string GetTemplateUrl()
        {
            return "/items.json";
        }

        public override DeltaWebService OpenRequest(DeltaConnection conn, HttpContext e)
        {
            return new V2ItemDefinitionsSyncRequest(conn, e);
        }
    }
}
