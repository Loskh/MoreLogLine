using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nhaama;
using Nhaama.Memory;
using System.Text.RegularExpressions;
using Advanced_Combat_Tracker;
using System.Threading;
using System.Data;

namespace MoreLogLine
{
    class Craft
    {
        private static NhaamaProcess _FFXIVProcess;
        //private MoreLogLineUI _UI;
        public static bool isCrafting = false;
        private static uint Progress;
        private static ulong ProgressOffset = (ulong)0x1CAB98C;
        private static ulong ProgressPtr;
        private static uint Step;
        private static ulong StepOffset = ProgressOffset - 4;
        private static ulong StepPtr;
        private static uint Quality;
        private static ulong QualityOffset = ProgressOffset + 8;
        private static ulong QualityPtr;
        //private uint HQRate;
        //private ulong HQRateOffset = ProgressOffset + 0x10+4;
        private static uint Durability;
        private static ulong DurabilityOffset = ProgressOffset + 0x14;
        private static ulong DurabilityPtr;
        private static UInt16 Condition;
        private static ulong ConditionOffset = ProgressOffset + 0x1C;
        private static ulong ConditionPtr;
        //normal=1, good=2, excellent=3, poor=4
        private static uint Craftsmanship;
        private static ulong CraftsmanshipOffset = (ulong)0x1C8B6D8 + 0x160 + 4 * 0x46;
        private static ulong CraftsmanshipPtr;
        private static uint Control;
        private static ulong ControlOffset = (ulong)0x1C8B6D8 + 0x160 + 4 * 0x47;
        private static ulong ControlPtr;
        private static uint MaxCP;
        private static ulong MaxCPOffset = (ulong)0x1C8B6D8 + 0x160 + 4 * 0x0b;
        private static ulong MaxCPPtr;
        private static uint CurrentCP;
        private static Nhaama.Memory.Pointer CurrentCPPtr;


        private static Regex StartCraft = new Regex("^.{14} 00:0842:([^:]+?)从背包里取出了材料。");
        private static Regex EndCraft = new Regex("^.{14} 00:0842:([^:]+?)中止了制作作业。");
        private static Regex CraftSkill = new Regex("^.{14} 00:08(42|2b):([^:]+?)发动([^:]+?)");
        private static Regex CraftSucess = new Regex("^.{14} 00:08c2:([^:]+?)制作([^:]+?)成功");

        public static void InitPtr(NhaamaProcess FFXIVProcess) {
            try {
                _FFXIVProcess = FFXIVProcess;
                ulong ffxivOffset = _FFXIVProcess.GetModuleBasedOffset("ffxiv_dx11.exe", 0);
                ProgressPtr = ffxivOffset + ProgressOffset;
                QualityPtr = ffxivOffset + QualityOffset;
                StepPtr = ffxivOffset + StepOffset;
                DurabilityPtr = ffxivOffset + DurabilityOffset;
                ConditionPtr = ffxivOffset + ConditionOffset;
                CraftsmanshipPtr = ffxivOffset + CraftsmanshipOffset;
                ControlPtr = ffxivOffset + ControlOffset;
                MaxCPPtr = ffxivOffset + MaxCPOffset;
                CurrentCPPtr = new Nhaama.Memory.Pointer(FFXIVProcess, 0x1C62680, 0x18AE);
                MoreLogLineUI.AddParserMessage("Points Found!");
            }
            catch {
                MoreLogLineUI.AddParserMessage("Points Not Found!");
                throw new Exception("Could not find Points.");
            }
        }
        public static void CleanStatus() {
            Progress = Quality = Durability = 0;
            Step = Condition = 1;
        }
        public static void ReadStatus() {
            Step = _FFXIVProcess.ReadUInt32(StepPtr);
            Progress = _FFXIVProcess.ReadUInt32(ProgressPtr);
            Quality = _FFXIVProcess.ReadUInt32(QualityPtr);
            //HQRate = FFXIVProcess.ReadUInt32(FFXIVProcess.GetModuleBasedOffset("ffxiv_dx11.exe", HQRateOffset));
            Durability = _FFXIVProcess.ReadUInt32(DurabilityPtr);
            Condition = _FFXIVProcess.ReadUInt16(ConditionPtr);
            //normal=1, good=2, excellent=3, poor=4
            Craftsmanship = _FFXIVProcess.ReadUInt32(CraftsmanshipPtr);
            Control = _FFXIVProcess.ReadUInt32(ControlPtr);
            MaxCP = _FFXIVProcess.ReadUInt32(MaxCPPtr);
            CurrentCP = _FFXIVProcess.ReadUInt16(CurrentCPPtr);
        }

        public static void ProcessLogLine(bool isImport, DateTime dateTime, string logline) {
            if (StartCraft.IsMatch(logline)) {
                isCrafting = true;
                OnLogLine_CraftStart(isImport, dateTime);
            }
            else if (isCrafting == true) {
                if (CraftSkill.IsMatch(logline)) {
                    OnLogLine_CraftSkill(isImport, dateTime);

                }
                else if (EndCraft.IsMatch(logline) || CraftSucess.IsMatch(logline)) {
                    OnLogLine_CraftEnd(isImport, dateTime);
                    isCrafting = false;
                }
            }

        }
        private static string GenCraftLogLine(DateTime time) {
            string timestamp = "[" + time.ToString("HH:mm:ss.fff") + "] ";
            return $"{timestamp}{MessageType.CraftHead}:{Step:X}:{Progress:X}:{Quality:X}:{Durability:X}:{Condition:X}:{CurrentCP:X}:{MaxCP:X}:{Control:X}:{Craftsmanship:X}";
        }
        private static void OnLogLine_CraftStart(bool isImport,DateTime time) {
            ReadStatus();
            CleanStatus();
            string logline= GenCraftLogLine(time);
            LogLineEventArgs logLine = new LogLineEventArgs(logline, 0, ActGlobals.oFormActMain.LastEstimatedTime, ActGlobals.oFormActMain.ActiveZone.ZoneName, false);
            LogLineHandle.SendLogLine(isImport, logLine);
        }
        private static void OnLogLine_CraftSkill(bool isImport,DateTime time) {
            Thread.Sleep(200);
            ReadStatus();
            string logline = GenCraftLogLine(time);
            LogLineEventArgs logLine = new LogLineEventArgs(logline, 0, ActGlobals.oFormActMain.LastEstimatedTime, ActGlobals.oFormActMain.ActiveZone.ZoneName, false);
            LogLineHandle.SendLogLine(isImport, logLine);
        }
        private static void OnLogLine_CraftEnd(bool isImport,DateTime time) {
            ReadStatus();
            string logline = GenCraftLogLine(time);
            LogLineEventArgs logLine = new LogLineEventArgs(logline, 0, ActGlobals.oFormActMain.LastEstimatedTime, ActGlobals.oFormActMain.ActiveZone.ZoneName, false);
            LogLineHandle.SendLogLine(isImport, logLine);
            CleanStatus();
        }
    }



}
