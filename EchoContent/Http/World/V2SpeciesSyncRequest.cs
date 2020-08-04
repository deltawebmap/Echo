using LibDeltaSystem;
using LibDeltaSystem.Db.ArkEntries;
using LibDeltaSystem.Entities.ArkEntries.Dinosaur;
using LibDeltaSystem.Tools.DeltaWebFormat;
using LibDeltaSystem.WebFramework;
using Microsoft.AspNetCore.Http;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace EchoContent.Http.World
{
    public class V2SpeciesSyncRequest : V2SyncDeltaService<DbArkEntry<DinosaurEntry>, DinosaurEntry>
    {
        public V2SpeciesSyncRequest(DeltaConnection conn, HttpContext e) : base(conn, e)
        {
        }

        public override DinosaurEntry ConvertToOutputType(DbArkEntry<DinosaurEntry> data)
        {
            return data.data;
        }

        public override FilterDefinition<DbArkEntry<DinosaurEntry>> GetFilterDefinition(DateTime epoch)
        {
            var filterBuilder = Builders<DbArkEntry<DinosaurEntry>>.Filter;
            return filterBuilder.Gt("time", epoch);
        }

        public override IMongoCollection<DbArkEntry<DinosaurEntry>> GetMongoCollection()
        {
            return conn.arkentries_dinos;
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

        public override async Task WriteResponse(List<DbArkEntry<DinosaurEntry>> adds, int epoch, string format)
        {
            if (format == "json")
                await WriteJSONResponse(adds, epoch);
            else if (format == "binary")
                await WriteBinaryResponse(adds, epoch);
            else
                await ExitInvalidFormat("json", "binary");
        }

        public async Task WriteBinaryResponse(List<DbArkEntry<DinosaurEntry>> adds, int epoch)
        {
            //Convert
            var addsConverted = MassConvertObjects(adds);

            using (MemoryStream ms = new MemoryStream())
            {
                //Encode
                DeltaWebFormatEncoder<DinosaurEntry> encoder = new LibDeltaSystem.Tools.DeltaWebFormat.DeltaWebFormatEncoder<DinosaurEntry>(ms);
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
