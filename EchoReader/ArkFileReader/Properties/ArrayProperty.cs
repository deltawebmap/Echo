using EchoReader.ArkFileReader.Structs;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EchoReader.ArkFileReader.Properties
{
    public class ArrayProperty : BaseProperty
    {
        public object data;

        public override async Task Read(string name, int index, int size, ArkFile ark)
        {
            //Open content
            await ark.io.ReadBuffer(8);

            //Get array type
            string type = ark.io.ReadNameTableIndex();

            //Process
            switch (type)
            {
                case "ObjectProperty": data = await ReadObjectProperty(ark, index, size, type); break;
                case "StructProperty": data = await ReadStructProperty(ark, index, size, type); break;
                case "UInt32Property": data = await ReadUInt32Property(ark, index, size, type); break;
                case "IntProperty": data = await ReadIntProperty(ark, index, size, type); break;
                case "UInt16Property": data = await ReadUInt16Property(ark, index, size, type); break;
                case "Int16Property": data = await ReadInt16Property(ark, index, size, type); break;
                case "ByteProperty": data = await ReadByteProperty(ark, index, size, type); break;
                case "Int8Property": data = await ReadInt8Property(ark, index, size, type); break;
                case "StrProperty": data = await ReadStrProperty(ark, index, size, type); break;
                case "UInt64Property": data = await ReadUInt64Property(ark, index, size, type); break;
                case "BoolProperty": data = await ReadBoolProperty(ark, index, size, type); break;
                case "FloatProperty": data = await ReadFloatProperty(ark, index, size, type); break;
                case "DoubleProperty": data = await ReadDoubleProperty(ark, index, size, type); break;
                case "NameProperty": data = await ReadNameProperty(ark, index, size, type); break;
                default:
                    throw new Exception($"Unknown ARK array type '{type}'.");
            }
        }

        public override async Task Skip(string name, int index, int size, ArkFile ark)
        {
            //Skip content
            await ark.io.FastForwardOffset(size + 8);
        }

        /* What the fuck? */
        private static async Task<List<ObjectProperty>> ReadObjectProperty(ArkFile d, int index, int length, string type)
        {
            //Open
            await d.io.ReadBuffer(4);
            int arraySize = d.io.ReadInt32();

            List<ObjectProperty> data = new List<ObjectProperty>();

            for (int i = 0; i < arraySize; i++)
            {
                var o = new ObjectProperty();
                await o.Read("ARRAY_PROPERTY", index, length, d);
                data.Add(o);
            }

            //Create
            return data;
        }

        private static async Task<List<BaseArkStruct>> ReadStructProperty(ArkFile d, int index, int length, string type)
        {
            //Open
            List<BaseArkStruct> data = new List<BaseArkStruct>();
            await d.io.ReadBuffer(4);
            int arraySize = d.io.ReadInt32();

            //Determine the type
            string structType;
            if (arraySize * 4 + 4 == length)
                structType = "Color";
            else if (arraySize * 12 + 4 == length)
                structType = "Vector";
            else if (arraySize * 16 + 4 == length)
                structType = "LinearColor";
            else
                structType = null;

            //Read
            if (structType != null)
            {
                for (int i = 0; i < arraySize; i++)
                {
                    data.Add(await StructProperty.ReadStructFromStream(d, structType));
                }
            }
            else
            {
                for (int i = 0; i < arraySize; i++)
                {
                    data.Add(await StructProperty.ReadStructFromStream(d, structType));
                }
            }

            //Create
            return data;
        }

        private static async Task<List<UInt32>> ReadUInt32Property(ArkFile d, int index, int length, string type)
        {
            //Open
            await d.io.ReadBuffer(4);
            int arraySize = d.io.ReadInt32();
            await d.io.ReadBuffer(4 * arraySize);

            List<UInt32> data = new List<UInt32>();

            for (int i = 0; i < arraySize; i++)
                data.Add(d.io.ReadUInt32());

            //Create
            return data;
        }

        private static async Task<List<int>> ReadIntProperty(ArkFile d, int index, int length, string type)
        {
            //Open
            await d.io.ReadBuffer(4);
            int arraySize = d.io.ReadInt32();
            await d.io.ReadBuffer(4 * arraySize);

            List<int> data = new List<int>();

            for (int i = 0; i < arraySize; i++)
                data.Add(d.io.ReadInt32());

            //Create
            return data;
        }

        private static async Task<List<UInt16>> ReadUInt16Property(ArkFile d, int index, int length, string type)
        {
            //Open
            await d.io.ReadBuffer(4);
            int arraySize = d.io.ReadInt32();
            await d.io.ReadBuffer(2 * arraySize);

            List<UInt16> data = new List<UInt16>();

            for (int i = 0; i < arraySize; i++)
                data.Add(d.io.ReadUInt16());

            //Create
            return data;
        }

        private static async Task<List<Int16>> ReadInt16Property(ArkFile d, int index, int length, string type)
        {
            //Open
            await d.io.ReadBuffer(4);
            int arraySize = d.io.ReadInt32();
            await d.io.ReadBuffer(2 * arraySize);

            List<Int16> data = new List<Int16>();

            for (int i = 0; i < arraySize; i++)
                data.Add(d.io.ReadInt16());

            //Create
            return data;
        }

        private static async Task<List<byte>> ReadByteProperty(ArkFile d, int index, int length, string type)
        {
            //Open
            await d.io.ReadBuffer(4);
            int arraySize = d.io.ReadInt32();
            await d.io.ReadBuffer(arraySize);

            List<byte> data = new List<byte>();

            for (int i = 0; i < arraySize; i++)
                data.Add(d.io.ReadByte());

            //Create
            return data;
        }

        private static async Task<List<byte>> ReadInt8Property(ArkFile d, int index, int length, string type)
        {
            //Open
            await d.io.ReadBuffer(4);
            int arraySize = d.io.ReadInt32();
            await d.io.ReadBuffer(arraySize);

            List<byte> data = new List<byte>();

            for (int i = 0; i < arraySize; i++)
                data.Add(d.io.ReadByte());

            //Create
            return data;
        }

        private static async Task<List<string>> ReadStrProperty(ArkFile d, int index, int length, string type)
        {
            //Open
            await d.io.ReadBuffer(4);
            int arraySize = d.io.ReadInt32();

            List<string> data = new List<string>();

            for (int i = 0; i < arraySize; i++)
                data.Add(await d.io.DirectReadUEString());

            //Create
            return data;
        }

        private static async Task<List<UInt64>> ReadUInt64Property(ArkFile d, int index, int length, string type)
        {
            //Open
            await d.io.ReadBuffer(4);
            int arraySize = d.io.ReadInt32();
            await d.io.ReadBuffer(arraySize * 8);

            List<UInt64> data = new List<UInt64>();

            for (int i = 0; i < arraySize; i++)
                data.Add(d.io.ReadUInt64());

            //Create
            return data;
        }

        private static async Task<List<bool>> ReadBoolProperty(ArkFile d, int index, int length, string type)
        {
            //Open
            await d.io.ReadBuffer(4);
            int arraySize = d.io.ReadInt32();
            await d.io.ReadBuffer(arraySize);

            List<bool> data = new List<bool>();

            for (int i = 0; i < arraySize; i++)
                data.Add(d.io.ReadByteBool());

            //Create
            return data;
        }

        private static async Task<List<float>> ReadFloatProperty(ArkFile d, int index, int length, string type)
        {
            //Open
            await d.io.ReadBuffer(4);
            int arraySize = d.io.ReadInt32();
            await d.io.ReadBuffer(4 * arraySize);

            List<float> data = new List<float>();

            for (int i = 0; i < arraySize; i++)
                data.Add(d.io.ReadFloat());

            //Create
            return data;
        }

        private static async Task<List<double>> ReadDoubleProperty(ArkFile d, int index, int length, string type)
        {
            //Open
            await d.io.ReadBuffer(4);
            int arraySize = d.io.ReadInt32();
            await d.io.ReadBuffer(8 * arraySize);

            List<double> data = new List<double>();

            for (int i = 0; i < arraySize; i++)
                data.Add(d.io.ReadDouble());

            //Create
            return data;
        }

        private static async Task<List<string>> ReadNameProperty(ArkFile d, int index, int length, string type)
        {
            //Open
            await d.io.ReadBuffer(4);
            int arraySize = d.io.ReadInt32();
            await d.io.ReadBuffer(8 * arraySize);

            List<string> data = new List<string>();

            for (int i = 0; i < arraySize; i++)
                data.Add(d.io.ReadNameTableIndex());

            //Create
            return data;
        }
    }
}
