using ArkSaveEditor.Entities;
using ArkSaveEditor.Entities.LowLevel;
using ArkSaveEditor.Entities.LowLevel.DotArk;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ArkSaveEditor.Deserializer
{
    /// <summary>
    /// This is the main class for deserializing the Ark save enviornment
    /// </summary>
    public static class ArkSaveDeserializer
    {
        public static DotArkFile OpenDotArk(MemoryStream ms)
        {
            //First, read in the Ark file
            DotArk.DotArkDeserializer dotArkDs;
            //Rewind
            ms.Position = 0;
            //Create
            dotArkDs = new DotArk.DotArkDeserializer();
            dotArkDs.OpenArkFile(new IOMemoryStream(ms, true), (DotArkGameObject obj, object d) => { return null;  }, null);
            return dotArkDs.ark;
        }
    }
}
