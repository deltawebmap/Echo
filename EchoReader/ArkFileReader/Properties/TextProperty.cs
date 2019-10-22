using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EchoReader.ArkFileReader.Properties
{
    public class TextProperty : BaseProperty
    {
        public string data;

        public override async Task Read(string name, int index, int size, ArkFile ark)
        {
            await ark.io.ReadBuffer(size);
            data = Convert.ToBase64String(ark.io.streamBuffer);
        }

        public override async Task Skip(string name, int index, int size, ArkFile ark)
        {
            await ark.io.FastForwardOffset(size);
        }
    }
}
