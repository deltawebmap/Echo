using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EchoReader.ArkFileReader.Properties
{
    public class ObjectProperty : BaseProperty
    {
        public ObjectPropertyType objectRefType;

        public int objectId; //Only used if the above is ObjectPropertyType.TypeID
        public string className; //Only used if the above is ObjectPropertyType.TypePath

        private ArkFile ark;

        public override async Task Read(string name, int index, int size, ArkFile ark)
        {
            //Set ark
            this.ark = ark;

            //Read type
            await ark.io.ReadBuffer(4);
            int type = ark.io.ReadInt32();
            if (type > 1 || type < 0)
                throw new Exception($"Unknown ref type! Expected 0 or 1, but got {type} instead!");

            //Convert this to our enum
            objectRefType = (ObjectPropertyType)type;

            //Depending on the type, read it in.
            if (objectRefType == ObjectPropertyType.TypeID)
            {
                await ark.io.ReadBuffer(4);
                objectId = ark.io.ReadInt32();
            }
            if (objectRefType == ObjectPropertyType.TypePath)
            {
                await ark.io.ReadBuffer(8);
                className = ark.io.ReadNameTableIndex();
            }

            /*//If this is a type ID, I **THINK** this is a refrence to a GameObject
            if(objectRefType == ObjectPropertyType.TypeID)
            {
                if(objectId != -1)
                    gameObjectRef = d.gameObjects[objectId];
            }*/
        }

        public override async Task Skip(string name, int index, int size, ArkFile ark)
        {
            await ark.io.FastForwardOffset(size);
        }
    }

    public enum ObjectPropertyType
    {
        TypeID = 0,
        TypePath = 1
    }
}
