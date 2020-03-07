using LibDeltaSystem;
using LibDeltaSystem.WebFramework;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace EchoContent.Http.World.Definitions
{
    public class V2DinoSyncDefinition : DeltaWebServiceDefinition
    {
        public override string GetTemplateUrl()
        {
            return "/{SERVER}/tribes/{TRIBE}/dinos";
        }

        public override DeltaWebService OpenRequest(DeltaConnection conn, HttpContext e)
        {
            return new V2DinoSyncRequest(conn, e);
        }
    }
}
