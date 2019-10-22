using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EchoReader.ArkFileReader.Properties
{
    public class DoubleProperty : BaseProperty
    {
        public double data;

        public override async Task Read(string name, int index, int size, ArkFile ark)
        {
            await ark.io.ReadBuffer(8);
            data = ark.io.ReadDouble();
        }

        public override async Task Skip(string name, int index, int size, ArkFile ark)
        {
            await ark.io.FastForwardOffset(8);
        }
    }
}
