using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EchoReader.ArkFileReader.Entities
{
    class DotArkEmbededBinaryData
    {
        public string path;
        public byte[][][] data;

        public async Task Read(ArkFile f)
        {
            //First, read the path.
            path = await f.io.DirectReadUEString();

            //Now, read the parts. This seems to be split up into part -> blob -> inner blob
            await f.io.ReadBuffer(4);
            int parts = f.io.ReadInt32();
            data = new byte[parts][][];

            //Loop through each of the parts 
            for (int i = 0; i < parts; i++)
            {
                await f.io.ReadBuffer(4);
                int blobs = f.io.ReadInt32();
                byte[][] partData = new byte[blobs][];

                for (int j = 0; j < blobs; j++)
                {
                    await f.io.ReadBuffer(4);
                    int blobSize = f.io.ReadInt32() * 4; //Array of 32 bit integers.

                    await f.io.ReadBuffer(blobSize);
                    partData[j] = new byte[blobSize];
                    f.io.ReadFromBuffer(partData[j], blobSize);
                }

                data[i] = partData;
            }
        }
    }
}
