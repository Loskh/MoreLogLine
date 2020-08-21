using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Advanced_Combat_Tracker;
using Lumina;
using Lumina.Excel.GeneratedSheets;

namespace MoreLogLine
{
    class CraftNetwork
    {
        private static Lumina.Lumina _lumina;
        private static FFXIV_ACT_Plugin.FFXIV_ACT_Plugin _ffxivPlugin;

        private static byte Inner_Quiet;
        private static byte Veneration;
        private static byte Innovation;
        private static byte Conviction_Marcato;
        private static byte Waste_Not;
        private static byte Waste_Not_II;
        private static byte Manipulation;
        public static void Init(Lumina.Lumina lumina, FFXIV_ACT_Plugin.FFXIV_ACT_Plugin ffxivPlugin) {
            _lumina = lumina;
            _ffxivPlugin = ffxivPlugin;

        }
        public static void BuffListProcess(long epoch, byte[] message) {
            if (Craft.isCrafting == false)
                return;
            var ActorID = BitConverter.ToUInt32(message, 4);
            if (ActorID != _ffxivPlugin.DataRepository.GetCurrentPlayerID())
                return;
            DateTime packetDate;
            if (epoch > 0) {
                packetDate = Utils.EpochToDateTime(epoch).ToLocalTime();
            }
            else {
                packetDate = DateTime.MinValue;
            }
            //byte[] JobData = message.Skip(32).Take(16).ToArray();
            //var CurrentMP = BitConverter.ToUInt16(JobData, 12);
            //var MaxMP = BitConverter.ToUInt16(JobData, 14);
            //AddParserMessage($"CurrentMP={CurrentMP}");
            //AddParserMessage($"MaxMP={MaxMP}");
            byte[] BuffList = message.Skip(52).ToArray();
            for (int i = 0; i < 30; i++) {
                byte[] BuffArrary = BuffList.Skip(i * 12).Take(12).ToArray();
                var Buff = BitConverter.ToUInt16(BuffArrary, 0);
                if (Buff == 0)
                    break;
                switch (Buff) {
                    //case 0x00:
                    //    break;
                    case 0xFB: //内静
                    //case 0xFC: //俭约
                    //case 0xFE: //阔步
                    //case 0x101://长期俭约
                    ////case 0x102://掌握
                    //case 0x103://改革
                    ////case 0x367://元素之美名
                    //case 0x48C://掌握
                    //case 0x88D://改革
                    //case 0x88F://坚信
                    //case 0x8B2://崇敬
                        var BuffName = _lumina.GetExcelSheet<Status>(Lumina.Data.Language.ChineseSimplified).GetRow((uint)(Buff)).Name;
                        var StackNum = BitConverter.ToUInt16(BuffArrary, 2);
                        var duartion = BitConverter.ToUInt32(BuffArrary, 4);
                        var sourceActorID = BitConverter.ToUInt32(BuffArrary, 8);
                        MoreLogLineUI.AddParserMessage($"BuffID={Buff} BuffName={BuffName} StackNum={StackNum} duartion={duartion} sourceActorID={sourceActorID:X}");
                        string timestamp = "[" + packetDate.ToString("HH:mm:ss.fff") + "] ";
                        string logline = $"{timestamp}{MessageType.BuffHead}:{ActorID:X}:{Buff:X}:{BuffName}:{StackNum:X}:{duartion:X}:{sourceActorID:X}";
                        LogLineEventArgs logLine = new LogLineEventArgs(logline, 0, ActGlobals.oFormActMain.LastEstimatedTime, ActGlobals.oFormActMain.ActiveZone.ZoneName, false);
                        LogLineHandle.SendLogLine(false, logLine);
                        break;
                }
            }

        }
    }
}
