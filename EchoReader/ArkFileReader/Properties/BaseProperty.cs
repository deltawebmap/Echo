using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EchoReader.ArkFileReader.Properties
{
    public abstract class BaseProperty
    {
        public abstract Task Read(string name, int index, int size, ArkFile ark);
        public abstract Task Skip(string name, int index, int size, ArkFile ark);
    }
}
