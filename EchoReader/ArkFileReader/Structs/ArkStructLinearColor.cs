using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EchoReader.ArkFileReader.Structs
{
    public class ArkStructLinearColor : BaseArkStruct
    {
        public float r;
        public float g;
        public float b;
        public float a;

        public override async Task Read(ArkFile ark)
        {
            await ark.io.ReadBuffer(4 * 4);
            r = ark.io.ReadFloat();
            g = ark.io.ReadFloat();
            b = ark.io.ReadFloat();
            a = ark.io.ReadFloat();
        }
    }
}
