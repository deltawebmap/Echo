using LibDeltaSystem;
using LibDeltaSystem.Db.Content;
using LibDeltaSystem.Db.System;
using LibDeltaSystem.Entities.ArkEntries.Dinosaur;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EchoContent.Http.World
{
    public static class DinoYounglingsRequest
    {
        public static async Task OnHttpRequest(Microsoft.AspNetCore.Http.HttpContext e, DbServer server, DbUser user, int tribeId, DeltaPrimalDataPackage package)
        {
            //Create response
            ResponseData response = new ResponseData
            {
                eggs = await GetEggs(server, user, tribeId, package)
            };

            //Write
            await Program.QuickWriteJsonToDoc(e, response);
        }

        private static async Task<List<EggResponseData>> GetEggs(DbServer server, DbUser user, int tribeId, DeltaPrimalDataPackage package)
        {
            //Get all eggs
            var eggs = await DbEgg.GetTribeEggs(Program.conn, server.id, tribeId);

            //Convert all eggs
            List<EggResponseData> output = new List<EggResponseData>();
            foreach(var e in eggs)
            {
                //Get dinosaur entry
                DinosaurEntry dinoEntry = package.GetDinoEntry(e.egg_type);

                //Convert data
                EggResponseData r = new EggResponseData
                {
                    id = e.item_id.ToString(),
                    max_temperature = e.max_temperature,
                    min_temperature = e.min_temperature,
                    current_temperature = e.current_temperature,
                    health = e.health,
                    incubation = e.incubation,
                    hatch_time = e.hatch_time,
                    placed_time = e.placed_time,
                    location = e.location,
                    parents = e.parents,
                    dino_valid = dinoEntry != null,
                    dino_type = e.egg_type
                };

                //Write dino entry data, if we can
                if(dinoEntry != null)
                {
                    r.dino_name = dinoEntry.screen_name;
                    r.dino_icon = dinoEntry.icon.image_url;
                }

                //Add
                output.Add(r);
            }

            return output;
        }

        class ResponseData
        {
            public List<EggResponseData> eggs;
        }

        class EggResponseData
        {
            public string id; //ID of this egg
            public float max_temperature;
            public float min_temperature;
            public float current_temperature;
            public float health;
            public float incubation;
            public DateTime hatch_time;
            public DateTime placed_time;
            public DbLocation location;
            public string parents;

            public bool dino_valid; //Is the dino entry valid?
            public string dino_name;
            public string dino_icon;
            public string dino_type;
        }
    }
}
