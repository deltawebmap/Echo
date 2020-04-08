using LibDeltaSystem.Db.System;
using LibDeltaSystem.Db.Content;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Driver;
using EchoContent.Exceptions;
using LibDeltaSystem;
using LibDeltaSystem.Entities.ArkEntries;
using EchoContent.Tools;
using Microsoft.AspNetCore.Http;
using static EchoContent.Http.World.TribeOverviewRequest;

namespace EchoContent.Http.World
{
    public class TribeOverviewRequest : EchoStreamedDataDeltaService<DbDino, TribeOverviewDino>
    {
        public TribeOverviewRequest(DeltaConnection conn, HttpContext e) : base(conn, e)
        {
        }

        public override IMongoCollection<DbDino> GetMongoCollection()
        {
            return conn.content_dinos;
        }

        public override FilterDefinition<DbDino> GetFilter()
        {
            var filterBuilder = Builders<DbDino>.Filter;
            var filter = filterBuilder.Eq("is_tamed", true) & GetServerTribeFilter<DbDino>();
            return filter;
        }

        public override async Task<List<TribeOverviewDino>> ConvertDocuments(List<DbDino> resultsArray)
        {
            List<TribeOverviewDino> resultsConverted = new List<TribeOverviewDino>();
            for (int i = 0; i < resultsArray.Count; i++)
            {
                var rd = await ConvertDocument(resultsArray[i]);
                if (rd != null)
                    resultsConverted.Add(rd);
            }
            return resultsConverted;
        }

        public async Task<TribeOverviewDino> ConvertDocument(DbDino p)
        {
            //Lookup dino entry for this
            var entry = await package.GetDinoEntryByClssnameAsnyc(p.classname);
            if (entry == null)
                return null;

            //Convert
            return new TribeOverviewDino
            {
                classDisplayName = entry.screen_name,
                displayName = p.tamed_name,
                id = p.dino_id.ToString(),
                img = entry.icon.image_thumb_url,
                level = p.level,
                status = p.status,
                color_tag = null,
                is_cryo = p.is_cryo,
                is_baby = p.is_baby,
                is_female = p.is_female
            };
        }

        public class TribeOverviewDino
        {
            public string displayName;
            public string classDisplayName;
            public int level;
            public string id;
            public string img;
            public string status;
            public string color_tag;
            public bool is_cryo;
            public bool is_baby;
            public bool is_female;
        }
    }
}
