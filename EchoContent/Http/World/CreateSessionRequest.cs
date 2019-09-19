﻿using ArkSaveEditor.Entities;
using EchoContent.Exceptions;
using LibDeltaSystem.Db.System;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EchoContent.Http.World
{
    public static class CreateSessionRequest
    {
        public static async Task OnHttpRequest(Microsoft.AspNetCore.Http.HttpContext e, DbServer server, DbUser user, int tribeId, ArkMapData mapInfo)
        {
            //Get base url
            string baseUrl = Program.ROOT_URL + "/" + server.id;

            //Produce output
            ResponseData d = new ResponseData
            {
                dayTime = server.latest_server_time,
                mapName = mapInfo.displayName,
                mapData = mapInfo,
                maps = mapInfo.maps,
                mapBackgroundColor = mapInfo.backgroundColor,
                href = baseUrl + "/create_session",
                endpoint_tribes = baseUrl + "/tribes/" + tribeId + "/info",
                endpoint_tribes_dino = baseUrl + "/tribes/" + tribeId + "/dinos/{dino}",
                endpoint_tribes_itemsearch = baseUrl + "/tribes/" + tribeId + "/items/?q={query}",
                endpoint_tribes_overview = baseUrl + "/tribes/" + tribeId + "/overview"
            };

            //Write
            await Program.QuickWriteJsonToDoc(e, d);
        }

        class ResponseData
        {
            public float dayTime;

            public string mapName;
            public ArkMapData mapData;
            public string mapBackgroundColor;
            public ArkMapDisplayData[] maps; //Displable maps

            public string href; //URL of this file. Depending on how this was loaded, this might be different from what was actually requested.
            
            public string endpoint_tribes; //Endpoint for viewing tribes
            public string endpoint_tribes_itemsearch; //Item search endpoint
            public string endpoint_tribes_dino; //Dino endpoint
            public string endpoint_tribes_overview; //Tribe properties list
        }
    }
}
