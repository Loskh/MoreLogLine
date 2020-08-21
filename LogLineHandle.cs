using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Reflection;
using System.Xml;
using System.Linq;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Advanced_Combat_Tracker;

namespace MoreLogLine
{
    class LogLineHandle
    {
        private static LogLineEventDelegate BoradCastLogLine;
        


        public static void GetOnLogReadList() {
            FormActMain oFormActMain = ActGlobals.oFormActMain;
            Delegate[] invokeList = Utils.GetObjectEventList(oFormActMain, "OnLogLineRead");
            MoreLogLineUI.AddParserMessage($"OnLogLineRead:");
            using (IEnumerator<LogLineEventDelegate> enumerator = invokeList.Cast<LogLineEventDelegate>().GetEnumerator()) {
                while (enumerator.MoveNext()) {
                    LogLineEventDelegate logLineEventDelegate = enumerator.Current;
                    try {
                        MethodInfo info = logLineEventDelegate.Method;
                        string str = info.Name;
                        if (str != "MoreLogLines_OnLogLineRead") {
                            BoradCastLogLine += logLineEventDelegate;
                            //OnLogLineReadList.Add(logLineEventDelegate);
                        }
                        //logLineEventDelegate2(isImport, logLineEventArgs3);
                        MoreLogLineUI.AddParserMessage($"{str}");
                    }
                    catch {
                        throw new Exception("Failed");
                    }
                }
            }
        }

        public static void SendLogLine(bool isImport, LogLineEventArgs logline) {
            MoreLogLineUI.AddParserMessage($"{logline.logLine}");
            //PluginUI.AddParserMessage($"Step:{Step}:Progress:{Progress}:Quality:{Quality}:Durability:{Durability}:Condition:{Condition}:CurrentCP:{CurrentCP}:MaxCP:{MaxCP}:Control:{Control}:Craftsmanship{Craftsmanship}");

            BoradCastLogLine(isImport, logline);

        }

    }
}
