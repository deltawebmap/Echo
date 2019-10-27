using ArkSaveEditor.Entities;
using LibDeltaSystem.Db.System;
using LibDeltaSystem.Db.Content;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Driver;
using ArkSaveEditor;
using LibDeltaSystem;

namespace EchoContent.Http.World
{
    public static class TribeInfoRequest
    {
        public static async Task OnHttpRequest(Microsoft.AspNetCore.Http.HttpContext e, DbServer server, DbUser user, int tribeId, ArkMapData mapInfo, DeltaPrimalDataPackage package)
        {
            //Create
            ResponseTribe tribe = new ResponseTribe
            {
                dinos = await CreateDinos(server, tribeId, mapInfo, package),
                gameTime = server.latest_server_time,
                player_characters = new List<ResponsePlayerCharacter>(),
                tribeId = tribeId
            };

            //Write
            await Program.QuickWriteJsonToDoc(e, tribe);
        }

        private static async Task<List<ResponseDino>> CreateDinos(DbServer server, int tribeId, ArkMapData mapInfo, DeltaPrimalDataPackage package)
        {
            //Find all dinosaurs
            var filterBuilder = Builders<DbDino>.Filter;
            var filter = filterBuilder.Eq("is_tamed", true) & filterBuilder.Eq("server_id", server.id) & filterBuilder.Eq("tribe_id", tribeId);
            var response = await server.conn.content_dinos.FindAsync(filter);
            var responseList = await response.ToListAsync();

            //Convert all dinosaurs
            List<ResponseDino> dinos = new List<ResponseDino>();
            foreach (var dino in responseList)
            {
                //Try to find a dinosaur entry
                var entry = package.GetDinoEntry(dino.classname);
                if (entry == null)
                    continue;

                //Get prefs
                var prefs = await dino.GetPrefs(Program.conn);

                //Make dinosaur entry
                ResponseDino d = new ResponseDino
                {
                    coord_pos = WorldTools.ConvertFromWorldToGameCoords(dino.location, mapInfo),
                    classname = dino.classname,
                    imgUrl = entry.icon.image_thumb_url,
                    id = dino.dino_id.ToString(),
                    apiUrl = Program.ROOT_URL + "/" + server.id + "/tribes/" + tribeId + "/dinos/" + dino.dino_id.ToString(),
                    tamedName = dino.tamed_name,
                    displayClassname = entry.screen_name,
                    level = dino.level,
                    adjusted_map_pos = mapInfo.ConvertFromGamePositionToNormalized(new Vector2(dino.location.x, dino.location.y)),
                    status = dino.status,
                    color_tag = prefs.color_tag
                };

                //Add
                dinos.Add(d);
            }

            return dinos;
        }

        class ResponseTribe
        {
            public float gameTime;
            public List<ResponseDino> dinos;
            public int tribeId;

            public List<ResponsePlayerCharacter> player_characters;
        }

        class ResponseDino
        {
            public Vector2 coord_pos;
            public Vector2 adjusted_map_pos; //Position for the web map

            public string classname;
            public string imgUrl;
            public string apiUrl;
            public string id;
            public string tamedName;
            public string displayClassname;
            public int level;
            public string status;
            public string color_tag;
        }

        class ResponsePlayerCharacter
        {
            public Vector2 coord_pos;
            public Vector2 adjusted_map_pos; //Position for the web map

            /*public ArkPlayerProfile profile;
            public SteamProfile steamProfile; //This will be set outside of our constructor*/
            public bool is_alive;
        }
    }
}
