using System;
using System.Collections.Generic;
using System.Text;

namespace EchoReader.Exceptions
{
    public class BaseError : Exception
    {
        public string msg;
        public int code;
    }
}
