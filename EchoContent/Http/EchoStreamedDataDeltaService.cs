using LibDeltaSystem;
using Microsoft.AspNetCore.Http;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EchoContent.Http
{
    /// <summary>
    /// Service that can represent streamed content
    /// </summary>
    public abstract class EchoStreamedDataDeltaService<T, R> : EchoTribeDeltaService
    {
        public EchoStreamedDataDeltaService(DeltaConnection conn, HttpContext e) : base(conn, e)
        {
        }

        public override async Task OnRequest()
        {
            //Get limit and skip
            int? limit = GetOptionalUrlParam("limit");
            int? skip = GetOptionalUrlParam("skip");
            
            //Get Mongo collection and filter
            var collec = GetMongoCollection();
            var filter = GetFilter();

            //Find a count of all returned results
            EndDebugCheckpoint("Get document count");
            long resultCount = await collec.CountDocumentsAsync(filter);

            //Now, query matching results
            EndDebugCheckpoint("Get documents");
            var results = await collec.FindAsync(filter, new FindOptions<T, T>
            {
                Limit = limit,
                Skip = skip
            });
            var resultsArray = await results.ToListAsync();

            //Convert all of these into a valid document
            EndDebugCheckpoint("Convert documents");
            List<R> resultsConverted = await ConvertDocuments(resultsArray);

            //Create response data
            ResponseData response = new ResponseData
            {
                limit = limit,
                skip = skip,
                total_results = resultCount,
                results = resultsConverted,
                has_previous = skip.HasValue ? skip > 0 : false,
                has_next = limit.HasValue ? ((skip.HasValue ? skip.Value : 0) + limit) < resultCount : false
            };

            //Write data
            await WriteJSON(response);
        }

        class ResponseData
        {
            public int? limit;
            public int? skip;
            public long total_results;
            public bool has_previous;
            public bool has_next;
            public List<R> results;
        }

        /// <summary>
        /// Gets a URL prameter
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        private int? GetOptionalUrlParam(string name)
        {
            if (!e.Request.Query.ContainsKey(name))
                return null;
            if (int.TryParse(e.Request.Query[name], out int r))
                return r;
            else
                return null;
        }

        /// <summary>
        /// Gets the MongoDB collection we will be searching in
        /// </summary>
        /// <returns></returns>
        public abstract IMongoCollection<T> GetMongoCollection();

        /// <summary>
        /// Get the requested filter for the MongoDB database
        /// </summary>
        /// <returns></returns>
        public abstract FilterDefinition<T> GetFilter();

        /// <summary>
        /// Converts a document to it's output type
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public abstract Task<List<R>> ConvertDocuments(List<T> data);
    }
}
