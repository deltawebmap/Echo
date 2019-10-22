using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EchoReader.ArkFileReader.Properties
{
    public class IntProperty : BaseProperty
    {
        public int data;

        public override async Task Read(string name, int index, int size, ArkFile ark)
        {
            await ark.io.ReadBuffer(4);
            data = ark.io.ReadInt32();
        }

        public override async Task Skip(string name, int index, int size, ArkFile ark)
        {
            await ark.io.FastForwardOffset(4);
        }
    }
}
