using LibDeltaSystem;
using LibDeltaSystem.Entities.ArkEntries;
using LibDeltaSystem.WebFramework.ServiceTemplates;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EchoContent.Http
{
    /// <summary>
    /// Base service used for echo tribes
    /// </summary>
    public abstract class EchoTribeDeltaService : RequestTribeServerDeltaService
    {
        public EchoTribeDeltaService(DeltaConnection conn, HttpContext e) : base(conn, e)
        {
        }

        public override async Task<bool> SetArgs(Dictionary<string, string> args)
        {
            if (!await base.SetArgs(args))
                return false;

            return true;
        }
    }
}
