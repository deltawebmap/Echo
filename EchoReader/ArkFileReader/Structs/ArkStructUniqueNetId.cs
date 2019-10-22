using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EchoReader.ArkFileReader.Structs
{
    public class ArkStructUniqueNetId : BaseArkStruct
    {
        public int unk;
        public string netId;

        public override async Task Read(ArkFile ark)
        {
            await ark.io.ReadBuffer(4);
            unk = ark.io.ReadInt32();
            netId = await ark.io.DirectReadUEString();
        }
    }
}
