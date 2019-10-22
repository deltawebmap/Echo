using EchoReader.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EchoReader.Http
{
    public static class ServerRefreshService
    {
        public static async Task OnHttpRequest(Microsoft.AspNetCore.Http.HttpContext e, ArkServer server)
        {
            //Process
            PerformanceReport report = await server.ProcessData();
            await Program.QuickWriteJsonToDoc(e, report);
        }
    }
}
