using EchoContent.Http.Charlie.Definitions;
using EchoContent.Http.World.Definitions;
using LibDeltaSystem;
using LibDeltaSystem.Db.Content;
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

        static void Main(string[] args)
        {
            //Connect to database
            conn = DeltaConnection.InitDeltaManagedApp(args, 0, 3, new EchoContentCoreNetwork());

            //Start server
            DeltaWebServer server = new DeltaWebServer(conn, conn.GetUserPort(0));
            server.exposedHeaders.Add("X-Delta-Sync-TotalItems");
            server.AddService(new StructureMetadataDefinition());
            server.AddService(new V2DinoSyncDefinition());
            server.AddService(new V2SpeciesSyncDefinition());
            server.AddService(new V2ItemDefinitionsSyncDefinition());
            server.AddService(new V2StructuresSyncDefinition());
            server.AddService(new V2InventoriesSyncDefinition());
            server.RunAsync().GetAwaiter().GetResult();
        }
    }

    public enum RequestHttpMethod
    {
        get,
        post,
        put,
        delete,
        options
    }
}
