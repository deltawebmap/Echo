using EchoContent.Exceptions;
using LibDeltaSystem.Db.System;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using LibDeltaSystem;
using LibDeltaSystem.Entities.ArkEntries;

namespace EchoContent.Http
{
    public static class HttpHandler
    {
        public const int SPLIT_SERVER = 1;
        public const int SPLIT_TRIBES_ENDPOINT = 2;
        public const int SPLIT_TRIBE = 3;

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

                //Get this map from the maps
                var mapInfo = await server.GetMapEntryAsync(Program.conn);
                if (mapInfo == null)
                    throw new StandardError("The map this server is using is not supported.", $"Map '{server.latest_server_map}' is not supported yet.");

                //Get next URL
                string next = e.Request.Path.ToString().Substring(serverId.Length + 1);

                //Check if this is a tribe request
                if (next.StartsWith("/tribes/"))
                    await OnTribesRequest(e, user, server, split, next, mapInfo);
                else
                    await OnServerRequest(e, user, server, split, next, mapInfo);
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

        private static async Task OnTribesRequest(Microsoft.AspNetCore.Http.HttpContext e, DbUser user, DbServer server, string[] split, string next, ArkMapEntry mapInfo)
        {
            //Get requested tribe Id
            int? tribeId = await GetRequestedTribeID(user, server, split);
            if (tribeId == 0)
            {
                await Program.QuickWriteToDoc(e, "You aren't permitted to access this tribe, you don't have a tribe, or you tried to request admin access when you can't do that.", "text/plain", 400);
                return;
            }

            //Get primal data package
            DeltaPrimalDataPackage package = await Program.conn.GetPrimalDataPackage(new string[0]);

            //Trim next
            next = next.Substring(2 + split[SPLIT_TRIBES_ENDPOINT].Length + split[SPLIT_TRIBE].Length);

            //Run next
            if (next == "/icons")
                await World.TribeInfoRequest.OnHttpRequest(e, server, user, tribeId, mapInfo, package);
            else if (next == "/overview")
                await World.TribeOverviewRequest.OnHttpRequest(e, server, user, tribeId, mapInfo, package);
            else if (next == "/items/")
                await World.ItemSearchRequest.OnHttpRequest(e, server, user, tribeId, mapInfo, package);
            else if (next == "/younglings")
                await World.DinoYounglingsRequest.OnHttpRequest(e, server, user, tribeId, package);
            else if (next.StartsWith("/dino/"))
                await World.DinoInfoRequest.OnHttpRequest(e, server, user, tribeId, mapInfo, package);
            else if (next.StartsWith("/structure/"))
                await World.StructureInfoRequest.OnHttpRequest(e, server, user, tribeId, mapInfo, package);
            else if (next.StartsWith("/dino_stats"))
                await World.DinoListRequest.OnHttpRequest(e, server, user, tribeId, package);
            else if (next.StartsWith("/log"))
                await World.TribeLogRequest.OnHttpRequest(e, server, user, tribeId);
            else if (next.StartsWith("/thumbnail"))
                await World.ThumbnailRequest.OnHttpRequest(e, server, user, tribeId, mapInfo, package);
            else if (next.StartsWith("/structures/all"))
                await World.TribeStructuresRequest.OnHttpRequest(e, server, user, tribeId, mapInfo, package);
            else if (next.StartsWith("/structures/metadata.json"))
                await World.TribeStructuresRequest.OnMetadataHttpRequest(e);
            else
                await Program.QuickWriteToDoc(e, "Server Endpoint Not Found", "text/plain", 404);
        }

        private static async Task OnServerRequest(Microsoft.AspNetCore.Http.HttpContext e, DbUser user, DbServer server, string[] split, string next, ArkMapEntry mapInfo)
        {
            if (next == "/create_session")
                await World.CreateSessionRequest.OnHttpRequest(e, server, user, mapInfo);
            else
                await Program.QuickWriteToDoc(e, "Server Endpoint Not Found", "text/plain", 404);
        }

        private static async Task<int?> GetRequestedTribeID(DbUser u, DbServer s, string[] split)
        {
            //Check if this user has admin
            bool isAdmin = s.CheckIsUserAdmin(u);

            //If we're requesting *all* tribes, make sure we're admin
            if (isAdmin && split[SPLIT_TRIBE] == "*")
                return null;
            else if (split[SPLIT_TRIBE] == "*")
                return 0;
            
            //Get the requested tribe ID
            if(!int.TryParse(split[SPLIT_TRIBE], out int requestedTribeId))
            {
                return 0;
            }

            //Get the target tribe ID
            int? myTribeId = await s.TryGetTribeIdAsync(u.steam_id);
            if (!myTribeId.HasValue)
            {
                return 0;
            }

            //If the target tribe ID and my tribe ID don't match, fail unless we're admin
            if (myTribeId != requestedTribeId && isAdmin)
                return requestedTribeId;
            else if (myTribeId != requestedTribeId)
                return 0;
            else
                return myTribeId;
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
