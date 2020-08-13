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

using Nhaama;
using Nhaama.Memory;
using Advanced_Combat_Tracker;
using FFXIV_ACT_Plugin;
using FFXIV_ACT_Plugin.Common;
using FFXIV_ACT_Plugin.Logfile;
using FFXIV_ACT_Plugin.Common.Models;
using System.Text.RegularExpressions;

namespace MoreLogLine
{
    public class MainClass : UserControl, IActPluginV1
    {
        private NhaamaProcess FFXIVProcess;
        private Process process;
        private ListBox lstMessages;
        private Button cmdClearMessages;
        private Button cmdCopyProblematic;
        private GroupBox grpDebug;

        private static uint Progress;
        private static ulong ProgressOffset = (ulong)0x1CAB98C;
        private static ulong ProgressPtr;
        private static uint Quality;
        private static ulong QualityOffset = ProgressOffset + 8;
        private static ulong QualityPtr;
        private static uint Step;
        private static ulong StepOffset = ProgressOffset + 0x10;
        private static ulong StepPtr;
        //private uint HQRate;
        //private ulong HQRateOffset = (ulong)0x1CAB994;
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
        private Nhaama.Memory.Pointer CurrentCPPtr;

        private List<LogLineEventDelegate> OnLogLineReadList;
        public void InitPlugin(TabPage pluginScreenSpace, Label pluginStatusText) {
            pluginScreenSpace.Text = "MoreLogLine";
            InitializeComponent(pluginScreenSpace);
            Task.Run(() => {
                while (process == null) {
                    GetFFXIVProcess();
                    Thread.Sleep(1000);
                }
            }).ContinueWith((t) => {
                InitPtr();
                ActGlobals.oFormActMain.OnLogLineRead += new LogLineEventDelegate(this.MoreLogLines_OnLogLineRead);
                GetOnLogReadList();
                //ActGlobals.oFormActMain.BeforeLogLineRead += new LogLineEventDelegate(this.oFormActMain_BeforeLogLineRead);

            });
        }
        public void InitPtr() {
            try {
                ulong ffxivOffset = FFXIVProcess.GetModuleBasedOffset("ffxiv_dx11.exe", 0);
                ProgressPtr = ffxivOffset + ProgressOffset;
                QualityPtr = ffxivOffset + QualityOffset;
                StepPtr = ffxivOffset + StepOffset;
                DurabilityPtr = ffxivOffset + DurabilityOffset;
                ConditionPtr = ffxivOffset + ConditionOffset;
                CraftsmanshipPtr = ffxivOffset + CraftsmanshipOffset;
                ControlPtr = ffxivOffset + ControlOffset;
                MaxCPPtr = ffxivOffset + MaxCPOffset;
                CurrentCPPtr = new Nhaama.Memory.Pointer(FFXIVProcess, 0x1C62680, 0x18AE);
                AddParserMessage("Points Found!");
            }
            catch {
                AddParserMessage("Points Not Found!");
                throw new Exception("Could not find Points.");
            }
        }

        public void CleanStatus() {
            Step = Progress = Quality = Durability = 0;
            Condition = 1;
        }
        public void ReadStatus() {
            Step = FFXIVProcess.ReadUInt32(StepPtr);
            Progress = FFXIVProcess.ReadUInt32(ProgressPtr);
            Quality = FFXIVProcess.ReadUInt32(QualityPtr);
            //HQRate = FFXIVProcess.ReadUInt32(FFXIVProcess.GetModuleBasedOffset("ffxiv_dx11.exe", HQRateOffset));
            Durability = FFXIVProcess.ReadUInt32(DurabilityPtr);
            Condition = FFXIVProcess.ReadUInt16(ConditionPtr);
            //normal=1, good=2, excellent=3, poor=4
            Craftsmanship = FFXIVProcess.ReadUInt32(CraftsmanshipPtr);
            Control = FFXIVProcess.ReadUInt32(ControlPtr);
            MaxCP = FFXIVProcess.ReadUInt32(MaxCPPtr);
            CurrentCP = FFXIVProcess.ReadUInt16(CurrentCPPtr);
        }
        private void GetFFXIVProcess() {
            List<ActPluginData> _ffxiv_act_plugins;
            _ffxiv_act_plugins = ActGlobals.oFormActMain.ActPlugins.Where(x => x.pluginFile.Name.ToUpper().Contains("FFXIV_ACT_Plugin".ToUpper()) && x.lblPluginStatus.Text.ToUpper().Contains("FFXIV Plugin Started.".ToUpper())).ToList();
            if (_ffxiv_act_plugins.Count != 1)
                return;
            FFXIV_ACT_Plugin.FFXIV_ACT_Plugin parseplugin;
            parseplugin = (FFXIV_ACT_Plugin.FFXIV_ACT_Plugin)_ffxiv_act_plugins[0].pluginObj;
            AddParserMessage("Found FFXIVProcess");
            process = parseplugin.DataRepository.GetCurrentFFXIVProcess();
            FFXIVProcess = process.GetNhaamaProcess();
        }


