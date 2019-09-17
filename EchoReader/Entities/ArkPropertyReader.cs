using ArkSaveEditor.Entities.LowLevel.DotArk;
using ArkSaveEditor.Entities.LowLevel.DotArk.ArkProperties;
using ArkSaveEditor.World;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EchoReader.Entities
{
    public class ArkPropertyReader
    {
        public List<DotArkProperty> props;

        public ArkPropertyReader(List<DotArkProperty> p)
        {
            this.props = p;
        }

        public DotArkProperty[] GetPropertiesByName(string name)
        {
            return props.Where(x => x.name.classname == name).ToArray();
        }

        public bool CheckIfValueExists(string name)
        {
            return GetPropertiesByName(name).Length >= 1;
        }

        public bool GetBooleanProperty(string name)
        {
            //Get the value. If it doesn't exist, return false because that is how items are saved.
            var p = GetPropertiesByName(name);
            if (p.Length == 0)
                return false;
            //Return if this is true or not.
            return (bool)((BoolProperty)p[0]).data;
        }

        public DotArkProperty GetSingleProperty(string name)
        {
            var arr = props.Where(x => x.name.classname == name).ToArray();
            //If there are less or more than 1, throw an exception
            if (arr.Length != 1)
                throw new Exception($"The number of properties with name {name} did not equal 1. Instead, there were {arr.Length} values.");
            return arr[0];
        }


        public uint GetUInt32Property(string name)
        {
            return (uint)GetSingleProperty(name).data;
        }

        public ushort GetUInt16Property(string name)
        {
            return (ushort)GetSingleProperty(name).data;
        }

        public int GetInt32Property(string name)
        {
            return (int)GetSingleProperty(name).data;
        }

        public DotArkGameObject GetGameObjectRef(string name)
        {
            var prop = (ObjectProperty)GetSingleProperty(name);
            if (prop.objectRefType != ObjectPropertyType.TypeID)
                throw new Exception("The ref provided by this property is not a GameObject!");
            return prop.gameObjectRef;
        }

        public float GetFloatProperty(string name)
        {
            return (float)GetSingleProperty(name).data;
        }

        public double GetDoubleProperty(string name)
        {
            return (double)GetSingleProperty(name).data;
        }

        public string GetStringProperty(string name)
        {
            return (string)GetSingleProperty(name).data;
        }

        public bool HasProperty(string name)
        {
            var arr = props.Where(x => x.name.classname == name).ToArray();
            return arr.Length != 0;
        }

        public UInt64 GetUInt64Property(string name)
        {
            return (UInt64)GetSingleProperty(name).data;
        }
    }
}
