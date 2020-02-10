using LibDeltaSystem;
using LibDeltaSystem.WebFramework;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace EchoContent.Http.World.Definitions
{
    public class TribeStructuresDefinition : DeltaWebServiceDefinition
    {
        public override string GetTemplateUrl()
        {
            return "/{SERVER}/tribes/{TRIBE}/structures";
        }

        public override DeltaWebService OpenRequest(DeltaConnection conn, HttpContext e)
        {
            return new TribeStructuresRequest(conn, e);
        }
    }
}
