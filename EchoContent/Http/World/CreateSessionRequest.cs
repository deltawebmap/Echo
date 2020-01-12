﻿using EchoContent.Exceptions;
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
        public static async Task OnHttpRequest(Microsoft.AspNetCore.Http.HttpContext e, DbServer server, DbUser user, int tribeId, ArkMapEntry mapInfo)
        {
            //Get base url
            string baseUrl = Program.ROOT_URL + "/" + server.id;

            //Produce output
            ResponseData d = new ResponseData
            {
                dayTime = server.latest_server_time,
                systemTime = DateTime.UtcNow,
                mapName = mapInfo.displayName,
                mapData = mapInfo,
                maps = mapInfo.maps,
                mapBackgroundColor = mapInfo.backgroundColor,
                href = baseUrl + "/create_session",
                endpoint_tribes_icons = baseUrl + "/tribes/" + tribeId + "/icons",
                endpoint_tribes_dino = baseUrl + "/tribes/" + tribeId + "/dino/{dino}",
                endpoint_tribes_structure = baseUrl + "/tribes/" + tribeId + "/structure/{structure}",
                endpoint_tribes_itemsearch = baseUrl + "/tribes/" + tribeId + "/items/?q={query}",
                endpoint_tribes_overview = baseUrl + "/tribes/" + tribeId + "/overview",
                endpoint_tribes_dino_stats = baseUrl + "/tribes/" + tribeId + "/dino_stats?limit=30",
                endpoint_tribes_log = baseUrl + "/tribes/" + tribeId + "/log?page=0&limit=200",
                endpoint_put_dino_prefs = "https://deltamap.net/api/servers/" + server.id + "/put_dino_prefs/{dino}",
                endpoint_canvases = "https://deltamap.net/api/servers/" + server.id + "/canvas",
                endpoint_tribes_structures = baseUrl + "/tribes/" + tribeId + "/structures/all",
                endpoint_tribes_structures_metadata = baseUrl + "/tribes/" + tribeId + "/structures/metadata.json",
                endpoint_tribes_younglings = baseUrl + "/tribes/" + tribeId + "/younglings",
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

            public string href; //URL of this file. Depending on how this was loaded, this might be different from what was actually requested.
            
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
