using ArkSaveEditor.Entities.LowLevel.DotArk.ArkProperties;
using ArkSaveEditor.World.WorldTypes;
using EchoReader.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace EchoReader.Helpers
{
    public static class ArkDinosaurStatHelper
    {
        public static ArkDinosaurStats ReadStats(ArkPropertyReader reader, string propertyName, bool isByteProp)
        {
            ArkDinosaurStats s = new ArkDinosaurStats();
            var props = reader.GetPropertiesByName(propertyName);
            foreach (var p in props)
            {
                int index = p.index;
                float data;
                if (isByteProp)
                    data = (float)(((ByteProperty)p).byteValue);
                else
                    data = (float)p.data;

                switch (index)
                {
                    case 0:
                        s.health = data;
                        break;
                    case 1:
                        s.stamina = data;
                        break;
                    case 2:
                        s.unknown1 = data;
                        break;
                    case 3:
                        s.oxygen = data;
                        break;
                    case 4:
                        s.food = data;
                        break;
                    case 5:
                        s.water = data;
                        break;
                    case 6:
                        s.unknown2 = data;
                        break;
                    case 7:
                        s.inventoryWeight = data;
                        break;
                    case 8:
                        s.meleeDamageMult = data;
                        break;
                    case 9:
                        s.movementSpeedMult = data;
                        break;
                    case 10:
                        s.unknown2 = data;
                        break;
                    case 11:
                        s.unknown4 = data;
                        break;
                    default:
                        //We shouldn't be here...
                        throw new Exception($"Unknown index ID while reading Dinosaur stats {index}!");
                }
            }

            return s;
        }
    }
}
