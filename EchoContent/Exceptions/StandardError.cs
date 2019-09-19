using System;
using System.Collections.Generic;
using System.Text;

namespace EchoContent.Exceptions
{
    public class StandardError : Exception
    {
        public string msg;
        public string msg_more;
        public int http_code;

        public StandardError(string msg, string msg_more, int http_code = 500)
        {
            this.msg = msg;
            this.msg_more = msg_more;
            this.http_code = http_code;
        }
    }
}
