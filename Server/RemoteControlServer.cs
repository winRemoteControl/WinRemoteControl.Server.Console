﻿using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.InteropServices;
using Nancy;
using Nancy.Hosting.Self;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DynamicSugar;
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
namespace WinRemoteControl.Server
{
    public class RemoteControlServer
    {
        public const int DEFAULT_PORT = 1964;
        public int Port = DEFAULT_PORT;

        private string URL
        {
            get
            {
                return "http://localhost:{0}/".FormatString(this.Port);
            }
        }

        public RemoteControlServer(int port)
        {
            this.Port = port;
        }

        NancyHost _host;
        public delegate void TraceDelegate(string message);
        public static event TraceDelegate TraceEvent;

        public static string Trace(string m)
        {
            if(TraceEvent != null)
                TraceEvent(m);
            Debug.WriteLine(m);
            return m;
        }

        public void Start()
        {
            this._host = new NancyHost(new Uri(URL));
            this._host.Start();
            Trace("Listening at {0}".FormatString(this.URL));
            Trace("Machine Ip {0}".FormatString(this.GetLocalIpAddress()));
        }
        public void Stop()
        {
            Trace("Server Stopped");
        }
        public string GetLocalIpAddress()
        {
            IPHostEntry host;
            string localIP = "";
            host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (IPAddress ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    localIP = ip.ToString();
                    break;
                }
            }
            return localIP;
        }
    }
}
