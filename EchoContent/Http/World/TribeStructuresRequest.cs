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

            //Sort
            EndDebugCheckpoint("Sort structures");
            structures.Sort(new Comparison<DbStructure>((x, y) =>
            {
                if (x.has_inventory || y.has_inventory)
                    return x.has_inventory.CompareTo(y.has_inventory);
                return x.location.z.CompareTo(y.location.z);
            }));

            //Run response mode
            if(e.Request.Query.ContainsKey("format"))
            {
                switch(e.Request.Query["format"])
                {
                    case "json": await RespondJSON(structures); break;
                    case "binary": await RespondBinary(structures); break;
                    default:
                        await WriteString("Unknown format specified.", "text/plain", 400);
                        return;
                }
            } else
            {
                //Default to json
                await RespondJSON(structures);
            }
        }

        private async Task RespondJSON(List<DbStructure> structures)
        {
            //Create response template
            ResponseData response = new ResponseData
            {
                i = new List<string>(),
                s = new List<ResponseStructure>()
            };

            //Loop through structures and find where they are
            EndDebugCheckpoint("Lay out structures");
            foreach (var t in structures)
            {
                //Get data
                StructureMetadata metadata = Program.conn.GetStructureMetadata().Where(x => x.names.Contains(t.classname)).FirstOrDefault();

                //Get image index
                string img = metadata.img;
                int index = response.i.IndexOf(img);
                if (index == -1)
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
        private async Task RespondBinary(List<DbStructure> structures)
        {
            //Binary mode writes out structures in binary structs

            //HEADER FORMAT:
            //  4   static  Signature, says "DWMS"
            //  4   int32   File type version, should be 1
            //  4   int32   Metadata version
            //  4   int32   Structure count
            //TOTAL: 16 bytes

            //Struct format:
            //  2   short   Metadata Index
            //  2   short   Rotation (in degrees, 0-360)
            //  4   float   Position X
            //  4   float   Position Y
            //  4   int     ID (0 = null)
            //TOTAL: 16 bytes each

            //Set headers
            e.Response.ContentLength = (16 * structures.Count) + 16;
            e.Response.ContentType = "application/octet-stream";
            e.Response.StatusCode = 200;
            EndDebugCheckpoint("Lay out structures");

            //Create and send file header
            byte[] buf = new byte[16];
            buf[0] = 0x44;
            buf[1] = 0x57;
            buf[2] = 0x4D;
            buf[3] = 0x53;
            BinaryTool.WriteInt32(buf, 4, 1);
            BinaryTool.WriteInt32(buf, 8, 0);
            BinaryTool.WriteInt32(buf, 12, structures.Count);
            e.Response.Body.Write(buf, 0, 16);

            //Loop through structures and find where they are
            foreach (var t in structures)
            {
                //Get data
                StructureMetadata metadata = Program.conn.GetStructureMetadata().Where(x => x.names.Contains(t.classname)).FirstOrDefault();
                int index = Program.conn.GetStructureMetadata().IndexOf(metadata);

                //Get the ID. It's set only if this has an inventory
                int sid = 0;
                if (t.has_inventory)
                    sid = t.structure_id;

                //Write parts
                BinaryTool.WriteInt16(buf, 0, (short)index);
                BinaryTool.WriteInt16(buf, 2, (short)t.location.yaw);
                BinaryTool.WriteFloat(buf, 4, t.location.x);
                BinaryTool.WriteFloat(buf, 8, t.location.y);
                BinaryTool.WriteInt32(buf, 12, sid);

                //Write to stream
                e.Response.Body.Write(buf, 0, 16);
            }
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
