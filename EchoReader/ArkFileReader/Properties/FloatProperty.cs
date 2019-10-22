using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EchoReader.ArkFileReader.Properties
{
    public class FloatProperty : BaseProperty
    {
        public float data;

        public override async Task Read(string name, int index, int size, ArkFile ark)
        {
            await ark.io.ReadBuffer(4);
            data = ark.io.ReadFloat();
        }

        public override async Task Skip(string name, int index, int size, ArkFile ark)
        {
            await ark.io.FastForwardOffset(4);
        }
    }
}
