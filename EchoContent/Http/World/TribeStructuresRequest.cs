using ArkSaveEditor.Entities;
using LibDeltaSystem;
using LibDeltaSystem.Db.Content;
using LibDeltaSystem.Db.System;
using LibDeltaSystem.Db.System.Entities;
using LibDeltaSystem.Entities.DynamicTiles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EchoContent.Http.World
{
    public static class TribeStructuresRequest
    {
        public static async Task OnHttpRequest(Microsoft.AspNetCore.Http.HttpContext e, DbServer server, DbUser user, int tribeId, ArkMapData mapInfo, DeltaPrimalDataPackage package)
        {
            //Find structures
            List<DbStructure> structures = await Program.conn.GetTribeStructures(server.id, tribeId);

            //Create response template
            ResponseData response = new ResponseData
            {
                i = new List<string>(),
                s = new List<ResponseStructure>()
            };

            //Sort
            structures.Sort(new Comparison<DbStructure>((x, y) =>
            {
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

                //Add to responses
                response.s.Add(new ResponseStructure
                {
                    i = index,
                    r = t.location.yaw,
                    s = metadata.size,
                    x = t.location.x,
                    y = t.location.y
                });
            }

            //Write response
            await Program.QuickWriteJsonToDoc(e, response);
        }

        public static async Task OnMetadataHttpRequest(Microsoft.AspNetCore.Http.HttpContext e)
        {
            //Simply write the structure metadata
            await Program.QuickWriteJsonToDoc(e, new MetadataResponse
            {
                metadata = Program.conn.GetStructureMetadata(),
                image_url = Program.config.endpoint_structure_images
            });
        }

        class MetadataResponse
        {
            public List<StructureMetadata> metadata;
            public string image_url;
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
        }
    }
}
