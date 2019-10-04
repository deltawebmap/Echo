using ArkSaveEditor.Deserializer.DotArk;
using ArkSaveEditor.Serializer.DotArk;
using System;
using System.Collections.Generic;
using System.Text;

namespace ArkSaveEditor.Entities.LowLevel.DotArk.ArkProperties
{
    public class UInt16Property : DotArkProperty
    {
        public UInt16Property(DotArkDeserializer d, int index, int length)
        {
            var ms = d.ms;
            dataFilePosition = ms.position;
            this.data = ms.ReadUShort();
        }

        public override void WriteProp(DotArkSerializerInstance s, DotArkGameObject go, DotArkFile f, IOMemoryStream ms)
        {
            base.WriteProp(s, go, f, ms);

            //Write UInt16
            ms.WriteUShort((UInt16)data);
        }

        public override int WriteToHashBuffer(byte[] buf, int pos)
        {
            byte[] b = BitConverter.GetBytes((UInt16)data);
            b.CopyTo(buf, pos);
            return pos + b.Length;
        }
    }
}
