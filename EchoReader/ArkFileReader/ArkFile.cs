using EchoReader.ArkFileReader.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace EchoReader.ArkFileReader
{
    public class ArkFile
    {
        /// <summary>
        /// The name table used for classnames
        /// </summary>
        public string[] name_table;

        /// <summary>
        /// Contains names of the map parts we use, such as "Extinction"
        /// </summary>
        public string[] binary_data_names;

        /// <summary>
        /// Some unknown data in the header
        /// </summary>
        DotArkEmbededBinaryData[] embedded_binary_data;

        /// <summary>
        /// Unknown mystery flags in the header
        /// </summary>
        DotArkIntroMysteryFlags[] mystery_flags;

        /// <summary>
        /// Game object headers
        /// </summary>
        public ArkGameObjectHead[] game_objects;

        /// <summary>
        /// The starting offset of the game object headers
        /// </summary>
        public long game_object_headers_start;

        /// <summary>
        /// Reader
        /// </summary>
        public BufferedIOReader io;

        /* Header data */
        public short save_version;
        public int binary_data_table_offset;
        public int unknown_header_1;
        public int name_table_offset;
        public int properties_block_offset;
        public float game_time;
        public int save_count;

        /// <summary>
        /// Opens an ARK File
        /// </summary>
        /// <param name="s"></param>
        public ArkFile(Stream s)
        {
            io = new BufferedIOReader(s, this);
        }

        /// <summary>
        /// Step one in reading content
        /// </summary>
        /// <returns></returns>
        public async Task ReadHeaders()
        {
            //Fill buffer
            await io.ReadBuffer(2 + 4 + 4 + 4 + 4 + 4 + 4);

            //Read version
            save_version = io.ReadInt16();
            if (save_version != 9)
                throw new Exception("Could not read ARK file: Unsupported save version.");

            //Read other data
            binary_data_table_offset = io.ReadInt32();
            unknown_header_1 = io.ReadInt32();
            name_table_offset = io.ReadInt32();
            properties_block_offset = io.ReadInt32();
            game_time = io.ReadFloat();
            save_count = io.ReadInt32();

            //Read binary data names (such as "Extinction")
            binary_data_names = await io.DirectReadUEStringArray();

            //Now, read embedded binary data. Not sure what this is
            await io.ReadBuffer(4);
            int arraySize = io.ReadInt32();
            embedded_binary_data = new DotArkEmbededBinaryData[arraySize];
            for(int i = 0; i<arraySize; i+=1)
            {
                embedded_binary_data[i] = new DotArkEmbededBinaryData();
                await embedded_binary_data[i].Read(this);
            }

            //Now, read the mystery flags
            await io.ReadBuffer(4);
            arraySize = io.ReadInt32();
            mystery_flags = new DotArkIntroMysteryFlags[arraySize];
            for(int i = 0; i<arraySize; i+=1)
            {
                mystery_flags[i] = new DotArkIntroMysteryFlags();
                await mystery_flags[i].Read(this);
            }

            //Now, we'll read the game object list into memory. We can't decode it yet though because we don't know the name table
            //We're assuming that there is no data between this and the name table, so we read all of it. That might be incorrect
            int bytes = name_table_offset - (int)io.s.Position;
            await io.ReadBuffer(bytes);

            //Now, we'll read the name table
            name_table = await io.DirectReadUEStringArray();

            //Now, we can read the gameobject table. It's all already in the buffer.
            arraySize = io.ReadInt32();
            game_objects = new ArkGameObjectHead[arraySize];
            byte[] buf = new byte[16];
            for (int i = 0; i<arraySize; i+=1)
            {
                ArkGameObjectHead head = new ArkGameObjectHead();
                head.listIndex = i;

                //Read GUID bytes
                io.ReadFromBuffer(buf, 16);
                head.guid = new Guid(buf);

                //Read content
                head.classname = io.ReadNameTableIndex(out head.classnameIndex);
                head.isItem = io.ReadIntBool();

                //Skip the name array
                int nameArraySize = io.ReadInt32();
                for (int j = 0; j < nameArraySize; j++)
                    io.ReadNameTableIndex();

                //Read some unknown data
                head.unknown1 = io.ReadIntBool();
                head.unknown2 = io.ReadInt32();

                //Read the location data if it exists
                bool locationDataExists = io.ReadIntBool();
                if(locationDataExists)
                {
                    head.location = new LibDeltaSystem.Db.Content.DbLocation();
                    head.location.x = io.ReadFloat();
                    head.location.y = io.ReadFloat();
                    head.location.z = io.ReadFloat();
                    head.location.pitch = io.ReadFloat();
                    head.location.yaw = io.ReadFloat();
                    head.location.roll = io.ReadFloat();
                }

                //Read some last data
                head.propDataOffset = io.ReadInt32();
                head.unknown3 = io.ReadInt32();

                //Add
                game_objects[i] = head;
            }
        }
    }
}
