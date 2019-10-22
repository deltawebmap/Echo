using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using EchoReader.Entities;
using EchoReader.Exceptions;
using LibDeltaSystem.Db.System;

namespace EchoReader.Http
{
    public static class HttpHandler
    {
        public static async Task OnHttpRequest(Microsoft.AspNetCore.Http.HttpContext e)
        {
            //Authenticate this server
            var serverData = await Program.conn.AuthenticateServerTokenAsync(e.Request.Headers["X-Delta-Server-Creds"]);

            //If auth failed, stop
            if (serverData == null)
            {
                await Program.QuickWriteToDoc(e, "Server authentication failed.", "text/plain", 401);
                return;
            }

            //Try to find the ArkServer
            ArkServer server;
            if (Program.cache.ContainsKey(serverData.id)) //If it exists in the load cache, use that
                server = Program.cache[serverData.id];
            else
            {
                //Get from db
                server = new ArkServer
                {
                    id = serverData.id,
                    files = serverData.echo_files
                };
                if (server.files == null)
                    server.files = new List<LibDeltaSystem.Db.System.Entities.ServerEchoUploadedFile>();
                Program.cache.Add(server.id, server);
            }

            //Stop if this server is already busy
            if(server.busy)
            {
                await Program.QuickWriteToDoc(e, "This Ark server is busy processing an earlier request, try again shortly...", "text/plain", 400);
                return;
            }
            server.busy = true;
            try
            {
                //Find path
                string path = e.Request.Path.ToString();
                var method = Program.FindRequestMethod(e);
                if (path == "/upload" && (method == RequestHttpMethod.post || method == RequestHttpMethod.put))
                    await ServerFilePutService.OnHttpRequest(e, server);
                else if (path == "/files" && method == RequestHttpMethod.get)
                    await Program.QuickWriteJsonToDoc(e, server.files);
                else if (path == "/refresh" && method == RequestHttpMethod.post)
                    await ServerRefreshService.OnHttpRequest(e, server);
                else
                    throw new Exception("Not Found");
            } catch (BaseError bx) {
                await Program.QuickWriteToDoc(e, bx.msg, "text/plain", bx.code);
            } catch (Exception ex)
            {
                Console.WriteLine(ex.Message + ex.StackTrace);
                await Program.QuickWriteToDoc(e, "Error", "text/plain", 500);
            }
            server.busy = false;
        }
    }
}
