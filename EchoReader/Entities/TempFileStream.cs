using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace EchoReader.Entities
{
    public class TempFileStream : FileStream
    {
        public string tempPath;

        public TempFileStream(string tempPath) : base(tempPath, FileMode.Create)
        {
            //Set vars
            this.tempPath = tempPath;
        }

        public override void Close()
        {
            //Run base close
            base.Close();

            //Delete the temporary file
            File.Delete(tempPath);
        }

        public static TempFileStream OpenTemp()
        {
            //Generating a random string
            string s = Program.GenerateRandomString(32);
            while(File.Exists(Program.config.temp_file_path + s))
                s = Program.GenerateRandomString(32);

            //Create stream
            return new TempFileStream(Program.config.temp_file_path + s);
        }
    }
}
