using EchoContent.Http.World.Definitions;
using LibDeltaSystem;
using LibDeltaSystem.Db.Content;
using LibDeltaSystem.Entities;
using LibDeltaSystem.WebFramework;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using MongoDB.Driver;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace EchoContent
{
    class Program
    {
        public static DeltaConnection conn;
        public static List<StructureMetadata> structureMetadata;

        static void Main(string[] args)
        {
            //Connect to database
            conn = DeltaConnection.InitDeltaManagedApp(args, DeltaCoreNetServerType.API_ECHO, 0, 9);

            //Get structure metadata
            structureMetadata = conn.GetStructureMetadata().GetAwaiter().GetResult();

            //Start server
            DeltaWebServer server = new DeltaWebServer(conn, conn.GetUserPort(0));
            server.exposedHeaders.Add("X-Delta-Sync-TotalItems");
            server.AddService(new StructureMetadataDefinition());
            server.AddService(new V2DinoSyncDefinition());
            server.AddService(new V2StructuresSyncDefinition());
            server.AddService(new V2InventoriesSyncDefinition());
            server.AddService(new WorldPlayerListDefinition());
            server.RunAsync().GetAwaiter().GetResult();
        }
    }
}
