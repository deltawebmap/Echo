using EchoContent.Exceptions;
using LibDeltaSystem.Db;
using LibDeltaSystem.Db.Content;
using LibDeltaSystem.Db.System;
using LibDeltaSystem.Entities.ArkEntries;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EchoContent.Http.World
{
    public static class CreateSessionRequest
    {
        public static async Task OnHttpRequest(Microsoft.AspNetCore.Http.HttpContext e, DbServer server, DbUser user, ArkMapEntry mapInfo)
        {
            //Get base url
            string baseUrl = Program.ROOT_URL + "/" + server.id + "/tribes/{tribe_id}";

            //Get the target tribe of this user, if any
            int? myTribeId = await server.TryGetTribeIdAsync(Program.conn, user.steam_id);
            DbTribe tribeData = null;
            if (myTribeId.HasValue)
                tribeData = await Program.conn.GetTribeByTribeIdAsync(server.id, myTribeId.Value);

            //Get my location
            DbVector3 myPos = null;
            var profile = await server.GetPlayerProfileBySteamIDAsync(Program.conn, myTribeId, user.steam_id);
            if(profile != null)
            {
                var character = await server.GetPlayerCharacterById(Program.conn, myTribeId, (uint)profile.ark_id);
                if (character != null)
                    myPos = character.pos;
            }

            //Produce output
            ResponseData d = new ResponseData
            {
                dayTime = server.latest_server_time,
                systemTime = DateTime.UtcNow,
                mapName = mapInfo.displayName,
                mapData = mapInfo,
                maps = mapInfo.maps,
                mapBackgroundColor = mapInfo.backgroundColor,
                endpoint_tribes_icons = baseUrl + "/icons",
                endpoint_tribes_dino = baseUrl + "/dino/{dino}",
                endpoint_tribes_structure = baseUrl + "/structure/{structure}",
                endpoint_tribes_itemsearch = baseUrl + "/items/?q={query}",
                endpoint_tribes_overview = baseUrl + "/overview",
                endpoint_tribes_dino_stats = baseUrl + "/dino_stats?limit=30",
                endpoint_tribes_log = baseUrl + "/log?page=0&limit=200",
                endpoint_put_dino_prefs = "https://deltamap.net/api/servers/" + server.id + "/put_dino_prefs/{dino}",
                endpoint_canvases = "https://deltamap.net/api/servers/" + server.id + "/canvas",
                endpoint_tribes_structures = baseUrl + "/structures/all",
                endpoint_tribes_structures_metadata = baseUrl + "/structures/metadata.json",
                endpoint_tribes_younglings = baseUrl + "/younglings",
                target_tribe = tribeData,
                my_location = myPos,
                my_profile = profile
            };

            //Write
            await Program.QuickWriteJsonToDoc(e, d);
        }

        class ResponseData
        {
            public float dayTime;

            public DateTime systemTime; //Time on this server that we should use instead of the user's system time

            public string mapName;
            public ArkMapEntry mapData;
            public string mapBackgroundColor;
            public ArkMapDisplayData[] maps; //Displable maps

            public DbTribe target_tribe; //The tribe this user belongs to
            public DbVector3 my_location; //The current location of the user
            public DbPlayerProfile my_profile; //The current profile of this user, contains ARK ID
            
            public string endpoint_tribes_icons; //Endpoint for viewing tribes
            public string endpoint_tribes_itemsearch; //Item search endpoint
            public string endpoint_tribes_dino; //Dino endpoint
            public string endpoint_tribes_structure; //Structure endpoint
            public string endpoint_tribes_overview; //Tribe properties list
            public string endpoint_tribes_dino_stats; //Tribe dino stats
            public string endpoint_tribes_log; //Tribe log
            public string endpoint_put_dino_prefs; //Puts dino prefs
            public string endpoint_canvases; //Gets canvas list
            public string endpoint_tribes_structures; //Structures
            public string endpoint_tribes_structures_metadata; //Structure metadata
            public string endpoint_tribes_younglings; //Baby dinos
        }
    }
}
