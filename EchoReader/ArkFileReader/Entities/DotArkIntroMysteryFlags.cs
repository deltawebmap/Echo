using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EchoReader.ArkFileReader.Entities
{
    class DotArkIntroMysteryFlags
    {
        public int flags;
        public int objectCount;
        public string nameString;

        public async Task Read(ArkFile f)
        {
            await f.io.ReadBuffer(8);
            flags = f.io.ReadInt32();
            objectCount = f.io.ReadInt32();
            nameString = await f.io.DirectReadUEString();
        }
    }
}
