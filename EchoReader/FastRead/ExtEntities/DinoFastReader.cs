using EchoReader.ArkFileReader;
using EchoReader.ArkFileReader.Entities;
using EchoReader.ArkFileReader.Properties;
using LibDeltaSystem.Db.Content;
using System;
using System.Collections.Generic;
using System.Text;

namespace EchoReader.FastRead.ExtEntities
{
    public class DinoFastReader : PropertyReaderInterface
    {
        public DbDino data;

        public DinoFastReader(ArkGameObjectHead h, FastReadSession s)
        {
            data = new DbDino
            {
                baby_age = 1,
                base_level = 1,
                base_levelups_applied = null,
                classname = h.classname,
                colors = new string[12],
                current_stats = null,
                dino_id = 0,
                experience = 0,
                hash = null,
                imprint_quality = 1,
                is_baby = false,
                is_female = false,
                is_tamed = false,
                level = 1,
                location = h.location,
                next_imprint_time = 0,
                server_id = s.server_id,
                tamed_levelups_applied = null,
                tamed_name = "",
                tamer_name = "",
                token = "",
                tribe_id = -1
            };
        }

        public StreamPropertyCallback GetPropertyCallback(string name, string type)
        {
            switch(name)
            {
                case "ColorSetIndices": return Prop_ColorSetIndices;
            }
            return null;
        }

        void Prop_ColorSetIndices(BaseProperty prop, string name, string type, int index)
        {
            //Something...
        }
    }
}
