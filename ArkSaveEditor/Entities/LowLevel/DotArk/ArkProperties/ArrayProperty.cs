using ArkSaveEditor.Deserializer.DotArk;
using ArkSaveEditor.Serializer.DotArk;
using System;
using System.Collections.Generic;
using System.Text;

namespace ArkSaveEditor.Entities.LowLevel.DotArk.ArkProperties
{
    public class ArrayProperty<T> : DotArkProperty
    {
        public ArkClassName arrayType;
        public List<T> items;

        public ArrayProperty()
        {
            
        }

        public override void WriteProp(DotArkSerializerInstance s, DotArkGameObject go, DotArkFile f, IOMemoryStream ms)
        {
            //For now, fake this and pretend this is an empty array
            //TODO: Add ArrayProperty to saveable properties.
            size = 4;

            base.WriteProp(s, go, f, ms);
            ms.WriteArkClassname(arrayType, s);
            ms.WriteInt(0);
        }

        public override int WriteToHashBuffer(byte[] buf, int pos)
        {
            /*foreach(var i in items)
            {
                var ii = (DotArkProperty)Convert.ChangeType(i, typeof(DotArkProperty));
                pos = ii.WriteToHashBuffer(buf, pos);
            }*/

            //TODO
            return pos;
        }
    }
}
