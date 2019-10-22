using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EchoReader.ArkFileReader.Structs
{
    public abstract class BaseArkStruct
    {
        public abstract Task Read(ArkFile ark);
    }
}
