using ArkSaveEditor;
using ArkSaveEditor.Entities;
using LibDeltaSystem.Db.Content;
using LibDeltaSystem.Db.System;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EchoContent.Http.World
{
    public static class ThumbnailRequest
    {
        public static async Task OnHttpRequest(Microsoft.AspNetCore.Http.HttpContext e, DbServer server, DbUser user, int tribeId, ArkMapData mapInfo)
        {
            //Find all dinosaurs
            var filterBuilder = Builders<DbDino>.Filter;
            var filter = filterBuilder.Eq("is_tamed", true) & filterBuilder.Eq("server_id", server.id) & filterBuilder.Eq("tribe_id", tribeId);
            var response = await server.conn.content_dinos.FindAsync(filter);
            var responseList = await response.ToListAsync();

            //Find bounds to use
            int quadIndexes = responseList.Count / 4;
            responseList.Sort(new Comparison<DbDino>((x, y) => x.location.x.CompareTo(y.location.x)));
            float left = responseList[quadIndexes * 2].location.x;
            float right = responseList[quadIndexes * 4].location.x;
            responseList.Sort(new Comparison<DbDino>((x, y) => x.location.y.CompareTo(y.location.y)));
            float top = responseList[quadIndexes * 2].location.y;
            float bottom = responseList[quadIndexes * 4].location.y;

            //Convert all dinosaurs
            List<ResponseDino> dinos = new List<ResponseDino>();
            List<string> assets = new List<string>();
            foreach (var dino in responseList)
            {
                //Skip if out of bounds
                if (dino.location.x > right || dino.location.x < left || dino.location.y > bottom || dino.location.y < top)
                    continue;
                
                //Try to find a dinosaur entry
                var entry = ArkImports.GetDinoDataByClassname(dino.classname);
                if (entry == null)
                    continue;

                //Get asset index
                int index = assets.IndexOf(entry.icon.image_thumb_url);
                if(index == -1)
                {
                    index = assets.Count;
                    assets.Add(entry.icon.image_thumb_url);
                }

                //Make dinosaur entry
                var pos = mapInfo.ConvertFromGamePositionToNormalized(new Vector2(dino.location.x, dino.location.y));
                ResponseDino d = new ResponseDino
                {
                    i = index,
                    x = pos.x,
                    y = pos.y
                };

                //Add
                dinos.Add(d);
            }

            //Write
            await Program.QuickWriteJsonToDoc(e, new ResponseData
            {
                a = assets,
                c = dinos,
                m = mapInfo.maps[0].url
            });
        }

        class ResponseData
        {
            public string m;
            public List<ResponseDino> c;
            public List<string> a;
        }

        class ResponseDino
        {
            public float x;
            public float y;
            public int i;
        }
    }
}
