using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EchoReader.ArkFileReader.Structs
{
    public class ArkStructColor : BaseArkStruct
    {
        public byte b;
        public byte g;
        public byte r;
        public byte a;

        public override async Task Read(ArkFile ark)
        {
            await ark.io.ReadBuffer(4);
            b = ark.io.ReadByte();
            g = ark.io.ReadByte();
            r = ark.io.ReadByte();
            a = ark.io.ReadByte();
        }
    }
}
