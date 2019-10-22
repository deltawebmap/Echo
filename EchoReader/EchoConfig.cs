using System;
using System.Collections.Generic;
using System.Text;

namespace EchoReader
{
    public class EchoConfig
    {
        public string content_uploads_path = @"E:\EchoReader\content\"; //Path where we'll upload put files
        public string temp_file_path = @"E:\EchoReader\temp\";
        public string pdp_file = @"C:\Users\Roman\Downloads\Delta Web Map Landing Imgs\primal_data.pdp";
        public string db_config = @"E:\database_config.json";
        public string key;
        public int port = 43298;
    }
}
