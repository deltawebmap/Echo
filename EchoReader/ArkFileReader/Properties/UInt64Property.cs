using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EchoReader.ArkFileReader.Properties
{
    public class UInt64Property : BaseProperty
    {
        public UInt64 data;

        public override async Task Read(string name, int index, int size, ArkFile ark)
        {
            await ark.io.ReadBuffer(8);
            data = ark.io.ReadUInt64();
        }

        public override async Task Skip(string name, int index, int size, ArkFile ark)
        {
            await ark.io.FastForwardOffset(8);
        }
    }
}
