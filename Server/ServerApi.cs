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
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Threading;
using DynamicSugar;
using System.IO;
using WindowsInput;
using System.Windows.Forms;
using Utils;
using vScreen.lib;

namespace WinRemoteControl.Server
{
    public class PlugInServerApi
    {
        public PlugInServerApi()
        {
            
        }
        
        public CommandExecutionDefinition Run(CommandDefinition c)
        {
            var r = new CommandExecutionDefinition();

            switch (c.Action)
            {
                // wait() command is executed on the client side
                case CommandDefinitionActionType.executeProgramAsBatch:
                {
                    // We store a ConsoleExecutionInfo as JSON in the ReturnValue
                    r.ReturnValue = this.ExecuteProgramAsBatch(c.CommandLine, c.ShowWindow).Serialize();
                }
                break;

                case CommandDefinitionActionType.executeProgram :
                    // Return the PID as double or -1
                    r.ReturnValue = this.ExecuteProgram(c.Program, c.CommandLine, c.Minimize, c.Wait);   
                break;

                case CommandDefinitionActionType.killProgram :
                    // Return true of ok
                    r.ReturnValue = this.KillProgram(c.PID); 
                break;

                case CommandDefinitionActionType.sendKeys :
                    // Should never be called
                    throw new NotImplementedException("Should be implemented by the WinForm UI");
                    
                case CommandDefinitionActionType.grabScreenShot:

                    var captureInfo = this.GrabScreenShotSmartCapture(c.Full);
                    if (captureInfo.Succeeded)
                    {
                        r.FileName    = captureInfo.ImageFileName;
                        r.OtherInfo   = captureInfo.OtherInfo;
                        r.ReturnValue = true;
                        r.Bound.From(captureInfo.Bound);
                        if (!c.Full && r.Bound.IsEmpty)
                        {
                            Debug.WriteLine("no change");
                        }
                    }
                    else
                    {
                        r.Exception   = captureInfo.Exception.ToString();
                        r.ReturnValue = false;
                    }
                break;
            }
            return r;
        }

        public bool Wait(double second)
        {
            if(second < 1)
                Thread.Sleep(Convert.ToInt32(second*1000));
            else  
                Thread.Sleep(Convert.ToInt32(second)*1000);
            return true;
        }

        private const int MAX_WIDTH = 640;
        private static int GrabScreenShotSmartCaptureCounter = 0;

