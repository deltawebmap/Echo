﻿using ArkSaveEditor.Deserializer.DotArk;
using ArkSaveEditor.Serializer.DotArk;
using System;
using System.Collections.Generic;
using System.Text;

namespace ArkSaveEditor.Entities.LowLevel.DotArk.ArkProperties
{
    public class FloatProperty : DotArkProperty
    {
        public FloatProperty(DotArkDeserializer d, int index, int length)
        {
            var ms = d.ms;
            dataFilePosition = ms.position;
            this.data = ms.ReadFloat();
        }

        public override void WriteProp(DotArkSerializerInstance s, DotArkGameObject go, DotArkFile f, IOMemoryStream ms)
        {
            base.WriteProp(s, go, f, ms);

            //Write float
            ms.WriteFloat((float)data);
        }

        public override int WriteToHashBuffer(byte[] buf, int pos)
        {
            byte[] b = BitConverter.GetBytes((float)data);
            b.CopyTo(buf, pos);
            return pos + b.Length;
        }
    }
}
