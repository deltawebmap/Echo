using LibDeltaSystem;
using LibDeltaSystem.Db.Content;
using LibDeltaSystem.Entities;
using LibDeltaSystem.Entities.CommonNet;
using LibDeltaSystem.Tools;
using Microsoft.AspNetCore.Http;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EchoContent.Http.World
{
    public class V2StructuresSyncRequest : V2SyncDeltaService<DbStructure, NetStructure>
    {
        public V2StructuresSyncRequest(DeltaConnection conn, HttpContext e) : base(conn, e)
        {
        }

        public override NetStructure ConvertToOutputType(DbStructure data)
        {
            return new NetStructure
            {
                tribe_id = data.tribe_id,
                classname = data.classname,
                has_inventory = data.has_inventory,
                location = data.location,
                structure_id = data.structure_id
            };
        }

        public override FilterDefinition<DbStructure> GetFilterDefinition(DateTime epoch)
        {
            var filterBuilder = Builders<DbStructure>.Filter;
            var filter = GetServerTribeFilter<DbStructure>();
            return filter;
        }

        public override IMongoCollection<DbStructure> GetMongoCollection()
        {
            return conn.content_structures;
        }

        public override async Task WriteResponse(List<DbStructure> adds, List<DbStructure> removes, int epoch, string format)
        {
            if (format == "json")
                await WriteJSONResponse(adds, removes, epoch);
            else if (format == "binary")
                await WriteBinaryResponse(adds, removes, epoch);
            else
                await ExitInvalidFormat("json", "binary");
        }

        public async Task WriteBinaryResponse(List<DbStructure> adds, List<DbStructure> removes, int epoch)
        {
            //Binary mode writes out structures in binary structs

            //HEADER FORMAT:
            //  4   static  Signature, says "DWMS"
            //  4   int32   File type version, should be 3
            //  4   int32   Metadata version
            //  4   int32   Structure count

            //  4   int32   Saved epoch
            //  4   int32   RESERVED
            //  4   int32   RESERVED
            //  4   int32   RESERVED
            //TOTAL: 32 bytes

            //Struct format:
            //  2   short   Metadata Index
            //  1   byte    Rotation (in degrees, 0-360, but scaled so that 256=360)
            //  1   byte    Flags (SEE BELOW)
            //  4   float   Position X
            //  4   float   Position Y
            //  4   int32   ID
            //  4   float   Position Z
            //  4   int32   Tribe ID
            //TOTAL: 24 bytes each

            //FLAGS
            //0: Has inventory
            //1: Is remove
            //2: RESERVED
            //3: RESERVED
            //4: RESERVED
            //5: RESERVED
            //6: RESERVED
            //7: RESERVED

            //Combine structures
            List<DbStructure> structures = new List<DbStructure>();
            structures.AddRange(adds);
            structures.AddRange(removes);

            //Set headers
            e.Response.ContentLength = (24 * structures.Count) + 32;
            e.Response.ContentType = "application/octet-stream";
            e.Response.StatusCode = 200;

            //Create and send file header
            byte[] buf = new byte[32];
            buf[0] = 0x44;
            buf[1] = 0x57;
            buf[2] = 0x4D;
            buf[3] = 0x53;
            BinaryTool.WriteInt32(buf, 4, 3); //Version tag
            BinaryTool.WriteInt32(buf, 8, 0);
            BinaryTool.WriteInt32(buf, 12, structures.Count);

            BinaryTool.WriteInt32(buf, 16, epoch);
            BinaryTool.WriteInt32(buf, 20, 0);
            BinaryTool.WriteInt32(buf, 24, 0);
            BinaryTool.WriteInt32(buf, 28, 0);

            await e.Response.Body.WriteAsync(buf, 0, 32);

            //Loop through structures and find where they are
            foreach (var t in structures)
            {
                //Get data
                StructureMetadata metadata = Program.conn.GetStructureMetadata().Where(x => x.names.Contains(t.classname)).FirstOrDefault();
                int index = Program.conn.GetStructureMetadata().IndexOf(metadata);

                //Produce flags
                byte flags = 0;
                if(t.has_inventory)
                    flags |= 0x01 << 0;
                if (removes.Contains(t))
                    flags |= 0x01 << 1; //This is a remove, not an add

                //Write parts
                BinaryTool.WriteInt16(buf, 0, (short)index);
                buf[2] = (byte)(t.location.yaw * 0.70833333333333333333333333333333f); //Scales this to fit the 0-360 degrees into 0-255
                buf[3] = flags;
                BinaryTool.WriteFloat(buf, 4, t.location.x);
                BinaryTool.WriteFloat(buf, 8, t.location.y);
                BinaryTool.WriteInt32(buf, 12, t.structure_id);
                BinaryTool.WriteFloat(buf, 16, t.location.z);
                BinaryTool.WriteInt32(buf, 20, t.tribe_id);

                //Write to stream
                await e.Response.Body.WriteAsync(buf, 0, 24);
            }
        }
    }
}
