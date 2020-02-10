using LibDeltaSystem;
using LibDeltaSystem.Db.Content;
using LibDeltaSystem.Db.System;
using LibDeltaSystem.Db.System.Entities;
using LibDeltaSystem.Entities.ArkEntries;
using LibDeltaSystem.Entities.DynamicTiles;
using Microsoft.AspNetCore.Http;
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
            List<DbStructure> structures = await Program.conn.GetTribeStructures(server, tribeId);

            //Create response template
            ResponseData response = new ResponseData
            {
                i = new List<string>(),
                s = new List<ResponseStructure>()
            };

            //Sort
            structures.Sort(new Comparison<DbStructure>((x, y) =>
            {
                if (x.has_inventory || y.has_inventory)
                    return x.has_inventory.CompareTo(y.has_inventory);
                return x.location.z.CompareTo(y.location.z);
            }));

            //Loop through structures and find where they are
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
