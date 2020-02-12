using LibDeltaSystem;
using LibDeltaSystem.Db.Content;
using LibDeltaSystem.Db.System;
using LibDeltaSystem.Db.System.Entities;
using LibDeltaSystem.Entities.ArkEntries;
using LibDeltaSystem.Entities.DynamicTiles;
using LibDeltaSystem.Tools;
using LibDeltaSystem.WebFramework;
using Microsoft.AspNetCore.Http;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EchoContent.Http.World
{
    public class TribeStructuresRequest : EchoTribeDeltaService
    {
        public TribeStructuresRequest(DeltaConnection conn, HttpContext e) : base(conn, e)
        {
        }

        public override async Task OnRequest()
        {
            //Find structures
            EndDebugCheckpoint("Get structures");
            List<DbStructure> structures = await GetTribeStructures(server, tribeId);

            //Create response template
            ResponseData response = new ResponseData
            {
                i = new List<string>(),
                s = new List<ResponseStructure>()
            };

            //Sort
            EndDebugCheckpoint("Sort structures");
            structures.Sort(new Comparison<DbStructure>((x, y) =>
            {
                if (x.has_inventory || y.has_inventory)
                    return x.has_inventory.CompareTo(y.has_inventory);
                return x.location.z.CompareTo(y.location.z);
            }));

            //Loop through structures and find where they are
            EndDebugCheckpoint("Lay out structures");
            foreach (var t in structures)
            {
                //Get data
                StructureMetadata metadata = Program.conn.GetStructureMetadata().Where(x => x.names.Contains(t.classname)).FirstOrDefault();

                //Get image index
                string img = metadata.img;
                int index = response.i.IndexOf(img);
                if(index == -1)
                {
                    index = response.i.Count;
                    response.i.Add(img);
                }

                //Get the ID. It's set only if this has an inventory
                int? sid = null;
                if (t.has_inventory)
                    sid = t.structure_id;

                //Add to responses
                response.s.Add(new ResponseStructure
                {
                    i = index,
                    r = t.location.yaw,
                    s = metadata.size,
                    x = t.location.x,
                    y = t.location.y,
                    id = sid
                });
            }

            //Write response
            await WriteJSON(response);
        }

        private async Task<List<DbStructure>> GetTribeStructures(DbServer server, int? tribe_id)
        {
            //Make sure structures are up to date
            var metadata = conn.GetSupportedStructureMetadata();

            //Commit query
            var filterBuilder = Builders<DbStructure>.Filter;
            var filter = FilterBuilderToolDb.CreateTribeFilter<DbStructure>(server, tribe_id) &
                filterBuilder.In("classname", metadata) & filterBuilder.And(BuildFilters());
            var results = await conn.content_structures.FindAsync(filter);
            return await results.ToListAsync();
        }

        private List<FilterDefinition<DbStructure>> BuildFilters()
        {
            var filterBuilder = Builders<DbStructure>.Filter;
            List<FilterDefinition<DbStructure>> filters = new List<FilterDefinition<DbStructure>>();

            if (QueryParamsTool.TryGetFloatField(e, "upperx", out float minX))
                filters.Add(filterBuilder.Lt("location.x", minX));
            if (QueryParamsTool.TryGetFloatField(e, "uppery", out float minY))
                filters.Add(filterBuilder.Lt("location.y", minY));
            if (QueryParamsTool.TryGetFloatField(e, "lowerx", out float maxX))
                filters.Add(filterBuilder.Gt("location.x", maxX));
            if (QueryParamsTool.TryGetFloatField(e, "lowery", out float maxY))
                filters.Add(filterBuilder.Gt("location.y", maxY));

            return filters;
        }

        class ResponseData
        {
            public List<ResponseStructure> s; //Structures
            public List<string> i; //Images
            public float upt; //Units per tile
        }

        class ResponseStructure
        {
            public int i; //Image index
            public float r; //Rotation
            public float x; //X pos
            public float y; //Y pos
            public float s; //Size
            public int? id; //ID of this structure, ONLY if it has an inventory!
        }
    }
}
