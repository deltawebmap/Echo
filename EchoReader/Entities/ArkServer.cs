using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace EchoReader.Entities
{
    /// <summary>
    /// Represents an Ark server
    /// </summary>
    public class ArkServer
    {
        /// <summary>
        /// The ARK server ID
        /// </summary>
        public string id;

        /// <summary>
        /// Content uploaded
        /// </summary>
        public List<ArkUploadedFile> files;

        /// <summary>
        /// Saves and changes to disk
        /// </summary>
        public void Save()
        {
            //Write serilaized
            ArkServerSerialized ser = new ArkServerSerialized
            {
                files = files,
                _id = id
            };

            //Save
            Program.servers_collection.Upsert(ser);
        }

        /// <summary>
        /// Loads all settings from disk
        /// </summary>
        public void Load(ArkServerSerialized ser)
        {
            id = ser._id;
            files = ser.files;
        }

        /// <summary>
        /// Returns a file's metadata. If it doesn't exist, this returns null
        /// </summary>
        /// <param name="type"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public ArkUploadedFile GetFileMeta(ArkUploadedFileType type, string name)
        {
            //Try to find
            var results = files.Where(x => x.type == type && x.name == name);
            if (results.Count() == 0)
                return null;
            return results.First();
        }

        /// <summary>
        /// Gets the decompressed file stream
        /// </summary>
        /// <param name="f"></param>
        /// <returns></returns>
        public static async Task<Stream> GetFileStream(ArkUploadedFile f)
        {
            //Open GZIP stream
            MemoryStream ms = new MemoryStream();
            using(FileStream fs = new FileStream(Program.config.content_uploads_path + f.token, FileMode.Open))
            {
                using (GZipStream gz = new GZipStream(fs, CompressionMode.Decompress))
                    await gz.CopyToAsync(ms);
            }
            return ms;
        }

        /// <summary>
        /// Uploads a file and replaces any existing file
        /// </summary>
        /// <param name="type">The type of the file.</param>
        /// <param name="name">The name of the file.</param>
        /// <param name="compressed">The data for this file.</param>
        /// <returns></returns>
        public async Task<ArkUploadedFile> PutFile(ArkUploadedFileType type, string name, Stream compressed)
        {
            //Generate a new token
            string token = Program.GenerateRandomString(32);
            while(File.Exists(Program.config.content_uploads_path+token))
                token = Program.GenerateRandomString(32);

            //Write this to disk
            long size;
            using (FileStream fs = new FileStream(Program.config.content_uploads_path + token, FileMode.Create))
            {
                await compressed.CopyToAsync(fs);
                size = fs.Length;
            }

            //If there is already a file with this name, get it and remove it
            ArkUploadedFile file = GetFileMeta(type, name);
            if(file != null)
            {
                File.Delete(Program.config.content_uploads_path + file.token);
                files.Remove(file);
            }

            //Create a new file entry
            file = new ArkUploadedFile
            {
                name = name,
                type = type,
                time_utc = DateTime.UtcNow.Ticks,
                token = token,
                compressed_size = size
            };

            //Obtain SHA1
            using (Stream data = await GetFileStream(file))
            using (SHA1Managed sha1 = new SHA1Managed())
            {
                byte[] hashBytes = sha1.ComputeHash(data);
                file.sha1 = string.Concat(hashBytes.Select(b => b.ToString("x2")));
                file.size = data.Length;
            }

            //Save
            files.Add(file);
            Save();

            return file;
        }
    }
}
