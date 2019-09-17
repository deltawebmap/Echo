using ArkSaveEditor.World.WorldTypes;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace EchoEntities.Db
{
    public class DbDino : DbBase
    {
        /// <summary>
        /// The ID of this dinosaur
        /// </summary>
        public ulong dino_id { get; set; }

        /// <summary>
        /// Is this dino tamed?
        /// </summary>
        public bool is_tamed { get; set; }

        /// <summary>
        /// Is this dino female?
        /// </summary>
        public bool is_female { get; set; }

        /// <summary>
        /// Colors in hex format
        /// </summary>
        public string[] colors { get; set; }

        /// <summary>
        /// The tamed name
        /// </summary>
        public string tamed_name { get; set; }

        /// <summary>
        /// The name of the person who originally tamed this dinosaur
        /// </summary>
        public string tamer_name { get; set; }

        /// <summary>
        /// The dinosaur classname
        /// </summary>
        public string classname { get; set; }

        /// <summary>
        /// The current stats
        /// </summary>
        public ArkDinosaurStats current_stats { get; set; }

        /// <summary>
        /// The number of levelups applied
        /// </summary>
        public ArkDinosaurStats base_levelups_applied { get; set; }

        /// <summary>
        /// The levelups applied while tamed
        /// </summary>
        public ArkDinosaurStats tamed_levelups_applied { get; set; }

        /// <summary>
        /// The base level at spawn time
        /// </summary>
        public int base_level { get; set; }

        /// <summary>
        /// The level of this dinosaur
        /// </summary>
        public int level { get; set; }

        /// <summary>
        /// Exp
        /// </summary>
        public float experience { get; set; }

        /// <summary>
        /// Is this a baby dinosaur?
        /// </summary>
        public bool is_baby { get; set; }

        /// <summary>
        /// Not null if is_baby. 1 = fully grown, 0 = newborn
        /// </summary>
        public float baby_age { get; set; }

        /// <summary>
        /// The next imprint time if this is a baby dino
        /// </summary>
        public double next_imprint_time { get; set; }

        /// <summary>
        /// The baby dino imprint quality if this is a baby dino
        /// </summary>
        public float imprint_quality { get; set; }

        /// <summary>
        /// The location of this dinosaur
        /// </summary>
        public DbLocation location { get; set; }
    }
}
