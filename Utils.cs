using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace MoreLogLine
{
    class Utils {

        private static readonly DateTime MinEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        public static DateTime EpochToDateTime(long epoch) {
            return MinEpoch.AddMilliseconds(epoch);
        }

        public static DateTime ParseLogDateTime(string message) {
            DateTime result = DateTime.MinValue;
            if (message == null || message.Length < 5) {
                return result;
            }
            //AddParserMessage(message.Substring(1, 12));
            if (!DateTime.TryParse(message.Substring(1, 12), out result)) {
                return result.AddMilliseconds(100);
            }
            return result;
        }

        public static Delegate[] GetObjectEventList(object p_Object, string p_EventName) {
            FieldInfo _Field = p_Object.GetType().GetField(p_EventName, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public | BindingFlags.Static);
            if (_Field == null) {
                return null;
            }
            object _FieldValue = _Field.GetValue(p_Object);
            if (_FieldValue != null && _FieldValue is Delegate) {
                Delegate _ObjectDelegate = (Delegate)_FieldValue;
                return _ObjectDelegate.GetInvocationList();
            }
            return null;
        }

        public static void RemoveEvent<T>(T c, string name) {
            Delegate[] invokeList = GetObjectEventList(c, name);
            foreach (Delegate del in invokeList) {
                typeof(T).GetEvent(name).RemoveEventHandler(c, del);
            }
        } 

        public static string byteToHexStr(byte[] bytes) {
            string returnStr = "";
            if (bytes != null) {
                for (int i = 0; i < bytes.Length; i++) {
                    returnStr += bytes[i].ToString("X2");
                }
            }
            return returnStr;
        }

    }
}
