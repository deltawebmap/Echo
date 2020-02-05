﻿using LibDeltaSystem.Db.System;
using LibDeltaSystem.Db.Content;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Driver;
using LibDeltaSystem;
using LibDeltaSystem.Db.System.Entities;
using LibDeltaSystem.Entities.ArkEntries;
using EchoContent.Tools;
using System.Linq;

namespace EchoContent.Http.World
{
    public static class TribeInfoRequest
    {
        public static async Task OnHttpRequest(Microsoft.AspNetCore.Http.HttpContext e, DbServer server, DbUser user, int? tribeId, ArkMapEntry mapInfo, DeltaPrimalDataPackage package)
        {
            //Create
            ResponseTribe tribe = new ResponseTribe
            {
                icons = new List<MapIcon>(),
                gameTime = server.latest_server_time,
                tribeId = tribeId
            };

            //Add all types
            tribe.icons.AddRange(await CreateDinos(server, tribeId, mapInfo, package));
            tribe.icons.AddRange(await CreatePlayers(server, tribeId, mapInfo, package));

            //Write
            await Program.QuickWriteJsonToDoc(e, tribe);
        }

        private static Dictionary<string, string> DINO_STATUS_COLOR_MAP = new Dictionary<string, string>
        {
            {"PASSIVE","#5AE000" },
            {"NEUTRAL","#000000" },
            {"AGGRESSIVE","#E63F19" },
            {"PASSIVE_FLEE","#E6D51C" },
            {"YOUR_TARGET","#1C9BE6" },
        };

        private static async Task<List<MapIcon>> CreateDinos(DbServer server, int? tribeId, ArkMapEntry mapInfo, DeltaPrimalDataPackage package)
        {
            //Find all dinosaurs
            var filterBuilder = Builders<DbDino>.Filter;
            var filter = FilterBuilderTool.CreateTribeFilter<DbDino>(server, tribeId) & filterBuilder.Eq("is_cryo", false);
            var response = await Program.conn.content_dinos.FindAsync(filter);
            var responseList = await response.ToListAsync();

            //Get prefs
            var massPrefs = await Program.conn.MassGetDinoPrefs(server, responseList);

            //Convert all dinosaurs
            List<MapIcon> dinos = new List<MapIcon>();
            foreach (var dino in responseList)
            {
                //Try to find a dinosaur entry
                var entry = await package.GetDinoEntryByClssnameAsnyc(dino.classname);
                if (entry == null)
                    continue;

                //Get prefs
                var prefs = massPrefs[dino.dino_id];

                //Add
                dinos.Add(new MapIcon
                {
                    location = dino.location,
                    img = entry.icon.image_thumb_url,
                    type = "dinos",
                    id = dino.dino_id.ToString(),
                    outline_color = DINO_STATUS_COLOR_MAP[dino.status],
                    tag_color = prefs.color_tag,
                    dialog = new MapIconHoverDialog
                    {
                        title = dino.tamed_name,
                        subtitle = entry.screen_name + " - Lvl " + dino.level
                    },
                    extras = new MapIconExtra_Dino
                    {
                        prefs = prefs,
                        url = Program.ROOT_URL + "/" + server.id + "/tribes/" + tribeId + "/dino/" + dino.dino_id.ToString()
                    }
                });
            }

            return dinos;
        }

        private static async Task<List<MapIcon>> CreatePlayers(DbServer server, int? tribeId, ArkMapEntry mapInfo, DeltaPrimalDataPackage package)
        {
            //Find all dinosaurs
            var filterBuilder = Builders<DbPlayerCharacter>.Filter;
            var filter = FilterBuilderTool.CreateTribeFilter<DbPlayerCharacter>(server, tribeId);
            var response = await Program.conn.content_player_characters.FindAsync(filter);
            var responseList = await response.ToListAsync();

            //Get all users on this server
            var players = await server.GetPlayerProfiles(Program.conn);

            //Convert all
            List<MapIcon> output = new List<MapIcon>();
            foreach (var player in responseList)
            {
                //Look up player data
                var p = players.Where(x => x.ark_id == player.ark_id).FirstOrDefault();
                if (p == null)
                    continue;

                //Look up Steam info
                var steamInfo = await Program.conn.GetSteamProfileById(p.steam_id);
                
                //Add
                output.Add(new MapIcon
                {
                    location = new DbLocation(player.pos.x, player.pos.y, player.pos.z),
                    img = steamInfo.icon_url,
                    type = "players",
                    id = p.ark_id.ToString(),
                    outline_color = null,
                    tag_color = null,
                    dialog = new MapIconHoverDialog
                    {
                        title = steamInfo.name,
                        subtitle = "ID "+p.ark_id
                    },
                    extras = null
                });
            }

            return output;
        }

        class ResponseTribe
        {
            public float gameTime;
            public List<MapIcon> icons;
            public int? tribeId;
        }

        class MapIcon
        {
            /// <summary>
            /// Location in game units
            /// </summary>
            public DbLocation location;

            /// <summary>
            /// Icon image
            /// </summary>
            public string img;

            /// <summary>
            /// The type of object
            /// </summary>
            public string type;

            /// <summary>
            /// Unqiue (to this type) ID used to map this object
            /// </summary>
            public string id;

            /// <summary>
            /// Outline color. Can be null
            /// </summary>
            public string outline_color;

            /// <summary>
            /// Color tag. Won't appear if null.
            /// </summary>
            public string tag_color;

            /// <summary>
            /// Hover dialog. Won't appear if this is null
            /// </summary>
            public MapIconHoverDialog dialog;

            /// <summary>
            /// Additional information to include
            /// </summary>
            public object extras;
        }

        class MapIconHoverDialog
        {
            public string title;
            public string subtitle;
        }

        class MapIconExtra_Dino
        {
            public string url;
            public SavedDinoTribePrefs prefs;
        }
    }
}
