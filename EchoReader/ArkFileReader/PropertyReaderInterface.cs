using EchoReader.ArkFileReader.Properties;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EchoReader.ArkFileReader
{
    /// <summary>
    /// This should be used by the class that we're exporting
    /// </summary>
    public interface PropertyReaderInterface
    {
        StreamPropertyCallback GetPropertyCallback(string name, string type); //Called when a property is being streamed. Return null to skip, or return a function to read
    }

    /// <summary>
    /// Callback when a property was read
    /// </summary>
    /// <param name="prop"></param>
    /// <param name="name"></param>
    /// <param name="type"></param>
    public delegate void StreamPropertyCallback(BaseProperty prop, string name, string type, int index);
}
