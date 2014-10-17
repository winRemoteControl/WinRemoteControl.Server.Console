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
using System.Threading;
using Utils;


namespace System.ConsoleEx
{
    public class ConsoleExecutor {
        
        public static ConsoleExecutionInfo Execute(string program, string commandLine, bool show, bool minimize, int timeout)
        {
            var r = new ConsoleExecutionInfo()
            {
                CommandLine = string.Format("{0} {1}", program, commandLine),
                Time        = Environment.TickCount
            };

            var output = new StringBuilder();
            var error  = new StringBuilder();
            
            using (var outputWaitHandle = new AutoResetEvent(false))
            using (var errorWaitHandle = new AutoResetEvent(false))
            using (Process process = new Process())
            {
                process.StartInfo.FileName               = program;
                process.StartInfo.Arguments              = commandLine;
                process.StartInfo.UseShellExecute        = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError  = true;
                process.StartInfo.CreateNoWindow         = !show;
                process.StartInfo.WorkingDirectory       = "";

                if (show)
                {
                    process.StartInfo.WindowStyle = minimize ? ProcessWindowStyle.Minimized : ProcessWindowStyle.Normal;
                }
                else
                {
                    process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                }
                
                process.OutputDataReceived += (sender, e) => {
                    if (e.Data == null)
                    {
                        outputWaitHandle.Set();
                    }
                    else
                    {
                        output.AppendLine(e.Data);
                    }
                };
                process.ErrorDataReceived += (sender, e) =>
                {
                    if (e.Data == null)
                    {
                        errorWaitHandle.Set();
                    }
                    else
                    {
                        error.AppendLine(e.Data);
                    }
                };

                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                if (process.WaitForExit(timeout*1000) && outputWaitHandle.WaitOne(timeout*1000) && errorWaitHandle.WaitOne(timeout*1000))
                {
                    r.Output      = output.ToString();
                    r.ErrorOutput = error.ToString();
                    r.ExitCode    = process.ExitCode;
                }
                else
                {
                    r.ExitCode = -1;
                    r.ErrorOutput = string.Format("TimeOut:{0}", r.CommandLine);
                }
            }
            r.Time = Environment.TickCount - r.Time;
            return r;
        }
    }
}

