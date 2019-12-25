using EchoContent.Exceptions;
using LibDeltaSystem.Db.System;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using LibDeltaSystem;

namespace EchoContent.Http
{
    public static class HttpHandler
    {
        public static async Task OnHttpRequest(Microsoft.AspNetCore.Http.HttpContext e)
        {
            //Handle CORS stuff
            e.Response.Headers.Add("Server", "Delta Web Map 'Echo' server");
            e.Response.Headers.Add("Access-Control-Allow-Headers", "Authorization");
            e.Response.Headers.Add("Access-Control-Allow-Origin", "https://deltamap.net");
            e.Response.Headers.Add("Access-Control-Allow-Methods", "GET, POST, OPTIONS, DELETE, PUT, PATCH");
            if(e.Request.Method.ToUpper() == "OPTIONS")
            {
                await Program.QuickWriteToDoc(e, "Dropping OPTIONS request. Hello CORS!", "text/plain", 200);
                return;
            }

            try
            {
                //Get the server ID from the URL
                string[] split = e.Request.Path.ToString().Split('/');
                if(split.Length < 3)
                {
                    await Program.QuickWriteToDoc(e, "Invalid URL Structure", "text/plain", 404);
                    return;
                }
                string serverId = split[1];

                //Now, authenticate
                string token = await GetAuthToken(e);
                if (token == null)
                    return;
                DbUser user = await Program.conn.AuthenticateUserToken(token);
                if (user == null)
                {
                    await Program.QuickWriteToDoc(e, "Token Invalid", "text/plain", 403);
                    return;
                }

                //Get the server
                DbServer server = await Program.conn.GetServerByIdAsync(serverId);
                if (server == null)
                {
                    await Program.QuickWriteToDoc(e, "Server Not Found", "text/plain", 404);
                    return;
                }

                //Make sure that we're in this server
                int? tribeIdNullable = await server.TryGetTribeIdAsync(user.steam_id);
                if (!tribeIdNullable.HasValue)
                {
                    await Program.QuickWriteToDoc(e, "You Are Not a Member of this Server", "text/plain", 401);
                    return;
                }
                int tribeId = tribeIdNullable.Value;

                //Get next URL
                string next = e.Request.Path.ToString().Substring(serverId.Length + 1);
            
                //Get this map from the maps
                if (!Program.ark_maps.ContainsKey(server.latest_server_map))
                    throw new StandardError("The map this server is using is not supported.", $"Map '{server.latest_server_map}' is not supported yet.");
                var mapInfo = Program.ark_maps[server.latest_server_map];

                //Get primal data package
                DeltaPrimalDataPackage package = await Program.primal_data.LoadFullPackage(server.mods);

                //Get next
                if (next == "/create_session")
                    await World.CreateSessionRequest.OnHttpRequest(e, server, user, tribeId, mapInfo);
                else if (next == "/tribes/" + tribeId + "/icons")
                    await World.TribeInfoRequest.OnHttpRequest(e, server, user, tribeId, mapInfo, package);
                else if (next == "/tribes/" + tribeId + "/overview")
                    await World.TribeOverviewRequest.OnHttpRequest(e, server, user, tribeId, mapInfo, package);
                else if (next == "/tribes/" + tribeId + "/items/")
                    await World.ItemSearchRequest.OnHttpRequest(e, server, user, tribeId, mapInfo, package);
                else if (next == "/tribes/" + tribeId + "/younglings")
                    await World.DinoYounglingsRequest.OnHttpRequest(e, server, user, tribeId, package);
                else if (next.StartsWith("/tribes/" + tribeId + "/dino/"))
                    await World.DinoInfoRequest.OnHttpRequest(e, server, user, tribeId, mapInfo, package);
                else if (next.StartsWith("/tribes/" + tribeId + "/structure/"))
                    await World.StructureInfoRequest.OnHttpRequest(e, server, user, tribeId, mapInfo, package);
                else if (next.StartsWith("/tribes/" + tribeId + "/dino_stats"))
                    await World.DinoListRequest.OnHttpRequest(e, server, user, tribeId, package);
                else if (next.StartsWith("/tribes/" + tribeId + "/log"))
                    await World.TribeLogRequest.OnHttpRequest(e, server, user, tribeId);
                else if (next.StartsWith("/tribes/" + tribeId + "/thumbnail"))
                    await World.ThumbnailRequest.OnHttpRequest(e, server, user, tribeId, mapInfo, package);
                else if (next.StartsWith("/tribes/" + tribeId + "/structures/all"))
                    await World.TribeStructuresRequest.OnHttpRequest(e, server, user, tribeId, mapInfo, package);
                else if (next.StartsWith("/tribes/" + tribeId + "/structures/metadata.json"))
                    await World.TribeStructuresRequest.OnMetadataHttpRequest(e);
                else
                    await Program.QuickWriteToDoc(e, "Server Endpoint Not Found", "text/plain", 404);
            } catch (StandardError ex)
            {
                await Program.QuickWriteJsonToDoc(e, new LibDeltaSystem.Entities.HttpErrorResponse
                {
                    message = ex.msg,
                    message_more = ex.msg_more,
                    support_tag = null
                }, ex.http_code);
            } catch (Exception ex)
            {
                var response = await Program.conn.LogHttpError(ex, new Dictionary<string, string>());
                await Program.QuickWriteJsonToDoc(e, response, 500);
            }
        }

        private static async Task<string> GetAuthToken(Microsoft.AspNetCore.Http.HttpContext e)
        {
            if (!e.Request.Headers.ContainsKey("authorization"))
            {
                await Program.QuickWriteToDoc(e, "No Token Provided", "text/plain", 403);
                return null;
            }
            if (!e.Request.Headers["authorization"].ToString().StartsWith("Bearer "))
            {
                await Program.QuickWriteToDoc(e, "No Token Provided", "text/plain", 403);
                return null;
            }
            return e.Request.Headers["authorization"].ToString().Substring("Bearer ".Length);
        }

        class ReturnedStandardError
        {
            public string msg;
            public string msg_more;
            public string support_code;
        }
    }
}