        public DesktopGrabber.SmartCaptureInfo GrabScreenShotSmartCapture(bool full)
        {
            try
            {
                //GrabScreenShotSmartCaptureCounter++;
                var fileName    = Path.Combine(Environment.GetEnvironmentVariable("TEMP"), @"desktop.{0}.jpg".FormatString(GrabScreenShotSmartCaptureCounter.ToString("000")));
                var captureInfo = DesktopGrabber.SmartCapture(fileName, MAX_WIDTH, full);
                return captureInfo;
            }
            catch (Exception ex)
            {
                var r = new DesktopGrabber.SmartCaptureInfo() { Exception = ex };
                Debug.WriteLine(ex.ToString());
                return r;
            }
        }
        /// <summary>
        /// Kill a process on the destination machine
        /// </summary>
        /// <param name="pid"></param>
        /// <returns></returns>
        public bool KillProgram(double pid)
        {
            foreach ( Process p in System.Diagnostics.Process.GetProcesses() )
            {
                if (p.Id == Convert.ToInt32(pid))
                {
                    try
                    {
                        p.Kill();
                        p.WaitForExit(); // possibly with a timeout
                    }
                    catch (Win32Exception)
                    {
                        return false; // process was terminating or can't be terminated - deal with it
                    }
                    catch (InvalidOperationException)
                    {
                        return true; // process has already exited - might be able to let this one go
                    }
                    catch
                    {
                        return false;
                    }
                    return true;
                }
            }
            return false;
        }
        /// <summary>
        /// Execute a program on the destination machine
        /// return the process id if ok (PID>=0;
        /// </summary>
        /// <param name="program"></param>
        /// <param name="commandLine"></param>
        /// <param name="minimize"></param>
        /// <param name="wait"></param>
        /// <returns></returns>
        public double ExecuteProgram(string program, string commandLine, Boolean minimize = false, bool wait = false)
        {
            var p = ExecuteProgram_WINDOWS(program, commandLine, minimize = false, wait = false);
            if (p == null)
                return -1;
            else
                return p.Id;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="program"></param>
        /// <param name="source"></param>
        /// <param name="minimize"></param>
        /// <param name="wait"></param>
        /// <returns></returns>
        public ConsoleExecutionInfo ExecuteProgramAsBatch(string source, bool show = true)
        {
            var r                    = new ConsoleExecutionInfo();
            var batchFileName        = Path.Combine(Environment.GetEnvironmentVariable("TEMP"), "WinRemoteControl.Server.Console.bat");

            var batchBuidler = new StringBuilder();
            batchBuidler.Append("@echo off").AppendLine();
            batchBuidler.Append(source).AppendLine();
            System.IO.File.WriteAllText(batchFileName, batchBuidler.ToString());

            var commandLine = @"/C ""{0}"" ".FormatString(batchFileName);

            r = System.ConsoleEx.ConsoleExecutor.Execute("cmd.exe", commandLine, show, false, 10*60 /* 10 minutes timeout out */);
            
            return r;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="program"></param>
        /// <param name="commandLine"></param>
        /// <param name="minimize"></param>
        /// <param name="wait"></param>
        /// <returns></returns>
        static Process ExecuteProgram_WINDOWS(string program, string commandLine, Boolean minimize = false, bool wait = false)
        {
            try
            {
                var path                            = System.IO.Path.GetDirectoryName(program);
                var processStartInfo                = new ProcessStartInfo(program, commandLine);
                processStartInfo.ErrorDialog        = false;
                processStartInfo.UseShellExecute    = true;
                processStartInfo.WorkingDirectory   = path;
                //processStartInfo.CreateNoWindow   = true;
                processStartInfo.WindowStyle        = minimize ? ProcessWindowStyle.Minimized : ProcessWindowStyle.Normal;
                var process                         = new Process();
                process.StartInfo                   = processStartInfo;
                bool processStarted                 = process.Start();
                if (wait)
                    process.WaitForExit();

                return process;

            }
            catch (Exception)
            {
                throw;
            }
            finally
            {

            }
        }

        public static ConsoleExecutionInfo ExecuteAsBatch(string batchCode, Boolean minimize = false) {

            var r                    = new ConsoleExecutionInfo();
            var program              = "cmd.exe";
            var batchFileName        = Path.Combine(Environment.GetEnvironmentVariable("TEMP"), "WinRemoteControl.Server.Console.bat");
            var batchOutputFileName  = Path.Combine(Environment.GetEnvironmentVariable("TEMP"), "WinRemoteControl.Server.Console.Output.bat");
            string commandLine       = @" /c ""{0}"" > ""{1}"" ".FormatString(batchFileName, batchOutputFileName);

            System.IO.File.WriteAllText(batchFileName, batchCode);
            
            var p = ExecuteProgram_WINDOWS(program, commandLine , minimize, wait: true);

            if (p.ExitCode == 0)
            {
                r.Output = File.ReadAllText(batchOutputFileName);
            }
            /*
            if (File.Exists(batchOutputFileName))
                File.Delete(batchOutputFileName);                
            if (File.Exists(batchFileName))
                File.Delete(batchFileName);                
            */
            return r;
        }

        private static ConsoleExecutionInfo ExecuteProgram_CONSOLE(string commandLine, bool show = true)
        {
            var e                     = new ConsoleExecutionInfo();
            StreamReader outputReader = null;
            StreamReader errorReader  = null;
            try
            {
                e.CommandLine                           = "cmd.exe {0}".FormatString(commandLine);
                var processStartInfo                    = new ProcessStartInfo("cmd.exe", commandLine);
                processStartInfo.ErrorDialog            = false;
                processStartInfo.UseShellExecute        = false;
                processStartInfo.RedirectStandardError  = true;
                processStartInfo.RedirectStandardInput  = false;
                processStartInfo.RedirectStandardOutput = true;
                processStartInfo.CreateNoWindow         = false;
                processStartInfo.WindowStyle            = ProcessWindowStyle.Hidden;
                processStartInfo.WorkingDirectory       = "";
                var process                             = new Process();
                process.StartInfo                       = processStartInfo;
                bool processStarted                     = process.Start();

                if (processStarted) 
                {
                    outputReader  = process.StandardOutput;
                    errorReader   = process.StandardError;
                    process.WaitForExit();
                    e.ExitCode    = process.ExitCode;
                    e.Output      = outputReader.ReadToEnd();
                    e.ErrorOutput = errorReader.ReadToEnd();
                }
            }
            catch (Exception ex) {
                e.ErrorOutput += Environment.NewLine + "{0}".FormatString(ex.ToString());
            }
            finally {
                if (outputReader != null)
                    outputReader.Close();
                if (errorReader != null)
                    errorReader.Close();

                //e.Output = e.Output.Replace("\n", "\r\n");
                //e.ErrorOutput = e.ErrorOutput.Replace("\n", "\r\n");
            }
            e.Time = Environment.TickCount - e.Time;
            return e;
        }
    }
}

