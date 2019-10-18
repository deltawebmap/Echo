using LibDeltaSystem.Db.Content;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace EchoReader.ServerJobs
{
    public static class JobSyncTribeLogs
    {
        private static readonly Regex r = new Regex(@"Day ([0-9])+, ([0-9])+:([0-9])+:([0-9])+: "); //Regex for matching the entire message
        private static readonly Regex hr = new Regex(@"([0-9])+"); //Regex for matching header params
        private static readonly Regex rc = new Regex("<RichColor Color=\"([0-9]), ([0-9]), ([0-9]), ([0-9])\">(.+)<\\/>"); //Regex for matching RichColor code
        private static readonly Regex rche = new Regex("([0-9]), ([0-9]), ([0-9]), ([0-9])\">"); //Regex used for matching the end of the RichColor header

        public static async Task SyncTribeLogs(string[] logs, string server_id, int tribe_id)
        {
            //Fetch old tribe log entries so that we can compare later
            List<DbTribeLogEntry> seen = await FindLatestEntriesOnDb(server_id, tribe_id, logs.Length);

            //We'll loop through and regex each log to ensure that is is compatible. THIS MIGHT NOT WORK ON NON-ENGLISH GAMES!!!
            bool warn = false;
            List<DbTribeLogEntry> add = new List<DbTribeLogEntry>();
            int validCount = 0;
            int addMinDay = int.MaxValue;
            int addMaxDay = int.MinValue;
            long index = DateTime.UtcNow.Ticks;
            foreach (var s in logs)
            {
                //Regex this
                Match m = r.Match(s);
                if(!m.Success)
                {
                    warn = true;
                    continue;
                }

                //Now, extract the group regions
                string header = m.Value;
                string content = s.Substring(header.Length);

                //Read header to extract data
                var headerMatches = hr.Matches(header);
                int day = int.Parse(headerMatches[0].Value);
                int hour = int.Parse(headerMatches[1].Value);
                int min = int.Parse(headerMatches[2].Value);
                int sec = int.Parse(headerMatches[3].Value);

                //Now, match RichColor data
                var contentRichMatches = rc.Matches(content).ToArray();
                string finalContent = "";
                string finalColor = "#ffffffff";
                bool hasColor = false;
                if(contentRichMatches.Length == 0)
                {
                    //Nothing fancy going on here
                    finalContent = content;
                } else if(contentRichMatches.Length == 1)
                {
                    //There is rich color
                    finalContent += ReadRichColorParam(contentRichMatches[0].Value, out string color);
                    if (!hasColor)
                        finalColor = color;
                    hasColor = true;
                } else
                {
                    //Something weird is up...
                    continue;
                }

                //Skip if content is blank
                if (finalContent.Length == 0)
                    continue;

                //Add to valid count
                validCount++;

                //Check if this already exists in the database. Notice that we do not check locally to see if an object was already added. This is intentional, as multiple exact same entries can exist but still be valid.
                if (seen.Where(x => x.content == finalContent && x.color == finalColor && x.day == day && x.hour == hour && x.min == min && x.sec == sec).Count() != 0)
                    continue; //Already seen

                //Set min/max
                addMinDay = Math.Min(addMinDay, day);
                addMaxDay = Math.Max(addMaxDay, day);

                //Add this to a list of objects to be added
                add.Add(new DbTribeLogEntry
                {
                    color = finalColor,
                    content = finalContent,
                    day = day,
                    hour = hour,
                    min = min,
                    sec = sec,
                    hash = "",
                    has_color = hasColor,
                    realtime = true, //Will be set in the next part
                    seen = DateTime.UtcNow,
                    server_id = server_id,
                    token = "",
                    tribe_id = tribe_id,
                    index = index++
                });
            }

            //If there's nothing to add, skip
            if (add.Count == 0)
                return;

            //Now, we'll determine if these are realtime or not
            bool realtime = (add.Count != validCount) //Check if the length of logs matches the number of results
                && ((addMaxDay - addMinDay) < 5); //Being generous, an Ark day is 30 minutes IRL. Five would be 2.5 hours, more than enough time than this should be uploading and refreshing

            //Update
            foreach (var v in add)
                v.realtime = realtime;

            //Add all
            await Program.conn.content_tribe_log.InsertManyAsync(add);
        }

        /// <summary>
        /// Finds the latest entries from the database
        /// </summary>
        /// <param name="server_id"></param>
        /// <param name="tribe_id"></param>
        /// <param name="limit"></param>
        /// <returns></returns>
        private static async Task<List<DbTribeLogEntry>> FindLatestEntriesOnDb(string server_id, int tribe_id, int limit)
        {
            var filterBuilder = Builders<DbTribeLogEntry>.Filter;
            var filter = filterBuilder.Eq("tribe_id", tribe_id) & filterBuilder.Eq("server_id", server_id);
            var results = await Program.conn.content_tribe_log.FindAsync(filter, new FindOptions<DbTribeLogEntry, DbTribeLogEntry>
            {
                Sort = Builders<DbTribeLogEntry>.Sort.Descending("index"),
                Limit = limit
            });
            return await results.ToListAsync();
        }

        /// <summary>
        /// Escapes rich color content
        /// </summary>
        /// <param name="content"></param>
        private static string ReadRichColorParam(string content, out string color)
        {
            //We can safely ignore the first bit of this because the regex ensures it was always constant. Same goes for the last bit
            content = content.Substring("<RichColor Color=\"".Length);
            content = content.Substring(0, content.Length - 3);

            //Now, extract the color params
            var colorMatches = hr.Matches(content);
            float r = float.Parse(colorMatches[0].Value);
            float g = float.Parse(colorMatches[1].Value);
            float b = float.Parse(colorMatches[2].Value);
            float a = float.Parse(colorMatches[3].Value);

            //Convert to hex color code
            color = "#" + HelperColorChannelFloatToHex(r) + HelperColorChannelFloatToHex(g) + HelperColorChannelFloatToHex(b) + HelperColorChannelFloatToHex(a);

            //Now, finish up by getting the raw content
            content = content.Substring(rche.Match(content).Length);
            return content;
        }

        /// <summary>
        /// Converts a color to a hex number. Used for producing hex color codes.
        /// </summary>
        /// <param name="f"></param>
        /// <returns></returns>
        private static string HelperColorChannelFloatToHex(float f)
        {
            byte b = (byte)(f * 255);
            return b.ToString("x2");
        }
    }
}
