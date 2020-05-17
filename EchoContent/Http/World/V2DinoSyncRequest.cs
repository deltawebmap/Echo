﻿using LibDeltaSystem;
using LibDeltaSystem.Db.Content;
using LibDeltaSystem.Entities.CommonNet;
using LibDeltaSystem.Tools;
using LibDeltaSystem.Tools.DeltaWebFormat;
using Microsoft.AspNetCore.Http;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace EchoContent.Http.World
{
    public class V2DinoSyncRequest : V2SyncDeltaService<DbDino, NetDino>
    {
        public V2DinoSyncRequest(DeltaConnection conn, HttpContext e) : base(conn, e)
        {
        }

        public override NetDino ConvertToOutputType(DbDino data)
        {
            return NetDino.ConvertDbDino(data);
        }

        public override FilterDefinition<DbDino> GetFilterDefinition(DateTime epoch)
        {
            var filterBuilder = Builders<DbDino>.Filter;
            var filter = filterBuilder.Eq("is_tamed", true) & GetServerTribeFilter<DbDino>();
            return filter;
        }

        public override IMongoCollection<DbDino> GetMongoCollection()
        {
            return conn.content_dinos;
        }

        public override async Task WriteResponse(List<DbDino> adds, List<DbDino> removes, int epoch, string format)
        {
            if (format == "json")
                await WriteJSONResponse(adds, removes, epoch);
            else if (format == "binary")
                await WriteBinaryResponse(adds, removes, epoch);
            else
                await ExitInvalidFormat("json", "binary");
        }

        public async Task WriteBinaryResponse(List<DbDino> adds, List<DbDino> removes, int epoch)
        {
            //Convert
            var addsConverted = MassConvertObjects(adds);

            using(MemoryStream ms = new MemoryStream())
            {
                //Encode
                DeltaWebFormatEncoder<NetDino> encoder = new LibDeltaSystem.Tools.DeltaWebFormat.DeltaWebFormatEncoder<NetDino>(ms);
                try
                {
                    encoder.Encode(addsConverted, new Dictionary<byte, byte[]>());
                } catch (Exception ex)
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
