using LibDeltaSystem;
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
        public static Random rand;
        public static DeltaConnection conn;
        public static EchoReaderConfig config;
        public static Dictionary<string, ArkSaveEditor.Entities.ArkMapData> ark_maps;

        public const string ROOT_URL = "https://echo-content.deltamap.net";

        static void Main(string[] args)
        {
            //Read config
            config = JsonConvert.DeserializeObject<EchoReaderConfig>(File.ReadAllText(args[0]));

            //Init everything
            rand = new Random();

            //Load PrimalData. This is temporary
            using (FileStream fs = new FileStream(config.pdp_file, System.IO.FileMode.Open, FileAccess.Read))
            {
                //Load package
                bool ok = ArkSaveEditor.ArkImports.ImportContentFromPackage(fs, (ArkSaveEditor.Entities.PrimalDataPackageMetadata metadata) =>
                {
                    return true;
                });
            }

            //Load maps
            ark_maps = JsonConvert.DeserializeObject<Dictionary<string, ArkSaveEditor.Entities.ArkMapData>>(File.ReadAllText(config.map_config_file));

            //Connect to database
            conn = new DeltaConnection(config.database_config_file, "echo-content", 0, 0);
            conn.Connect().GetAwaiter().GetResult();

            //Start server
            MainAsync().GetAwaiter().GetResult();
        }

        public static Task MainAsync()
        {
            var host = new WebHostBuilder()
                .UseKestrel(options =>
                {
                    IPAddress addr = IPAddress.Any;
                    options.Listen(addr, 43289);

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
