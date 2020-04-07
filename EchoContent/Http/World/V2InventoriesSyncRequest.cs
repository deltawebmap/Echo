using LibDeltaSystem;
using LibDeltaSystem.Db.Content;
using LibDeltaSystem.Entities.CommonNet;
using LibDeltaSystem.Entities.DynamicTiles;
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
    public class V2InventoriesSyncRequest : V2SyncDeltaService<DbInventory, NetInventory>
    {
        public V2InventoriesSyncRequest(DeltaConnection conn, HttpContext e) : base(conn, e)
        {
        }

        public override NetInventory ConvertToOutputType(DbInventory data)
        {
            return NetInventory.ConvertInventory(data);
        }

        public override FilterDefinition<DbInventory> GetFilterDefinition(DateTime epoch)
        {
            var filterBuilder = Builders<DbInventory>.Filter;
            var filter = FilterBuilderToolDb.CreateTribeFilter<DbInventory>(server, tribeId);
            return filter;
        }

        public override IMongoCollection<DbInventory> GetMongoCollection()
        {
            return conn.content_inventories;
        }

        public override async Task WriteResponse(List<DbInventory> adds, List<DbInventory> removes, int epoch, string format)
        {
            if (format == "json")
                await WriteJSONResponse(adds, removes, epoch);
            else if (format == "binary")
                await WriteBinaryResponse(adds, removes, epoch);
            else
                await ExitInvalidFormat("json", "binary");
        }

        public async Task WriteBinaryResponse(List<DbInventory> adds, List<DbInventory> removes, int epoch)
        {
            //Binary mode writes out structures in binary structs
            //In format header, name table, inventory struct

            //HEADER FORMAT:
            //  4   static  Signature, says "DWMI"
            //  4   int32   File type version, should be 1
            //  4   int32   RESERVED
            //  4   int32   Inventory count

            //  4   int32   Saved epoch
            //  4   int32   Name table entry count
            //  4   int32   RESERVED
            //  4   int32   RESERVED
            //TOTAL: 32 bytes

            //The name table comes next, and is just a list of classnames, with a byte indicating their length before

            //Inventory Struct format:
            //  8   int64   Holder ID
            //  1   byte    Holder type
            //  2   int16   Item count
            //  4   int32   Tribe ID
            //  ?   Item[]  Items
            //TOTAL: VARIABLE

            //Item struct format:
            //  8   int64   Item ID
            //  4   int32   Name table classname index
            //  4   float   Durability
            //  2   int16   Stack size
            //  2   int16   Flags
            //  1   byte    Custom data count
            //  ?   CD[]    Custom datas, number specified above
            //Total: VARIABLE

            //CD (Custom Data) format:
            //  2   int16   Tag
            //  2   int16   String length
            //  ?   string  Data

            //Combine items
            List<DbInventory> inventories = new List<DbInventory>();
            inventories.AddRange(adds);
            inventories.AddRange(removes);

            //Set headers
            e.Response.ContentType = "application/octet-stream";
            e.Response.StatusCode = 200;

            //Create name table
            List<string> names = new List<string>();
            foreach(var i in inventories)
            {
                foreach(var ii in i.items)
                {
                    if (!names.Contains(ii.classname))
                        names.Add(ii.classname);
                }
            }

            //Create and send file header
            byte[] buf = new byte[16384];
            buf[0] = 0x44;
            buf[1] = 0x57;
            buf[2] = 0x4D;
            buf[3] = 0x49;
            BinaryTool.WriteInt32(buf, 4, 1); //Version tag
            BinaryTool.WriteInt32(buf, 8, 0);
            BinaryTool.WriteInt32(buf, 12, inventories.Count);
            BinaryTool.WriteInt32(buf, 16, epoch);
            BinaryTool.WriteInt32(buf, 20, names.Count);
            BinaryTool.WriteInt32(buf, 24, 0);
            BinaryTool.WriteInt32(buf, 28, 0);
            await e.Response.Body.WriteAsync(buf, 0, 32);

            //Send name table
            foreach(var name in names)
            {
                //Write to buffer
                byte[] d = Encoding.UTF8.GetBytes(name);
                buf[0] = (byte)d.Length;
                if (d.Length > byte.MaxValue)
                    throw new Exception("Encoding failed, name is too long!");
                Array.Copy(d, 0, buf, 1, d.Length);

                //Send on network
                await e.Response.Body.WriteAsync(buf, 0, d.Length + 1);
            }

            //Send inventories
            foreach(var inventory in inventories)
            {
                //Create header
                BinaryTool.WriteInt64(buf, 0, inventory.holder_id);
                buf[8] = (byte)inventory.holder_type;
                BinaryTool.WriteInt16(buf, 9, (short)inventory.items.Length);
                BinaryTool.WriteInt32(buf, 11, inventory.tribe_id);
                int offset = 15;

                //Add items
                foreach(var i in inventory.items)
                {
                    //Create header
                    BinaryTool.WriteInt64(buf, offset, i.item_id);
                    BinaryTool.WriteInt32(buf, offset + 8, names.IndexOf(i.classname));
                    BinaryTool.WriteFloat(buf, offset + 12, i.durability);
                    BinaryTool.WriteInt16(buf, offset + 16, (short)i.stack_size);
                    BinaryTool.WriteInt16(buf, offset + 18, (short)i.flags);
                    buf[offset + 20] = (byte)i.custom_data.Count;
                    offset += 21;

                    //Write custom datas
                    foreach(var c in i.custom_data)
                    {
                        BinaryTool.WriteInt16(buf, offset, (short)c.Key);
                        byte[] d = Encoding.UTF8.GetBytes(c.Value);
                        BinaryTool.WriteInt16(buf, offset + 2, (short)d.Length);
                        Array.Copy(d, 0, buf, offset + 4, d.Length);
                        offset += d.Length + 4;
                    }
                }

                //Send data
                await e.Response.Body.WriteAsync(buf, 0, offset);
            }
        }
    }
}
