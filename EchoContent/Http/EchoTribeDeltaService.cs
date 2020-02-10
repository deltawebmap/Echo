﻿using LibDeltaSystem;
using LibDeltaSystem.Entities.ArkEntries;
using LibDeltaSystem.WebFramework.ServiceTemplates;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EchoContent.Http
{
    /// <summary>
    /// Base service used for echo tribes
    /// </summary>
    public abstract class EchoTribeDeltaService : RequestTribeServerDeltaService
    {
        public ArkMapEntry mapInfo;
        public DeltaPrimalDataPackage package;

        public EchoTribeDeltaService(DeltaConnection conn, HttpContext e) : base(conn, e)
        {
        }

        public override async Task<bool> CheckIfTribeIdAllowed(int? tribeId)
        {
            //Check actual tribe
            int? actualTribe = await server.TryGetTribeIdAsync(conn, user.steam_id);
            if (tribeId.HasValue && actualTribe == tribeId)
                return true;

            //For users to request other tribes or a null tribe, they must have admin access. Check
            return server.CheckIsUserAdmin(user);
        }

        public override async Task<bool> SetArgs(Dictionary<string, string> args)
        {
            if (!await base.SetArgs(args))
                return false;

            //Gather package info
            package = await Program.conn.GetPrimalDataPackage(new string[0]);

            //Get this map from the maps
            mapInfo = await server.GetMapEntryAsync(conn);
            if (mapInfo == null)
            {
                await WriteString("The map this server is using is not supported.", "text/plain", 400);
                return false;
            }

            return true;
        }
    }
}