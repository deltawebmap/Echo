using EchoReader.Entities;
using LibDeltaSystem.Db.System.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EchoReader.Http
{
    public static class ServerFilePutService
    {
        public static async Task OnHttpRequest(Microsoft.AspNetCore.Http.HttpContext e, ArkServer server)
        {
            //Get data
            string name = e.Request.Headers["X-Delta-Filename"];
            ArkUploadedFileType type = Enum.Parse<ArkUploadedFileType>(e.Request.Headers["X-Delta-File-Type"]);

            //Now, put
            ServerEchoUploadedFile file = await server.PutFile(type, name, e.Request.Body);

            //Respond with file data
            await Program.QuickWriteJsonToDoc(e, file);
        }
    }
}
