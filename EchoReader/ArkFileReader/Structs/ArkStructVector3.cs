using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EchoReader.ArkFileReader.Structs
{
    public class ArkStructVector3 : BaseArkStruct
    {
        public float x;
        public float y;
        public float z;

        public override async Task Read(ArkFile ark)
        {
            await ark.io.ReadBuffer(4 * 3);
            x = ark.io.ReadFloat();
            y = ark.io.ReadFloat();
            z = ark.io.ReadFloat();
        }
    }
}
