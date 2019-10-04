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
    public class JobSyncStructures
    {
        public ArkSaveEditor.Deserializer.DotArk.DotArkDeserializer deser;
        public string server_id;
        public DeltaContentDbSyncSession<DbStructure> sync;

        public JobSyncStructures(ArkServer s, ArkSaveEditor.Deserializer.DotArk.DotArkDeserializer deser)
        {
            server_id = s.id;
            sync = new DeltaContentDbSyncSession<DbStructure>(Program.conn.content_structures, server_id);
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
            if (!GetSupported(reader))
                return;

            //Get hash code
            string hash = obj.GetHash(props);

            //Get dino token
            string token = GetToken(obj);

            //First, check if an update is even needed.
            if (!sync.CheckIfUpdateRequired(token, hash))
                return;

            //Convert dino
            DbStructure structure = ConvertStructure(obj, reader, token);

            //Update
            await sync.UpdateOne(structure, token, hash);

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

        private string GetToken(DotArkGameObject obj)
        {
            return $"{obj.classname.classname}@{obj.locationData.x}@{obj.locationData.y}@{obj.locationData.z}";
        }

        private bool GetSupported(ArkPropertyReader reader)
        {
            return reader.CheckIfValueExists("TargetingTeam");
        }

        public DbStructure ConvertStructure(DotArkGameObject obj, ArkPropertyReader reader, string token)
        {
            //Create data
            int tribeId = reader.GetInt32Property("TargetingTeam");
            DbStructure db = new DbStructure
            {
                classname = obj.classname.classname,
                tribe_id = tribeId,
                server_id = server_id,
                token = token,
                location = new DbLocation
                {
                    x = obj.locationData.x,
                    y = obj.locationData.y,
                    z = obj.locationData.z,
                    pitch = obj.locationData.pitch,
                    yaw = obj.locationData.yaw,
                    roll = obj.locationData.roll,
                },
                has_inventory = reader.HasProperty("MyInventoryComponent")
            };

            //Set optional data
            if (reader.HasProperty("CurrentItemCount"))
                db.current_item_count = reader.GetInt32Property("CurrentItemCount");
            if (reader.HasProperty("MaxItemCount"))
                db.max_item_count = reader.GetInt32Property("MaxItemCount");
            if (reader.HasProperty("Health"))
                db.current_health = reader.GetFloatProperty("Health");
            if (reader.HasProperty("MaxHealth"))
                db.max_health = reader.GetFloatProperty("MaxHealth");

            return db;
        }
    }
}
