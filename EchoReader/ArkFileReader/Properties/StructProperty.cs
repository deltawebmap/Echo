using EchoReader.ArkFileReader.Structs;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EchoReader.ArkFileReader.Properties
{
    public class StructProperty : BaseProperty
    {
        public string structType;
        public BaseArkStruct structData;

        public override async Task Read(string name, int index, int size, ArkFile ark)
        {
            //Read type
            await ark.io.ReadBuffer(8);
            structType = ark.io.ReadNameTableIndex();

            //Read adta
            structData = await ReadStructFromStream(ark, structType);
        }

        public static async Task<BaseArkStruct> ReadStructFromStream(ArkFile ark, string typeName)
        {
            BaseArkStruct st;
            //First, we check known types for the struct property list. There could be other data, but it could fail.
            if (typeName == "ItemNetID" || typeName == "ItemNetInfo" || typeName == "Transform" || typeName == "PrimalPlayerDataStruct" || typeName == "PrimalPlayerCharacterConfigStruct" || typeName == "PrimalPersistentCharacterStatsStruct" || typeName == "TribeData" || typeName == "TribeGovernment" || typeName == "TerrainInfo" || typeName == "ArkInventoryData" || typeName == "DinoOrderGroup" || typeName == "ARKDinoData")
            {
                //Open this as a struct property list.
                st = new ArkStructProps();
            }
            else if (typeName == "Vector" || typeName == "Rotator")
            {
                //3d vector or rotor 
                st = new ArkStructVector3();
            }
            else if (typeName == "Vector2D")
            {
                //2d vector
                st = new ArkStructVector2();
            }
            else if (typeName == "Quat")
            {
                //Quat
                st = new ArkStructQuat();
            }
            else if (typeName == "Color")
            {
                //Color
                st = new ArkStructColor();
            }
            else if (typeName == "LinearColor")
            {
                //Linear color
                st = new ArkStructLinearColor();
            }
            else if (typeName == "UniqueNetIdRepl")
            {
                //Some net stuff
                st = new ArkStructUniqueNetId();
            }
            else
            {
                //Interpet this as a struct property list. Maybe raise a warning later?
                //Console.WriteLine($"Unknown struct type '{typeName}'. Interpeting as struct property list.");
                st = new ArkStructProps();
            }

            //Now, read
            await st.Read(ark);

            //Return this
            return st;
        }

        public override async Task Skip(string name, int index, int size, ArkFile ark)
        {
            await ark.io.FastForwardOffset(8 + size);
        }
    }
}
