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
    class MoreLogLineUI : UserControl
    {
        private static ListBox lstMessages;
        private static Button cmdClearMessages;
        private static Button cmdCopyProblematic;
        private static GroupBox grpDebug;

        public void InitializeComponent(TabPage pluginScreenSpace) {
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

        public static void cmdCopyProblematic_Click(object sender, EventArgs e) {
            StringBuilder stringBuilder = new StringBuilder();
            foreach (object item in lstMessages.Items) {
                stringBuilder.AppendLine((item ?? "").ToString());
            }
            if (stringBuilder.Length > 0) {
                Clipboard.SetText(stringBuilder.ToString());
            }
        }

        public static void cmdClearMessages_Click(object sender, EventArgs e) {
            lstMessages.Items.Clear();
        }

        public static void AddParserMessage(string message) {
            ACTWrapper.RunOnACTUIThread((System.Action)delegate {
                lstMessages.Items.Add(message);
            });
        }


    }
}