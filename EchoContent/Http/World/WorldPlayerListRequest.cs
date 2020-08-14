using LibDeltaSystem;
using LibDeltaSystem.Db.Content;
using Microsoft.AspNetCore.Http;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EchoContent.Http.World
{
    public class WorldPlayerListRequest : EchoTribeDeltaService
    {
        public WorldPlayerListRequest(DeltaConnection conn, HttpContext e) : base(conn, e)
        {
        }

        public override async Task OnRequest()
        {
            //Get all player profiles
            var profilesTask = await conn.content_player_profiles.FindAsync<DbPlayerProfile>(GetServerTribeFilter<DbPlayerProfile>());
            var profiles = await profilesTask.ToListAsync();

            //Convert all
            ResponseData response = new ResponseData
            {
                profiles = new List<ResponseData_Profile>()
            };

            foreach(var p in profiles)
            {
                response.profiles.Add(new ResponseData_Profile
                {
                    steam_icon = p.icon,
                    steam_id = p.steam_id,
                    steam_name = p.name,
                    last_seen = p.last_seen,
                    x = p.x,
                    y = p.y,
                    z = p.z,
                    yaw = p.yaw
                });
            }

            //Write response
            await WriteJSON(response);
        }

        class ResponseData
        {
            public List<ResponseData_Profile> profiles;
        }

        class ResponseData_Profile
        {
            public string steam_id;
            public string steam_name;
            public string steam_icon;
            public DateTime last_seen;
            public float? x;
            public float? y;
            public float? z;
            public float? yaw;
        }
    }
}
