using LibDeltaSystem;
using LibDeltaSystem.Db.Content;
using LibDeltaSystem.Db.System;
using LibDeltaSystem.Entities.ArkEntries.Dinosaur;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EchoContent.Http.World
{
    public class DinoYounglingsRequest : EchoTribeDeltaService
    {
        public DinoYounglingsRequest(DeltaConnection conn, HttpContext e) : base(conn, e)
        {
        }

        public override async Task OnRequest()
        {
            //Create response
            ResponseData response = new ResponseData
            {
                eggs = await GetEggs()
            };

            //Write
            await WriteJSON(response);
        }

        private async Task<List<EggResponseData>> GetEggs()
        {
            //Get all eggs
            var eggs = await DbEgg.GetEggs(conn, GetServerTribeFilter<DbEgg>());

            //Convert all eggs
            List<EggResponseData> output = new List<EggResponseData>();
            foreach(var e in eggs)
            {
                //Get dinosaur entry
                DinosaurEntry dinoEntry = await package.GetDinoEntryByClssnameAsnyc(e.egg_type);

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
