using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EchoReader.ArkFileReader.Properties
{
    public class ByteProperty : BaseProperty
    {
        public string enumName;
        public bool normalByte;

        /* VALUES */
        public string enumValue;
        public byte byteValue;

        public override async Task Read(string name, int index, int size, ArkFile ark)
        {
            //Read the enum name
            await ark.io.ReadBuffer(8);
            enumName = ark.io.ReadNameTableIndex();
            normalByte = enumName == "None";

            //Now, read based on that
            if (normalByte)
            {
                await ark.io.ReadBuffer(1);
                byteValue = ark.io.streamBuffer[0];
            }
            else
            {
                await ark.io.ReadBuffer(8);
                enumValue = ark.io.ReadNameTableIndex();
            }
        }

        public override async Task Skip(string name, int index, int size, ArkFile ark)
        {
            await ark.io.FastForwardOffset(size + 8);
        }
    }
}
