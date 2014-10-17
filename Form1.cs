/*
    WinRemoteControlServer.Console

    Copyright (C) 2014 Frederic Torres

    Permission is hereby granted, free of charge, to any person obtaining a copy of this software and 
    associated documentation files (the "Software"), to deal in the Software without restriction, 
    including without limitation the rights to use, copy, modify, merge, publish, distribute, 
    sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is 
    furnished to do so, subject to the following conditions:

    The above copyright notice and this permission notice shall be included in all copies or substantial 
    portions of the Software.

    THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT 
    LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. 
    IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, 
    WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE 
    OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WindowsInput;
using DotNetAutoInstaller;
using DynamicSugar;
using Utils;
using WinRemoteControl.Server.Console;
using WinRemoteControlConsole;
using WinRemoteControl.Server;


namespace WinRemoteControl.WinTestApp
{
    public partial class Form1 : Form
    {
        private WinRemoteControl.Server.RemoteControlServer _remoteControlServer;

        public Form1()
        {
            InitializeComponent();
            this.Icon = WinRemoteControl.Server.Console.Program.AppIcon;
        }

        private string TraceFileName
        {
            get { return Path.Combine(WebApiModule.MainModule.GetTempFolder(), "WinRemoteControl.Server.Console.Trace.log"); }
        }

        private void LogToFile(string m)
        {
            var mm = "[{0}]{1}".FormatString(DateTime.Now, m);
            System.IO.File.AppendAllText(this.TraceFileName, mm + Environment.NewLine);
        }

        private void Trace(string m)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => Trace(m)));
            }
            else
            {
                System.Diagnostics.Debug.WriteLine(m);
                if (this.TraceToUI)
                {
                    this.lbOut.Items.Add(m);
                    this.lbOut.SelectedIndex = this.lbOut.Items.Count - 1;
                    Application.DoEvents();
                }
                if (this.TraceToFile)
                {
                    LogToFile(m);
                }
            }
        }

        private void AssertIsTrue(bool e, string m)
        {
            Trace("{0} - {1}".FormatString(e ? "[passed]":"[failed]", m));
        }

        private void quitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _remoteControlServer.Stop();
            this.Close();
        }

        private int GetPort()
        {
            var args = Environment.GetCommandLineArgs().ToList();
            for (var i = 0; i < args.Count; i++)
            {
                if (args[i] == "-port")
                    return int.Parse(args[i+1]);
            }
            return RemoteControlServer.DEFAULT_PORT;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.TraceToUI = true;
            _remoteControlServer = new RemoteControlServer(this.GetPort());
            RemoteControlServer.TraceEvent += RemoteControlServer_TraceEvent;
            _remoteControlServer.Start();
            this.TraceToUI = false;

            this.Left = Screen.PrimaryScreen.Bounds.Width - this.Width - 3;
            this.Top  = Screen.PrimaryScreen.Bounds.Height - this.Height - 46;

            Form1_Resize(null, null);
        }

        void RemoteControlServer_TraceEvent(string message)
        {
            Trace(message);
        }

        private void clearToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.lbOut.Items.Clear();
        }

        public static List<CommandDefinition> CommandToExecute = new List<CommandDefinition>();

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (CommandToExecute.Count > 0)
            {
                var c = CommandToExecute[0];

                if (c.Action == CommandDefinitionActionType.sendKeys)
                {
                    this.sendKeys(c.Keys, c.Wait);
                }
                CommandToExecute.RemoveAt(0);
            }
        }

        /// <summary>
        /// http://msdn.microsoft.com/en-us/library/system.windows.forms.sendkeys.send(v=vs.110).aspx
        /// </summary>
        /// <param name="keys"></param>
        /// <param name="wait"></param>
        /// <returns></returns>
        private bool sendKeys(string keys, bool wait = false)
        {
            try
            {
                // Use the Windows sendkeys
                if (keys.Contains("{") || keys.Contains("%")|| keys.Contains("+")|| keys.Contains("~"))
                {
                    if (wait)
                        SendKeys.SendWait(keys);
                    else
                        SendKeys.Send(keys);
                }
                else
                {
                    // This is faster to send text
                    InputSimulator.SimulateTextEntry(keys);
                }
                return true;
            }
            catch
            {
                throw;
            }
        }

        private void lbOut_DoubleClick(object sender, EventArgs e)
        {
            var s = this.lbOut.SelectedItem.ToString();
            System.Windows.Forms.MessageBox.Show(s);
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            var margin = 4;
            this.lbOut.Left = margin;
            this.lbOut.Width = this.Width - (margin*4);
            this.lbOut.Top = margin;
            this.lbOut.Height = this.Height - (margin*4) - 16;
        }

        private void setRemoteDesktopSuppressWhenMinimizedToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var remoteDesktop_SuppressWhenMinimized_Reg = DynamicSugar.DS.Resources.SaveBinaryResourceAsFile(Assembly.GetExecutingAssembly(), Environment.GetEnvironmentVariable("TEMP"), "RemoteDesktop_SuppressWhenMinimized.reg");
            AutoInstaller.ExecuteProgram("regedit.exe", @"""{0}""".FormatString(remoteDesktop_SuppressWhenMinimized_Reg),  minimize:false, wait:true, useShellExecute: false);
        }


        private bool TraceToFile
        {
            get { return this.toFileToolStripMenuItem.Checked; }
            set { this.toFileToolStripMenuItem.Checked = value; }
        }

        private bool TraceToUI
        {
            get { return this.toUIToolStripMenuItem.Checked; }
            set { this.toUIToolStripMenuItem.Checked = value; }
        }

        //private void mnuTrace_Click(object sender, EventArgs e)
        //{
        //    this.mnuTrace.Checked = !this.mnuTrace.Checked;
        //}
       



        private void toUIToolStripMenuItem_Click(object sender, EventArgs e)
        {
            toUIToolStripMenuItem.Checked = !toUIToolStripMenuItem.Checked;
        }

        private void toFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            toFileToolStripMenuItem.Checked = !toFileToolStripMenuItem.Checked;
        }

        private void clearToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            this.lbOut.Items.Clear();
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            WinRemoteControl.Server.Console.Program.AboutBoxForm.ShowDialog();
        }
    }
}
