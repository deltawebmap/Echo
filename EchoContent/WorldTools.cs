﻿using LibDeltaSystem.Db.Content;
using LibDeltaSystem.Entities;
using LibDeltaSystem.Entities.ArkEntries;
using System;
using System.Collections.Generic;
using System.Text;

namespace EchoContent
{
    public static class WorldTools
    {
        public static Vector2 ConvertFromWorldToGameCoords(DbLocation src, ArkMapEntry mapInfo)
        {
            return new Vector2(ConvertSingleFromWolrdToGameCoords(src.x, mapInfo), ConvertSingleFromWolrdToGameCoords(src.y, mapInfo));
        }

        public static Vector2 ConvertFromWorldToNormalizedPos(DbLocation src, ArkMapEntry mapInfo)
        {
            return new Vector2(ConvertFromWorldToNormalizedPos(src.x, mapInfo), ConvertFromWorldToNormalizedPos(src.y, mapInfo));
        }

        public static float ConvertFromWorldToNormalizedPos(float src, ArkMapEntry mapInfo)
        {
            //Add half of the width to make it start at zero.
            float r = src + (mapInfo.bounds.height / 2);

            //Now divide by the length of the map
            return r / mapInfo.bounds.height;
        }

        public static float ConvertSingleFromWolrdToGameCoords(float src, ArkMapEntry mapInfo)
        {
            return (src / mapInfo.latLonMultiplier) + 50;
        }
    }
}
