using EchoReader.ArkFileReader.Entities;
using EchoReader.ArkFileReader.Properties;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EchoReader.ArkFileReader.Structs
{
    public class ArkStructProps : BaseArkStruct
    {
        public Dictionary<string, BaseProperty> props;

        public override async Task Read(ArkFile ark)
        {
            //Create data
            props = new Dictionary<string, BaseProperty>();

            //Read all props
            while (true)
            {
                //Read name info into buffer
                await ark.io.ReadBuffer(8);

                //Read header info
                string name = ark.io.ReadNameTableIndex(out int nameIndex);

                //If the name is none, stop
                if (name == "None")
                    break;

                //Read the type
                await ark.io.ReadBuffer(16);
                string type = ark.io.ReadNameTableIndex();
                if (!type.EndsWith("Property"))
                    throw new Exception("Failed to read object data: Invalid type " + type);

                //Read the size and index
                int size = ark.io.ReadInt32();
                int index = ark.io.ReadInt32();

                //Get type
                BaseProperty prop = ArkGameObjectHead.GetPropertyFromType(type);

                //Read
                await prop.Read(name, index, size, ark);

                //Add
                props.Add(name, prop);
            }
        }
    }
}