        private void InitializeComponent(TabPage pluginScreenSpace) {
            lstMessages = new System.Windows.Forms.ListBox();
            cmdClearMessages = new System.Windows.Forms.Button();
            cmdCopyProblematic = new System.Windows.Forms.Button();
            grpDebug = new System.Windows.Forms.GroupBox();
            lstMessages.FormattingEnabled = true;
            lstMessages.Location = new System.Drawing.Point(6, 20);
            lstMessages.Name = "lstMessages";
            lstMessages.ScrollAlwaysVisible = true;
            lstMessages.Size = new System.Drawing.Size(900, 900);
            lstMessages.TabIndex = 80;
            cmdClearMessages.Location = new System.Drawing.Point(42, 950);
            cmdClearMessages.Name = "cmdClearMessages";
            cmdClearMessages.Size = new System.Drawing.Size(97, 26);
            cmdClearMessages.TabIndex = 82;
            cmdClearMessages.Text = "清空日志";
            cmdClearMessages.UseVisualStyleBackColor = true;
            cmdClearMessages.Click += new System.EventHandler(cmdClearMessages_Click);
            cmdCopyProblematic.Location = new System.Drawing.Point(537, 950);
            cmdCopyProblematic.Name = "cmdCopyProblematic";
            cmdCopyProblematic.Size = new System.Drawing.Size(109, 26);
            cmdCopyProblematic.TabIndex = 81;
            cmdCopyProblematic.Text = "复制到剪贴板";
            cmdCopyProblematic.UseVisualStyleBackColor = true;
            cmdCopyProblematic.Click += new System.EventHandler(cmdCopyProblematic_Click);
            //grpDebug.Controls.Add(label1);

            grpDebug.Controls.Add(cmdClearMessages);
            grpDebug.Controls.Add(cmdCopyProblematic);
            grpDebug.Controls.Add(lstMessages);
            //grpDebug.Controls.Add(chkShowRealDoTs);
            //grpDebug.Controls.Add(chkParsePotency);
            //grpDebug.Controls.Add(chkSimulateDoTCrits);
            //grpDebug.Controls.Add(chkLogAllNetwork);
            //grpDebug.Controls.Add(chkGraphPotency);
            grpDebug.Location = new System.Drawing.Point(14, 14);
            grpDebug.Name = "grpDebug";
            grpDebug.Size = new System.Drawing.Size(1000, 1000);
            grpDebug.TabIndex = 75;
            grpDebug.TabStop = false;
            pluginScreenSpace.Controls.Add(grpDebug);
            pluginScreenSpace.Size = new System.Drawing.Size(1000, 1000);
            grpDebug.ResumeLayout(false);
            grpDebug.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
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

        private void GetOnLogReadList() {
            FormActMain oFormActMain = ActGlobals.oFormActMain;
            Delegate[] invokeList = GetObjectEventList(oFormActMain, "OnLogLineRead");
            DateTime dateTime = DateTime.Now;
            string timestamp = "[" + dateTime.ToString("HH:mm:ss.fff") + "] ";
            //LogLineEventArgs logLineEventArgs3 = new LogLineEventArgs($"{timestamp}FF:34:5611:11:TEST123", 0, oFormActMain.LastEstimatedTime, oFormActMain.ActiveZone.ZoneName, false);
            using (IEnumerator<LogLineEventDelegate> enumerator = invokeList.Cast<LogLineEventDelegate>().GetEnumerator()) {
                while (enumerator.MoveNext()) {
                    LogLineEventDelegate logLineEventDelegate = enumerator.Current;
                    try {
                        //logLineEventDelegate2.Invoke(isImport, logLineEventArgs3);
                        MethodInfo info = logLineEventDelegate.Method;
                        string str = info.Name;
                        if (str != "MoreLogLines_OnLogLineRead") {
                            AddParserMessage($"{str}");
                            OnLogLineReadList.Add(logLineEventDelegate);
                        }
                        //logLineEventDelegate2(isImport, logLineEventArgs3);
                        AddParserMessage($"OnLogLineRead:");
                        AddParserMessage($"{str}");
                        
                        //logLineEventDelegate2.
                    }
                    catch {
                        throw new Exception("Failed");
                    }
                }
            }
        }
        private void MoreLogLines_OnLogLineRead(bool isImport, LogLineEventArgs logInfo) {
            //ActGlobals.oFormActMain.ActiveZone.ActiveEncounter.LogLines.Add(new LogLineEntry(logInfo.detectedTime, "LOGLINELOGLINE", 0, ActGlobals.oFormActMain.GlobalTimeSorter));

        }

        private DateTime ParseLogDateTime(string message) {
            DateTime result = DateTime.MinValue;
            if (message == null || message.Length < 5) {
                return result;
            }
            if (message[2] == '|' && message.IndexOf('|', 3) > 0) {
                if (!DateTime.TryParse(message.Substring(3, message.IndexOf('|', 3) - 3), out result)) {
                    return DateTime.MinValue;
                }
            }
            else if (message[3] == '|' && message.IndexOf('|', 4) > 0) {
                return DateTime.MinValue;
            }
            return result;
        }

        private string GenLogLine(DateTime time) {
            //return $"{ Step.ToString("X")}:{Progress.ToString("X")}:{Quality.ToString("X")}:{HQRate.ToString("X")}:{Durability.ToString("X")}:{Condition.ToString("X")}:{Craftsmanship.ToString("X")}:{Control.ToString("X")}:{CurrentCP.ToString("X")}:{MaxCP.ToString("X")}";
            return $"{Step:X}:{Progress:X}:{Quality:X}:{Durability:X}:{Condition:X}:{CurrentCP:X}:{MaxCP:X}:{Control:X}:{Craftsmanship:X}";
        }
        private void oFormActMain_BeforeLogLineRead(bool isImport, LogLineEventArgs logInfo) {
            DateTime dateTime = ParseLogDateTime(logInfo.logLine);
            string logline = logInfo.logLine;
            Regex StartCraft = new Regex("^.{14} 00:0842:([^:]+?)从背包里取出了材料。");
            Regex EndCraft = new Regex("^.{14} 00:0842:([^:]+?)中止了制作作业。");
            Regex CraftSkill = new Regex("^.{14} 00:08(42|2b):([^:]+?)发动([^:]+?)⇒([^:]+?)");
            Regex CraftSucess = new Regex("^.{14} 00:08c2:([^:]+?)制作([^:]+?)成功");


            if (StartCraft.IsMatch(logline)) {
                CleanStatus();
                logInfo.logLine = logline + ":" + GenLogLine(dateTime);
                AddParserMessage(logInfo.logLine);

            }
            else if (CraftSkill.IsMatch(logline)) {
                ReadStatus();
                logInfo.logLine = logline + ":" + GenLogLine(dateTime);
                AddParserMessage(logInfo.logLine);
            }
            else if (EndCraft.IsMatch(logline)) {
                ReadStatus();
                logInfo.logLine = logline + ":" + GenLogLine(dateTime);
                CleanStatus();
                AddParserMessage(logInfo.logLine);
            }
            else if (CraftSucess.IsMatch(logline)) {
                ReadStatus();
                logInfo.logLine = logline + ":" + GenLogLine(dateTime);
                CleanStatus();
                AddParserMessage(logInfo.logLine);
            }


        }

        private void OnLogLine_CraftStart() {
            CleanStatus();
            throw new NotImplementedException();
        }
        private void OnLogLine_CraftSkill() {
            throw new NotImplementedException();
        }
        private void OnLogLine_CraftEnd() {
            throw new NotImplementedException();
        }

        public void DeInitPlugin() {
            //ActGlobals.oFormActMain.BeforeLogLineRead -= new LogLineEventDelegate(this.oFormActMain_BeforeLogLineRead);
            ActGlobals.oFormActMain.OnLogLineRead -= new LogLineEventDelegate(this.MoreLogLines_OnLogLineRead);
            throw new NotImplementedException();
        }

        private void cmdCopyProblematic_Click(object sender, EventArgs e) {
            StringBuilder stringBuilder = new StringBuilder();
            foreach (object item in lstMessages.Items) {
                stringBuilder.AppendLine((item ?? "").ToString());
            }
            if (stringBuilder.Length > 0) {
                Clipboard.SetText(stringBuilder.ToString());
            }
        }

        private void cmdClearMessages_Click(object sender, EventArgs e) {
            lstMessages.Items.Clear();
        }

        public void AddParserMessage(string message) {
            ACTWrapper.RunOnACTUIThread((Action)delegate {
                lstMessages.Items.Add(message);
            });
        }

    }
}
