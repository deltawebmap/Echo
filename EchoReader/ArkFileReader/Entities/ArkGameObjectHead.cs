using EchoReader.ArkFileReader.Properties;
using LibDeltaSystem.Db.Content;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EchoReader.ArkFileReader.Entities
{
    public class ArkGameObjectHead
    {
        public Guid guid;
        public string classname;
        public int classnameIndex;
        public bool isItem;
        public bool unknown1;
        public int unknown2;
        public DbLocation location;
        public int propDataOffset;
        public int unknown3;

        public int listIndex; //The position in the GameObject list this uses

        public async Task Open(ArkFile ark, PropertyReaderInterface i)
        {
            //Seek to this position
            await ark.io.FastForwardToPosition(propDataOffset + ark.properties_block_offset);

            //Keep reading properties
            StreamPropertyCallback callback;
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
                BaseProperty prop = GetPropertyFromType(type);

                //Check if we should read this type or not
                callback = i.GetPropertyCallback(name, type);
                if (callback != null)
                {
                    await prop.Read(name, index, size, ark);
                    callback(prop, name, type, index);
                }
                else
                    await prop.Skip(name, index, size, ark);
            }
        }

        public static BaseProperty GetPropertyFromType(string type)
        {
            BaseProperty prop;
            switch (type)
            {
                case "IntProperty":
                    prop = new IntProperty();
                    break;
                case "UInt32Property":
                    prop = new UInt32Property();
                    break;
                case "Int8Property":
                    prop = new Int8Property();
                    break;
                case "Int16Property":
                    prop = new Int16Property();
                    break;
                case "UInt16Property":
                    prop = new UInt16Property();
                    break;
                case "UInt64Property":
                    prop = new UInt64Property();
                    break;
                case "BoolProperty":
                    prop = new BoolProperty();
                    break;
                case "ByteProperty":
                    prop = new ByteProperty();
                    break;
                case "FloatProperty":
                    prop = new FloatProperty();
                    break;
                case "DoubleProperty":
                    prop = new DoubleProperty();
                    break;
                case "NameProperty":
                    prop = new NameProperty();
                    break;
                case "ObjectProperty":
                    prop = new ObjectProperty();
                    break;
                case "StrProperty":
                    prop = new StrProperty();
                    break;
                case "StructProperty":
                    prop = new StructProperty();
                    break;
                case "ArrayProperty":
                    prop = new ArrayProperty();
                    break;
                case "TextProperty":
                    prop = new TextProperty();
                    break;
                default:
                    //Unknown
                    throw new Exception($"Couldn't read object data: Type {type} was not a valid property type. Something failed to read.");
            }
            return prop;
        }
    }
}
