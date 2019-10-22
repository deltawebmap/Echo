using EchoReader.ArkFileReader;
using EchoReader.FastRead.ExtEntities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace EchoReader.FastRead
{
    public class FastReadSession
    {
        public ArkFile ark;
        public string server_id;

        public async Task OpenSession(Stream s, string server_id)
        {
            //Open and read ARK headers
            ark = new ArkFile(s);
            this.server_id = server_id;
            await ark.ReadHeaders();

            //Now, stream objects
            foreach(var o in ark.game_objects)
            {
                //Determine the type of file
                //We're going to determine what this is.
                string classnameOriginal = o.classname;
                string classname = classnameOriginal;
                if (classname.EndsWith("_C"))
                    classname = classname.Substring(0, classname.Length - 2);

                //Decide
                if (ArkSaveEditor.ArkImports.GetDinoDataByClassname(classname) != null)
                {
                    //This is a dinosaur.
                    DinoFastReader fr = new DinoFastReader(o, this);
                    await o.Open(ark, fr);
                }
            }
        }
    }
}
