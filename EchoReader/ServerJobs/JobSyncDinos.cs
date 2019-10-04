using ArkSaveEditor.Entities.LowLevel.DotArk;
using ArkSaveEditor.Entities.LowLevel.DotArk.ArkProperties;
using ArkSaveEditor.World;
using EchoReader.Entities;
using EchoReader.Helpers;
using LibDeltaSystem.Db.Content;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EchoReader.ServerJobs
{
    public class JobSyncDinos
    {
        public ArkSaveEditor.Deserializer.DotArk.DotArkDeserializer deser;
        public string server_id;
        public DeltaContentDbSyncSession<DbDino> sync;

        public JobSyncDinos(ArkServer s, ArkSaveEditor.Deserializer.DotArk.DotArkDeserializer deser)
        {
            server_id = s.id;
            sync = new DeltaContentDbSyncSession<DbDino>(Program.conn.content_dinos, server_id);
            this.deser = deser;
        }

        public async Task End()
        {
            await sync.FinishSync();
        }

        public void RunOne(DotArkGameObject obj, List<DotArkProperty> props, object na)
        {
            RunOneAsync(obj, props).GetAwaiter().GetResult();
        }

        public async Task RunOneAsync(DotArkGameObject obj, List<DotArkProperty> props)
        {
            //Make sure we're supported
            ArkPropertyReader reader = new ArkPropertyReader(props);
            if (!GetDinoSupported(reader))
                return;

            //Get hash code
            string hash = obj.GetHash(props);

            //Get dino token
            string token = GetDinoToken(reader).ToString();

            //First, check if an update is even needed.
            if (!sync.CheckIfUpdateRequired(token, hash))
                return;

            //Convert dino
            DbDino dino = ConvertDino(obj, reader);

            //Update
            await sync.UpdateOne(dino, token, hash);

            /*//Now, insert items
            if(reader.HasProperty("MyInventoryComponent"))
            {
                //Get the referenced inventory and open a reader on it
                var inventoryComponent = reader.GetGameObjectRef("MyInventoryComponent");
                var inventoryComponentReader = new ArkPropertyReader(inventoryComponent.ReadPropsFromFile(deser));

                //Upload all
                ArkInventoryReader.SaveInventory(inventoryComponent, inventoryComponentReader, deser, input.itemSyncSession, db.token, DbInventoryParentType.Dino, db.tribe_id, id, revision_id);
            }*/
        }

        private UInt64 GetDinoToken(ArkPropertyReader reader)
        {
            //Read the dinosaur ID by combining the the bytes of the two UInt32 values.
            byte[] buf = new byte[8];
            BitConverter.GetBytes(reader.GetUInt32Property("DinoID1")).CopyTo(buf, 0);
            BitConverter.GetBytes(reader.GetUInt32Property("DinoID2")).CopyTo(buf, 4);
            //Convert this to a ulong
            return BitConverter.ToUInt64(buf, 0);
        }

        private bool GetDinoSupported(ArkPropertyReader reader)
        {
            return reader.CheckIfValueExists("TamedName") && reader.CheckIfValueExists("TribeName") && reader.CheckIfValueExists("TargetingTeam");
        }

        private DbDino ConvertDino(DotArkGameObject obj, ArkPropertyReader reader)
        {
            bool tamed = reader.CheckIfValueExists("TamedName") && reader.CheckIfValueExists("TribeName") && reader.CheckIfValueExists("TargetingTeam");
            if (!tamed)
                return null;

            //Get the referenced status and open a reader on it
            var statusComponent = reader.GetGameObjectRef("MyCharacterStatusComponent");
            var statusComponentReader = new ArkPropertyReader(statusComponent.ReadPropsFromFile(deser));

            //Read properties
            DbDino db = new DbDino
            {
                is_tamed = tamed,
                is_female = reader.GetBooleanProperty("bIsFemale"),
                server_id = server_id,
                tribe_id = -1,
                classname = obj.classname.classname,
                location = new DbLocation
                {
                    x = obj.locationData.x,
                    y = obj.locationData.y,
                    z = obj.locationData.z,
                    pitch = obj.locationData.pitch,
                    yaw = obj.locationData.yaw,
                    roll = obj.locationData.roll,
                }
            };

            //Convert the colors into a byte array and hex.
            var colorAttrib = reader.GetPropertiesByName("ColorSetIndices"); //Get all of the color properties from the dinosaur. These are indexes in the color table.
            byte[] colors = new byte[colorAttrib.Length]; //Initialize the array for storing the indexes. These will be saved to the file.
            db.colors = new string[colorAttrib.Length]; //Initialize the array for reading nice HTML color values.
            for (int i = 0; i < colors.Length; i++) //For each color region this dinosaur has. Each "ColorSetIndices" value is a color region.
            {
                colors[i] = ((ByteProperty)colorAttrib[i]).byteValue; //Get the index in the color table by getting the byte value out of the property
                //Validate that the color is in range
                byte color = colors[i];
                if (color <= 0 || color > ArkColorIds.ARK_COLOR_IDS.Length)
                    db.colors[i] = "#FFF";
                else
                    db.colors[i] = ArkColorIds.ARK_COLOR_IDS[colors[i] - 1]; //Look this up in the color table to get the nice HTML value.
            }

            //Read the dinosaur ID by combining the the bytes of the two UInt32 values.
            byte[] buf = new byte[8];
            BitConverter.GetBytes(reader.GetUInt32Property("DinoID1")).CopyTo(buf, 0);
            BitConverter.GetBytes(reader.GetUInt32Property("DinoID2")).CopyTo(buf, 4);
            //Convert this to a ulong
            db.dino_id = BitConverter.ToUInt64(buf, 0);
            db.token = db.dino_id.ToString();

            //Read stats
            db.current_stats = ArkDinosaurStatHelper.ReadStats(statusComponentReader, "CurrentStatusValues", false);
            db.base_levelups_applied = ArkDinosaurStatHelper.ReadStats(statusComponentReader, "NumberOfLevelUpPointsApplied", true);
            db.base_level = 1;
            if (statusComponentReader.CheckIfValueExists("BaseCharacterLevel"))
                db.base_level = statusComponentReader.GetInt32Property("BaseCharacterLevel");
            db.level = db.base_level;
            db.tamed_levelups_applied = new DbArkDinosaurStats();

            //Now, convert attributes that only exist on tamed dinosaurs.
            if (db.is_tamed)
            {
                db.tamed_name = reader.GetStringProperty("TamedName");
                db.tribe_id = reader.GetInt32Property("TargetingTeam");
                db.tamer_name = reader.GetStringProperty("TribeName");
                db.tamed_levelups_applied = ArkDinosaurStatHelper.ReadStats(statusComponentReader, "NumberOfLevelUpPointsAppliedTamed", true);
                if (statusComponentReader.CheckIfValueExists("ExtraCharacterLevel"))
                    db.level += statusComponentReader.GetUInt16Property("ExtraCharacterLevel");
                if (statusComponentReader.HasProperty("ExperiencePoints"))
                    db.experience = statusComponentReader.GetFloatProperty("ExperiencePoints");
                else
                    db.experience = 0;

                db.is_baby = reader.GetBooleanProperty("bIsBaby");
                if (db.is_baby)
                {
                    db.baby_age = reader.GetFloatProperty("BabyAge");
                    db.next_imprint_time = -1;
                    if (reader.HasProperty("BabyNextCuddleTime"))
                        db.next_imprint_time = reader.GetDoubleProperty("BabyNextCuddleTime");
                    if (statusComponentReader.HasProperty("DinoImprintingQuality"))
                        db.imprint_quality = statusComponentReader.GetFloatProperty("DinoImprintingQuality");
                    else
                        db.imprint_quality = 0;
                }
            }

            return db;
        }
    }
}
