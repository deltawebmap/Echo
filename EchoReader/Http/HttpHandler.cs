using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using EchoReader.Entities;
using LibDelta;

namespace EchoReader.Http
{
    public static class HttpHandler
    {
        public static async Task OnHttpRequest(Microsoft.AspNetCore.Http.HttpContext e)
        {
            try
            {
                //Authenticate this server
                var serverData = await DeltaAuth.AuthenticateServer(e.Request.Headers["X-Delta-Server-ID"], e.Request.Headers["X-Delta-Server-Creds"]);

                //If auth failed, stop
                if (serverData == null)
                    throw new Exception("Server authentication failed.");

                //Try to find the ArkServer
                ArkServer server;
                if (Program.cache.ContainsKey(serverData.server_id)) //If it exists in the load cache, use that
                    server = Program.cache[serverData.server_id];
                else if (Program.servers_collection.FindById(serverData.server_id) != null) //Check if a serialized copy exists on disk
                {
                    server = new ArkServer();
                    server.Load(Program.servers_collection.FindById(serverData.server_id));
                    Program.cache.Add(server.id, server);
                } else //This is the first time we've accessed this server. Create a new server
                {
                    server = new ArkServer();
                    server.id = serverData.server_id;
                    server.files = new List<ArkUploadedFile>();
                    server.Save();
                    Program.cache.Add(server.id, server);
                }

                //Find path
                string path = e.Request.Path.ToString();
                var method = Program.FindRequestMethod(e);
                if (path == "/upload" && (method == RequestHttpMethod.post || method == RequestHttpMethod.put))
                    await ServerFilePutService.OnHttpRequest(e, server);
                else if (path == "/files" && method == RequestHttpMethod.get)
                    await Program.QuickWriteJsonToDoc(e, server.files);
                else
                    throw new Exception("Not Found");
            } catch (Exception ex)
            {
                Console.WriteLine(ex.Message + ex.StackTrace);
                await Program.QuickWriteToDoc(e, "Error", "text/plain", 500);
            }
        }
    }
}
