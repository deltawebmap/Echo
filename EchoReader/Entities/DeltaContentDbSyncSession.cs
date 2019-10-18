using ArkSaveEditor.Entities.LowLevel.DotArk;
using LibDeltaSystem.Db.Content;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace EchoReader.Entities
{
    /// <summary>
    /// Helps with syncing sessions. For use with one content collection
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class DeltaContentDbSyncSession<T>
    {
        /// <summary>
        /// The actual collection we're using
        /// </summary>
        public IMongoCollection<T> collection;

        /// <summary>
        /// The server this is acting on.
        /// </summary>
        public string server_id;

        /// <summary>
        /// Tokens that are in use
        /// </summary>
        public List<string> used_tokens;

        private List<T> queue = new List<T>();

        private List<T> old = new List<T>();

        /// <summary>
        /// Creates a new session
        /// </summary>
        /// <param name="collec"></param>
        /// <param name="server_id"></param>
        public DeltaContentDbSyncSession (IMongoCollection<T> collec, string server_id)
        {
            this.collection = collec;
            this.server_id = server_id;
            this.used_tokens = new List<string>();

            //Get
            var filterBuilder = Builders<T>.Filter;
            var filter = filterBuilder.Eq("server_id", server_id);
            old = collec.Find(filter).ToList();
        }

        /// <summary>
        /// Returns true if an update is required.
        /// </summary>
        /// <param name="token"></param>
        /// <param name="hash"></param>
        /// <returns></returns>
        public bool CheckIfUpdateRequired(string token, string hash)
        {
            //We'll need to get the hash in a pretty janky fashion
            var hashPropInfo = typeof(T).GetProperty("hash");
            var tokenPropInfo = typeof(T).GetProperty("token");

            //Add to tokens
            if(!used_tokens.Contains(token))
                used_tokens.Add(token);

            //Find this item if it's ever existed
            var oldItem = old.Where(x => (string)hashPropInfo.GetValue(x) == hash && (string)tokenPropInfo.GetValue(x) == token).FirstOrDefault();

            return oldItem == null;
        }

        /// <summary>
        /// Updates one item. Assumes that an update is needed.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task UpdateOne(T data, string token, string hash)
        {
            //We'll need to get the hash in a pretty janky fashion
            var hashPropInfo = typeof(T).GetProperty("hash");

            //Add to tokens
            if (!used_tokens.Contains(token))
                used_tokens.Add(token);

            //Set hash
            hashPropInfo.SetValue(data, hash);

            //Check if we need to insert
            var tokenPropInfo = typeof(T).GetProperty("token");

            //Add to tokens
            if (!used_tokens.Contains(token))
                used_tokens.Add(token);

            //Find this item if it's ever existed
            bool isInsert = old.Where(x => (string)tokenPropInfo.GetValue(x) == token).Count() == 0;

            if (isInsert)
            {
                //Queue it for faster use
                queue.Add(data);
            } else
            {
                //Just update it now
                var filterBuilder = Builders<T>.Filter;
                var filter = filterBuilder.Eq("server_id", server_id) & filterBuilder.Eq("token", token);
                await collection.FindOneAndReplaceAsync(filter, data, new FindOneAndReplaceOptions<T, T>
                {
                    IsUpsert = true
                });
            }
            //Console.WriteLine("updating " + hash);
        }

        /// <summary>
        /// Finalizes the sync by removing old items
        /// </summary>
        /// <returns></returns>
        public async Task FinishSync()
        {
            //Generate filter to use when inserting
            var filterBuilder = Builders<T>.Filter;
            var filter = filterBuilder.Eq("server_id", server_id) & (!filterBuilder.In("token", used_tokens));

            //Add
            if(queue.Count > 0)
            {
                await collection.InsertManyAsync(queue);
                //Console.WriteLine("wrote " + queue.Count);
                queue.Clear();
            }

            //Remove all of these
            await collection.DeleteManyAsync(filter);

            //Clear
            used_tokens.Clear();
            old.Clear();
        }
    }
}
