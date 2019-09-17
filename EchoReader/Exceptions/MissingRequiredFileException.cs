using EchoReader.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace EchoReader.Exceptions
{
    public class MissingRequiredFileException : BaseError
    {
        public MissingRequiredFileException(string filename, ArkUploadedFileType type)
        {
            this.msg = "Missing a required file '" + filename + "' of type "+type.ToString()+".";
            this.code = 500;
        }
    }
}
