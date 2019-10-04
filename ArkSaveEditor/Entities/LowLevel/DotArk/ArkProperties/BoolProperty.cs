using ArkSaveEditor.Deserializer.DotArk;
using ArkSaveEditor.Serializer.DotArk;
using System;
using System.Collections.Generic;
using System.Text;

namespace ArkSaveEditor.Entities.LowLevel.DotArk.ArkProperties
{
    public class BoolProperty : DotArkProperty
    {
        public BoolProperty(DotArkDeserializer d, int index, int length)
        {
            var ms = d.ms;
            dataFilePosition = ms.position;
            this.data = ms.ReadByte() != 0;
        }

        public override void WriteProp(DotArkSerializerInstance s, DotArkGameObject go, DotArkFile f, IOMemoryStream ms)
        {
            base.WriteProp(s, go, f, ms);

            byte value = 0;
            if ((bool)data)
                value = 1;

            //Write
            ms.WriteByte(value);
        }

        public override int WriteToHashBuffer(byte[] buf, int pos)
        {
            if((bool)this.data == true)
                buf[pos] = 0x01;
            else
                buf[pos] = 0x00;
            return pos + 1;
        }
    }
}
