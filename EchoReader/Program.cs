using EchoEntities.Db;
using EchoReader.Entities;
using LiteDB;
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

namespace EchoReader
{
    class Program
    {
        public static Random rand;
        public static LiteDatabase data_db;
        public static LiteCollection<ArkServerSerialized> servers_collection;
        public static Dictionary<string, ArkServer> cache; //Loaded servers in memory

        public static MongoClient content_client;
        public static IMongoDatabase content_database;
        public static IMongoCollection<DbDino> content_dinos;
        public static IMongoCollection<DbItem> content_items;
        public static IMongoCollection<DbTribe> content_tribes;
        public static IMongoCollection<DbPlayerProfile> content_player_profiles;

        public static EchoConfig config = new EchoConfig();

        static void Main(string[] args)
        {
            //Init everything
            rand = new Random();
            data_db = new LiteDatabase(config.data_db);
            servers_collection = data_db.GetCollection<ArkServerSerialized>("servers");
            cache = new Dictionary<string, ArkServer>();

            //Clean up temporary files
            string[] tempClean = Directory.GetFiles(config.temp_file_path);
            foreach (string f in tempClean)
                File.Delete(f);

            //Load PrimalData. This is temporary
            using (FileStream fs = new FileStream(@"C:\Users\Roman\source\repos\ArkWebMap\backend\ArkHttpServer\bin\Debug\netcoreapp2.1\primal_data.pdp", System.IO.FileMode.Open, FileAccess.Read))
            {
                //Load package
                bool ok = ArkSaveEditor.ArkImports.ImportContentFromPackage(fs, (ArkSaveEditor.Entities.PrimalDataPackageMetadata metadata) =>
                {
                    return true;
                });
            }

            content_client = new MongoClient(
                "mongodb://localhost:27017"
            );
            content_database = content_client.GetDatabase("delta-staging");
            content_dinos = content_database.GetCollection<DbDino>("dinos");
            content_items = content_database.GetCollection<DbItem>("items");
            content_tribes = content_database.GetCollection<DbTribe>("tribes");
            content_player_profiles = content_database.GetCollection<DbPlayerProfile>("player_profiles");

            //Start server
            MainAsync().GetAwaiter().GetResult();
        }

        public static Task MainAsync()
        {
            var host = new WebHostBuilder()
                .UseKestrel(options =>
                {
                    IPAddress addr = IPAddress.Any;
                    options.Listen(addr, 43298);

                })
                .UseStartup<Program>()
                .Build();

            return host.RunAsync();
        }

        public void Configure(IApplicationBuilder app)
        {
            app.Run(Http.HttpHandler.OnHttpRequest);
        }

        public static Task QuickWriteToDoc(Microsoft.AspNetCore.Http.HttpContext context, string content, string type = "text/html", int code = 200)
        {
            var response = context.Response;
            response.StatusCode = code;
            response.ContentType = type;

            //Load the template.
            string html = content;
            var data = Encoding.UTF8.GetBytes(html);
            response.ContentLength = data.Length;
            return response.Body.WriteAsync(data, 0, data.Length);
        }

        public static string GetPostBodyString(Microsoft.AspNetCore.Http.HttpContext context)
        {
            string buffer;
            using (StreamReader sr = new StreamReader(context.Request.Body))
                buffer = sr.ReadToEnd();

            return buffer;
        }

        public static T DecodePostBody<T>(Microsoft.AspNetCore.Http.HttpContext context)
        {
            string buffer = GetPostBodyString(context);

            //Deserialize
            return JsonConvert.DeserializeObject<T>(buffer);
        }

        public static Task QuickWriteJsonToDoc<T>(Microsoft.AspNetCore.Http.HttpContext context, T data, int code = 200)
        {
            return QuickWriteToDoc(context, JsonConvert.SerializeObject(data, Formatting.Indented), "application/json", code);
        }

        public static string GenerateRandomString(int length)
        {
            return GenerateRandomStringCustom(length, "qwertyuiopasdfghjklzxcvbnm1234567890QWERTYUIOPASDFGHJKLZXCVBNM".ToCharArray());
        }

        public static string GenerateRandomStringCustom(int length, char[] chars)
        {
            string output = "";
            for (int i = 0; i < length; i++)
            {
                output += chars[rand.Next(0, chars.Length)];
            }
            return output;
        }

        public static byte[] GenerateRandomBytes(int length)
        {
            byte[] buf = new byte[length];
            rand.NextBytes(buf);
            return buf;
        }

        public static RequestHttpMethod FindRequestMethod(Microsoft.AspNetCore.Http.HttpContext context)
        {
            return Enum.Parse<RequestHttpMethod>(context.Request.Method.ToLower());
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
