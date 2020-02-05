using LibDeltaSystem.Db.System;
using LibDeltaSystem.Db.Content;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Driver;
using EchoContent.Exceptions;
using LibDeltaSystem;
using LibDeltaSystem.Entities.ArkEntries;
using EchoContent.Tools;

namespace EchoContent.Http.World
{
    public static class TribeOverviewRequest
    {
        public static async Task OnHttpRequest(Microsoft.AspNetCore.Http.HttpContext e, DbServer server, DbUser user, int? tribeId, ArkMapEntry mapInfo, DeltaPrimalDataPackage package)
        {
            //Get player profiles
            var playerProfiles = await GetPlayerProfiles(server, tribeId);
            List<TribeOverviewPlayer> responsePlayers = new List<TribeOverviewPlayer>();
            foreach(var p in playerProfiles)
            {
                //Fetch steam data
                DbSteamCache steamProfile = await Program.conn.GetSteamProfileById(p.steam_id);
                if (steamProfile == null)
                    continue;

                //Add
                responsePlayers.Add(new TribeOverviewPlayer
                {
                    steamId = steamProfile.steam_id,
                    steamName = steamProfile.name,
                    steamUrl = steamProfile.profile_url,
                    img = steamProfile.icon_url,
                    arkId = p.ark_id.ToString(),
                    arkName = p.ig_name
                });
            }

            //Get dino profiles
            var dinoProfiles = await GetDinosaurs(server, tribeId);

            //Get prefs
            var massPrefs = await Program.conn.MassGetDinoPrefs(server, dinoProfiles);

            List<TribeOverviewDino> dinos = new List<TribeOverviewDino>();
            foreach(var p in dinoProfiles)
            {
                //Lookup dino entry for this
                var entry = await package.GetDinoEntryByClssnameAsnyc(p.classname);
                if (entry == null)
                    continue;

                //Get prefs
                var prefs = massPrefs[p.dino_id];

                //Convert
                dinos.Add(new TribeOverviewDino
                {
                    classDisplayName = entry.screen_name,
                    displayName = p.tamed_name,
                    id = p.dino_id.ToString(),
                    img = entry.icon.image_thumb_url,
                    level = p.level,
                    status = p.status,
                    color_tag = prefs.color_tag,
                    is_cryo = p.is_cryo,
                    is_baby = p.is_baby,
                    is_female = p.is_female
                });
            }

            //Create a response
            OverviewResponse response = new OverviewResponse
            {
                dinos = dinos,
                tribemates = responsePlayers
            };

            //Write
            await Program.QuickWriteJsonToDoc(e, response);
        }

        private static async Task<List<DbPlayerProfile>> GetPlayerProfiles(DbServer server, int? tribeId)
        {
            //Find all
            var filterBuilder = Builders<DbPlayerProfile>.Filter;
            var filter = FilterBuilderTool.CreateTribeFilter<DbPlayerProfile>(server, tribeId);
            var response = await Program.conn.content_player_profiles.FindAsync(filter);
            var profile = await response.ToListAsync();
            return profile;
        }

        private static async Task<List<DbDino>> GetDinosaurs(DbServer server, int? tribeId)
        {
            var filterBuilder = Builders<DbDino>.Filter;
            var filter = filterBuilder.Eq("is_tamed", true) & FilterBuilderTool.CreateTribeFilter<DbDino>(server, tribeId);
            var response = await Program.conn.content_dinos.FindAsync(filter);
            var dino = await response.ToListAsync();
            return dino;
        }

        class OverviewResponse
        {
            public List<TribeOverviewPlayer> tribemates;
            public List<TribeOverviewDino> dinos;
        }

        class TribeOverviewPlayer
        {
            public string arkName;
            public string steamName;
            public string arkId;
            public string steamId;
            public string steamUrl;
            public string img;
        }

        public class TribeOverviewDino
        {
            public string displayName;
            public string classDisplayName;
            public int level;
            public string id;
            public string img;
            public string status;
            public string color_tag;
            public bool is_cryo;
            public bool is_baby;
            public bool is_female;
        }
    }
}
