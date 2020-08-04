using LibDeltaSystem;
using LibDeltaSystem.Db.ArkEntries;
using LibDeltaSystem.Entities.ArkEntries.Dinosaur;
using LibDeltaSystem.Tools.DeltaWebFormat;
using Microsoft.AspNetCore.Http;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace EchoContent.Http.Charlie
{
    public class V2ItemDefinitionsSyncRequest : V2SyncDeltaService<DbArkEntry<ItemEntry>, ItemEntry>
    {
        public V2ItemDefinitionsSyncRequest(DeltaConnection conn, HttpContext e) : base(conn, e)
        {
        }

        public override ItemEntry ConvertToOutputType(DbArkEntry<ItemEntry> data)
        {
            return data.data;
        }

        public override FilterDefinition<DbArkEntry<ItemEntry>> GetFilterDefinition(DateTime epoch)
        {
            var filterBuilder = Builders<DbArkEntry<ItemEntry>>.Filter;
            return filterBuilder.Gt("time", epoch);
        }

        public override IMongoCollection<DbArkEntry<ItemEntry>> GetMongoCollection()
        {
            return conn.arkentries_items;
        }

        public override async Task<bool> OnPreRequest()
        {
            //Replaces the built in one from earlier
            return true;
        }

        public override async Task<bool> SetArgs(Dictionary<string, string> args)
        {
            //Replaces the built in one from earlier
            return true;
        }

        public override async Task WriteResponse(List<DbArkEntry<ItemEntry>> adds, int epoch, string format)
        {
            if (format == "json")
                await WriteJSONResponse(adds, epoch);
            else if (format == "binary")
                await WriteBinaryResponse(adds, epoch);
            else
                await ExitInvalidFormat("json", "binary");
        }

        public async Task WriteBinaryResponse(List<DbArkEntry<ItemEntry>> adds, int epoch)
        {
            //Convert
            var addsConverted = MassConvertObjects(adds);

            using (MemoryStream ms = new MemoryStream())
            {
                //Encode
                DeltaWebFormatEncoder<ItemEntry> encoder = new LibDeltaSystem.Tools.DeltaWebFormat.DeltaWebFormatEncoder<ItemEntry>(ms);
                try
                {
                    encoder.Encode(addsConverted, new Dictionary<byte, byte[]>()
                    {
                        {0, BitConverter.GetBytes(epoch) }
                    });
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message + ex.StackTrace);
                    e.Response.StatusCode = 500;
                }
                ms.Position = 0;
                e.Response.ContentType = "application/octet-stream";
                await ms.CopyToAsync(e.Response.Body);
            }
        }
    }
}
