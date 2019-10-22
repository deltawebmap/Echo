using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace EchoReader.ArkFileReader
{
    public class BufferedIOReader
    {
        public Stream s; //Index in the stream
        public byte[] streamBuffer; //Buffer that holds data read from the stream
        public int index; //Index in the streamBuffer
        public ArkFile ark; //Used for some advanced features

        public BufferedIOReader(Stream s, ArkFile f)
        {
            this.s = s;
            streamBuffer = new byte[0];
            ark = f;
        }

        /// <summary>
        /// Reads into the buffer
        /// </summary>
        /// <param name="len"></param>
        /// <returns></returns>
        public async Task<int> ReadBuffer(int len)
        {
            //Resize buffer
            if (streamBuffer.Length != len)
                streamBuffer = new byte[len];
            index = 0;

            //Read
            return await s.ReadAsync(streamBuffer, 0, len);
        }

        /// <summary>
        /// Copies bytes from the buffer
        /// </summary>
        /// <param name="buf"></param>
        /// <param name="len"></param>
        public void ReadFromBuffer(byte[] buf, int len)
        {
            //Copy
            Array.Copy(streamBuffer, index, buf, 0, len);

            //Advance index
            index += len;
        }

        /// <summary>
        /// Reads from the buffer.
        /// </summary>
        /// <returns></returns>
        public UInt16 ReadUInt16()
        {
            //Convert
            var data = BitConverter.ToUInt16(streamBuffer, index);

            //Advance index
            index += 2;

            //Return data
            return data;
        }

        /// <summary>
        /// Reads from the buffer.
        /// </summary>
        /// <returns></returns>
        public Int16 ReadInt16()
        {
            //Convert
            var data = BitConverter.ToInt16(streamBuffer, index);

            //Advance index
            index += 2;

            //Return data
            return data;
        }

        /// <summary>
        /// Reads from the buffer.
        /// </summary>
        /// <returns></returns>
        public UInt32 ReadUInt32()
        {
            //Convert
            var data = BitConverter.ToUInt32(streamBuffer, index);

            //Advance index
            index += 4;

            //Return data
            return data;
        }

        /// <summary>
        /// Reads from the buffer.
        /// </summary>
        /// <returns></returns>
        public Int32 ReadInt32()
        {
            //Convert
            var data = BitConverter.ToInt32(streamBuffer, index);

            //Advance index
            index += 4;

            //Return data
            return data;
        }

        /// <summary>
        /// Reads from the buffer.
        /// </summary>
        /// <returns></returns>
        public float ReadFloat()
        {
            //Convert
            var data = BitConverter.ToSingle(streamBuffer, index);

            //Advance index
            index += 4;

            //Return data
            return data;
        }

        /// <summary>
        /// Reads from the buffer.
        /// </summary>
        /// <returns></returns>
        public bool ReadIntBool()
        {
            return ReadInt32() == 1;
        }

        /// <summary>
        /// Reads from the buffer.
        /// </summary>
        /// <returns></returns>
        public bool ReadByteBool()
        {
            return ReadByte() == 1;
        }

        /// <summary>
        /// Reads from the buffer.
        /// </summary>
        /// <returns></returns>
        public UInt64 ReadUInt64()
        {
            //Convert
            var data = BitConverter.ToUInt64(streamBuffer, index);

            //Advance index
            index += 8;

            //Return data
            return data;
        }

        /// <summary>
        /// Reads from the buffer.
        /// </summary>
        /// <returns></returns>
        public Int64 ReadInt64()
        {
            //Convert
            var data = BitConverter.ToInt64(streamBuffer, index);

            //Advance index
            index += 8;

            //Return data
            return data;
        }

        /// <summary>
        /// Reads from the buffer.
        /// </summary>
        /// <returns></returns>
        public double ReadDouble()
        {
            //Convert
            var data = BitConverter.ToDouble(streamBuffer, index);

            //Advance index
            index += 8;

            //Return data
            return data;
        }

        /// <summary>
        /// Reads from the buffer.
        /// </summary>
        /// <returns></returns>
        public byte ReadByte()
        {
            //Convert
            var data = streamBuffer[index];

            //Advance index
            index += 1;

            //Return data
            return data;
        }

        /// <summary>
        /// Reads a UEString from the buffer. CAN BE DANGEROUS!
        /// </summary>
        /// <returns></returns>
        public string ReadUEString(int max = 8192)
        {
            //Read and get length
            int length = ReadInt32();

            //Check
            if (Math.Abs(length) > max)
                throw new Exception("Failed to read UE string: " + length + " was longer than the maximum length, " + max);

            //Read
            string data;
            if (length < 0)
            {
                //Two bytes per character
                data = Encoding.Unicode.GetString(streamBuffer, index, (-length * 2) - 1);
                index += (-length * 2) - 1;
            }
            else
            {
                //One byte per character
                data = Encoding.UTF8.GetString(streamBuffer, index, length - 1);
                index += length - 1;
            }

            //Now, read the last byte. It should be a null terminator
            if (ReadByte() != 0x00)
                throw new Exception("Failed to read UE string: Last byte was not null!");

            //Return data
            return data;
        }

        /// <summary>
        /// Reads a UEString directly from the stream, skipping the buffer
        /// </summary>
        /// <returns></returns>
        public async Task<string> DirectReadUEString(int max = 8192)
        {
            //Create buffer
            byte[] buf = new byte[max * 2];

            //Read and get length
            await s.ReadAsync(buf, 0, 4);
            int length = BitConverter.ToInt32(buf);

            //Check
            if (Math.Abs(length) > max)
                throw new Exception("Failed to read UE string: " + length + " was longer than the maximum length, " + max);

            //Read
            string data;
            if(length < 0)
            {
                //Two bytes per character
                await s.ReadAsync(buf, 0, -length * 2);
                data = Encoding.Unicode.GetString(buf, index, (-length * 2) - 1);
            } else
            {
                //One byte per character
                await s.ReadAsync(buf, 0, length);
                data = Encoding.UTF8.GetString(buf, index, length - 1);
            }

            //Now, read the last byte. It should be a null terminator
            if (buf[buf.Length - 1] != 0x00)
                throw new Exception("Failed to read UE string: Last byte was not null!");

            //Return data
            return data;
        }

        /// <summary>
        /// Reads a UE string array directly from the stream, skipping the buffer
        /// </summary>
        /// <param name="ms"></param>
        /// <returns></returns>
        public async Task<string[]> DirectReadUEStringArray()
        {
            //Create buffer
            byte[] buf = new byte[4];

            //Read and get length
            await s.ReadAsync(buf, 0, 4);
            int length = BitConverter.ToInt32(buf);

            //Create an array and read in strings
            string[] array = new string[length];
            for (int i = 0; i < length; i++)
            {
                array[i] = await DirectReadUEString();
            }
            return array;
        }

        /// <summary>
        /// Reads the name table entry index
        /// </summary>
        /// <returns></returns>
        public string ReadNameTableIndex()
        {
            return ReadNameTableIndex(out int i);
        }

        /// <summary>
        /// Reads the name table entry index
        /// </summary>
        /// <returns></returns>
        public string ReadNameTableIndex(out int index)
        {
            //Get index
            int i = ReadInt32()-1;

            //Make sure that it is in bounds
            if (i < 0 || i > ark.name_table.Length)
                throw new Exception("Failed to read Ark name table index: Out of bounds");

            //Read the index
            index = ReadInt32();
            return ark.name_table[i];
        }

        /// <summary>
        /// Goes forward to a position in the stream
        /// </summary>
        /// <param name="pos"></param>
        /// <returns></returns>
        public async Task FastForwardToPosition(long pos)
        {
            //Check if we're already here
            if (pos == s.Position)
                return;
            
            //Check if we're behind this position
            if (pos < s.Position)
                throw new Exception("Cannot fast forward: This function cannot go back in the stream. ");

            s.Seek(pos, SeekOrigin.Begin);
        }

        /// <summary>
        /// Goes forward to a position in the stream
        /// </summary>
        /// <param name="pos"></param>
        /// <returns></returns>
        public async Task FastForwardOffset(int offset)
        {
            await FastForwardToPosition(s.Position + offset);
            
        }
    }
}
