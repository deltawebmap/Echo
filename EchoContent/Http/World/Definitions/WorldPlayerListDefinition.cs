using LibDeltaSystem;
using LibDeltaSystem.WebFramework;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace EchoContent.Http.World.Definitions
{
    public class WorldPlayerListDefinition : DeltaWebServiceDefinition
    {
        public override string GetTemplateUrl()
        {
            return "/v1/{SERVER}/players";
        }

        public override DeltaWebService OpenRequest(DeltaConnection conn, HttpContext e)
        {
            return new WorldPlayerListRequest(conn, e);
        }
    }
}
