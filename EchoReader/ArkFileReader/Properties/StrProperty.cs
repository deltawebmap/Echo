using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EchoReader.ArkFileReader.Properties
{
    public class StrProperty : BaseProperty
    {
        public string data;

        public override async Task Read(string name, int index, int size, ArkFile ark)
        {
            data = await ark.io.DirectReadUEString();
        }

        public override async Task Skip(string name, int index, int size, ArkFile ark)
        {
            data = await ark.io.DirectReadUEString();
        }
    }
}
