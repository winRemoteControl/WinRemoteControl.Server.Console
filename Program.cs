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
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using DotNetAutoInstaller;
using vScreen.lib;
using WinRemoteControl.WinTestApp;

namespace WinRemoteControl.Server.Console
{
    static class Program
    {
        public static Icon AppIcon;
        public static AboutBox AboutBoxForm;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            AboutBoxForm = new AboutBox();
            AppIcon = WinRemoteControl.Server.Console.Properties.Resources.winRemoteControlIcon;

            new AutoInstaller(Locations.TempFolder, Locations.TempFolder, @"WinRemoteControl.Server.Console")
            .DeployAssemblies(
                "DynamicSugar.dll",
                "InputSimulator.dll",
                "JsonObject.dll",
                "Nancy.dll",
                "Nancy.Hosting.Self.dll",
                "Newtonsoft.Json.dll",
                "WinRemoteControl.PlugInManager.dll"
            ).Finish();

            
            //var resultFileName = @"D:\DVT\ExaLife\WinRemoteControl\WinRemoteControl.Server.Console\Images\result.jpg";
            //var B1 = Bitmap.FromFile(@"D:\DVT\ExaLife\WinRemoteControl\WinRemoteControl.Server.Console\Images\dbg_desktop.001.jpg");
            //var B2 = Bitmap.FromFile(@"D:\DVT\ExaLife\WinRemoteControl\WinRemoteControl.Server.Console\Images\dbg_desktop.002.jpg");
            //DesktopGrabber.SmartCapture(resultFileName, 640, true, B1 as Bitmap);
            //var c = DesktopGrabber.SmartCapture(resultFileName, 640, false, B2 as Bitmap);
            //System.Environment.Exit(1);

            
            Application.Run(new Form1());
        }
    }
}
