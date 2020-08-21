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

using Lumina;
using Lumina.Excel.GeneratedSheets;
using Nhaama;
using Nhaama.Memory;
using Advanced_Combat_Tracker;
using FFXIV_ACT_Plugin;
using FFXIV_ACT_Plugin.Common;
using FFXIV_ACT_Plugin.Logfile;
using FFXIV_ACT_Plugin.Common.Models;
using System.Text.RegularExpressions;
using Machina.FFXIV.Headers;

namespace MoreLogLine
{
    public class MainClass : UserControl, IActPluginV1
    {

        public static Lumina.Lumina lumina = null;

        private static FFXIV_ACT_Plugin.FFXIV_ACT_Plugin ffxivPlugin;

        private static MoreLogLineUI PluginUI;
        private static Process FFXIV;
        //private static Craft CraftInfo;

        public void InitPlugin(TabPage pluginScreenSpace, Label pluginStatusText) {
            pluginScreenSpace.Text = "MoreLogLine";
            PluginUI = new MoreLogLineUI();
            //CraftInfo = new Craft();
            PluginUI.InitializeComponent(pluginScreenSpace);
            ActGlobals.oFormActMain.OnLogLineRead += new LogLineEventDelegate(this.MoreLogLines_OnLogLineRead);
            LogLineHandle.GetOnLogReadList();
            GetFfxivPlugin();


            Task.Run(() => {
                while (FFXIV == null) {
                    FFXIV = ffxivPlugin.DataRepository.GetCurrentFFXIVProcess();
                    Thread.Sleep(1000);
                }
            }).ContinueWith((t) => {
                GetFFXIVProcess();
                MoreLogLineUI.AddParserMessage($"CurrentPID:{ffxivPlugin.DataRepository.GetCurrentFFXIVProcess().Id}");
                //ActGlobals.oFormActMain.BeforeLogLineRead += new LogLineEventDelegate(this.oFormActMain_BeforeLogLineRead);

            });
        }



        private void GetFfxivPlugin() {
            ffxivPlugin = null;
            List<ActPluginData> _ffxiv_act_plugins;
            _ffxiv_act_plugins = ActGlobals.oFormActMain.ActPlugins.Where(x => x.pluginFile.Name.ToUpper().Contains("FFXIV_ACT_Plugin".ToUpper()) && x.lblPluginStatus.Text.ToUpper().Contains("FFXIV Plugin Started.".ToUpper())).ToList();
            if (_ffxiv_act_plugins.Count != 1)
                return;
            //MoreLogLineUI.AddParserMessage($"{_ffxiv_act_plugins.Count}");
            ffxivPlugin = (FFXIV_ACT_Plugin.FFXIV_ACT_Plugin)_ffxiv_act_plugins[0].pluginObj;
            //MoreLogLineUI.AddParserMessage($"{ffxivPlugin.DataRepository.GetCurrentFFXIVProcess().Id}");
        }
        private void GetFFXIVProcess() {
            FFXIV = ffxivPlugin.DataRepository.GetCurrentFFXIVProcess();
            if (FFXIV == null) {
                MoreLogLineUI.AddParserMessage("FFXIVProcess NOT Found");
                return;
            }
            MoreLogLineUI.AddParserMessage("Found FFXIVProcess");
            NhaamaProcess FFXIVProcess = FFXIV.GetNhaamaProcess();
            var GameDir = Path.Combine(FFXIV.MainModule.FileName, "..\\sqpack");
            MoreLogLineUI.AddParserMessage(GameDir);
            lumina = new Lumina.Lumina(GameDir);
            Craft.InitPtr(FFXIVProcess);
            CraftNetwork.Init(lumina, ffxivPlugin);
            ffxivPlugin.DataSubscription.NetworkReceived += new NetworkReceivedDelegate(this.MoreLogLines_OnNetworkReceived);

        }



        private void MoreLogLines_OnNetworkReceived(string connection, long epoch, byte[] message) {

            if (BitConverter.ToUInt16(message, 18) == OpCode.StatusEffectList) {
                CraftNetwork.BuffListProcess(epoch, message);
            }
        }

        private void MoreLogLines_OnLogLineRead(bool isImport, LogLineEventArgs logInfo) {
            DateTime dateTime = Utils.ParseLogDateTime(logInfo.logLine);
            string logline = logInfo.logLine;
            //AddParserMessage(logline);
            Craft.ProcessLogLine(isImport, dateTime, logline);
            //LogLineEventArgs logLine = new LogLineEventArgs("111111111111", 0, ActGlobals.oFormActMain.LastEstimatedTime, ActGlobals.oFormActMain.ActiveZone.ZoneName, false);
            //BoradCastLogLine(isImport, logLine);

            //SendLogLine(isImport, logLine);


        }


        public void DeInitPlugin() {
            ActGlobals.oFormActMain.OnLogLineRead -= new LogLineEventDelegate(this.MoreLogLines_OnLogLineRead);
            ffxivPlugin.DataSubscription.NetworkReceived -= new NetworkReceivedDelegate(this.MoreLogLines_OnNetworkReceived);
            //ffxivPlugin.DataSubscription.ProcessChanged -= new ProcessChangedDelegate(this.MoreLogLines_OnProcessChanged);
        }

    }
}
