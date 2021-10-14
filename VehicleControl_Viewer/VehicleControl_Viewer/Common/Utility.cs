using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VehicleControl_Viewer.Protots;

namespace VehicleControl_Viewer.Common
{
    public class Utility
    {

        public static T ToObject<T>(byte[] buf) where T : Google.Protobuf.IMessage<T>, new()
        {
            if (buf == null)
                return default(T);
            Google.Protobuf.MessageParser<T> parser = new Google.Protobuf.MessageParser<T>(() => new T());
            return parser.ParseFrom(buf);
        }
        public static Boolean isMatche(String str1, String str2)
        {
            try
            {
                if (str1 == str2) return true;
                if (str1 == null || str2 == null) return false;
                return str1.Trim().Equals(str2.Trim(), StringComparison.OrdinalIgnoreCase);
            }
            catch (Exception e)
            {
                //LogManager.GetCurrentClassLogger().Warn(e, "Exception");
            }
            return false;
        }

    }
}
