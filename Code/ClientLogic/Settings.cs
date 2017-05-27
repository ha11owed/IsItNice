using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientLogic
{
    /// <summary>
    /// The global settings of the Client.
    /// </summary>
    public static class Settings
    {
        public static string MissingUserImageURI = null;
        public static string MissingNiceRequestImageURI = null;

        public static int MaxLatestItemsCount = 4;
    }
}
