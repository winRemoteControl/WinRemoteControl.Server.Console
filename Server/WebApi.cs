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

using System.Diagnostics;
using System.IO;
using System.Reflection;
using Nancy;
using Nancy.Hosting.Self;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DynamicSugar;

namespace WinRemoteControl.Server
{
    public class WebApiModule : NancyModule
    {
        /// <summary>
        /// http://localhost:1964/WinRemoteControl?json={"Action":0,"Program":"notepad.exe","CommandLine":"" }
        /// http://192.168.1.5:1964/WinRemoteControl?json={"Action":0,"Program":"notepad.exe","CommandLine":"" }
        /// http://192.168.1.6:1964/WinRemoteControl?json={"Action":0,"Program":"notepad.exe","CommandLine":"","Parameters":null,"PID":0,"Keys":null,"Wait":false,"Minimize":false}
        /// http://localhost:1964/WinRemoteControl?json={"Action":4,"Program":null,"CommandLine":"Cls\r\necho BATCH DEMO\r\necho ----------\r\nCD C:\\Windows\\System32\r\nDir *.exe\r\nDir *.com\r\nDir *.cpl\r\nDir *.txt\r\nDir *.log\r\nDir *.xml\r\necho.\r\necho DONE","Parameters":null,"PID":0,"Keys":null,"Wait":true,"Minimize":false,"Full":false}
        /// </summary>
        public class MainModule : NancyModule
        {
            private PlugInServerApi _plugInServerApi;

            private const string ERR_001_MESSAGE = "WRC-ERR001:Internal error grabbing a screenshot";

            private static Response CreateNewNoContentReponse()
            {
                var response = new Response
                {
                    ContentType = "text/html",
                    StatusCode  =  HttpStatusCode.NoContent,
                    Contents    = (stream) =>
                    {
                        
                    }
                };
                return response;
            }

            private static Response CreateNewHttpErrorReponse(Nancy.HttpStatusCode statusCode, string errorMessage)
            {
                var response = new Response
                {
                    ContentType = "text/html",
                    StatusCode  = statusCode,
                    Contents    = (stream) =>
                    {
                        var buffer = Encoding.UTF8.GetBytes(errorMessage);
                        stream.Write(buffer, 0, buffer.Length);
                    }
                };
                return response;
            }

            private static Response CreateNewHttpImageReponse(byte[] buffer, string contentType = "image/jpeg")
            {
                var response = new Response()
                {
                    ContentType = contentType,
                    StatusCode  = Nancy.HttpStatusCode.OK,
                    Contents    = stream =>
                    {
                        using (var writer = new BinaryWriter(stream))
                        {
                            writer.Write(buffer);
                        }
                    }
                };
                return response;
            }

            public static string Base64Encode(string plainText) 
            {
                var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
                return System.Convert.ToBase64String(plainTextBytes);
            }

            public static string GetTempFolder()
            {
                var f = Path.Combine(Environment.GetEnvironmentVariable("TEMP"), "WinRemoteControl.Server.Console");
                if (!Directory.Exists(f))
                    Directory.CreateDirectory(f);
                return f;
            }

            public static string GetImageTempFolder()
            {
                var f = Path.Combine(GetTempFolder(), "Images");
                if (!Directory.Exists(f))
                    Directory.CreateDirectory(f);
                return f;
            }

            private string GetNoChangesImageFileName()
            {
                return DS.Resources.SaveBinaryResourceAsFile(Assembly.GetExecutingAssembly(), GetImageTempFolder(), "NoChange.jpg");
            }
            
            public void Initialize()
            {
                if (_plugInServerApi == null)
                    _plugInServerApi = new PlugInServerApi();
            }
            
            private CommandExecutionDefinition ProcessCommandDefinition(CommandDefinition c, string json)
            {
                try
                {
                    if (c == null)
                        throw new ArgumentException("Invalid json:{0}".FormatString(json));

                    var ecd = new CommandExecutionDefinition();

                    if (c.Action == CommandDefinitionActionType.sendKeys)
                    {
                        WinRemoteControl.WinTestApp.Form1.CommandToExecute.Add(c);
                        while (WinRemoteControl.WinTestApp.Form1.CommandToExecute.Count > 0) {
                            System.Threading.Thread.Sleep(30);
                        }
                    }
                    else
                    {
                        ecd = _plugInServerApi.Run(c);
                    }
                    return ecd;
                }
                catch (System.Exception ex)
                {
                    return new CommandExecutionDefinition(ex);
                }
            }

            public MainModule()
            {
                this.Initialize();
                
                Post["/WinRemoteControl"] = x =>
                {
                    string inputJson = Request.Form.Json;
                    RemoteControlServer.Trace("~Post /WinRemoteControl json={0}".FormatString(inputJson));
                    var r            = ProcessCommandDefinition(CommandDefinition.Deserialize<CommandDefinition>(inputJson), inputJson);
                    var jsonToReturn = r.Serialize();
                    RemoteControlServer.Trace("~Post returns json={0}".FormatString(jsonToReturn));
                    return jsonToReturn;
                };
                
                Get["/WinRemoteControl"] = x =>
                {
                    string json = Request.Query.Json;
                    RemoteControlServer.Trace("Get /WinRemoteControl json={0}".FormatString(json));
                    var r = ProcessCommandDefinition(CommandDefinition.Deserialize<CommandDefinition>(json), json);
                    var jsonToReturn = r.Serialize();
                    RemoteControlServer.Trace("~Post returns json={0}".FormatString(jsonToReturn));
                    return jsonToReturn;
                };

                /*
                 *  http://192.168.1.12:1964/GrabScreenShot?full=true       
                 *  http://localhost:1964/GrabScreenShot       
                 *  http://localhost:1964/GrabScreenShot?full=true
                 *  http://23.102.176.250:1964/GrabScreenShot?full=true
                 */
                Get["/GrabScreenShot"] = x =>
                {                    
                    bool full = Request.Query.Full == "true";
                    RemoteControlServer.Trace("Get /GrabScreenShot full:{0}".FormatString(full));                    
                    var execCmdDef = ProcessCommandDefinition(new CommandDefinition() { Action = CommandDefinitionActionType.grabScreenShot, Full = full }, null);
                    if (execCmdDef.Succeeded)
                    {
                        if (!full && execCmdDef.Bound.IsEmpty) // No Changes detected
                        {
                            return CreateNewNoContentReponse();
                        }

                        var pngFilename = full || !execCmdDef.Bound.IsEmpty ? execCmdDef.FileName : GetNoChangesImageFileName();
                        var buffer      = File.ReadAllBytes(pngFilename);
                        var response    = CreateNewHttpImageReponse(buffer);
                        var boundAsJson = execCmdDef.BoundAsJson();
                        RemoteControlServer.Trace("boundAsJson:{0}, OtherInfo:{1}".FormatString(boundAsJson, execCmdDef.OtherInfo));
                        response.Headers.Add("BoundInfo", boundAsJson);

                        if (!full && execCmdDef.Bound.IsEmpty)
                        {
                            execCmdDef.OtherInfo += " [INTERNAL ERROR]";
                        }

                        response.Headers.Add("OtherInfo", Base64Encode(execCmdDef.OtherInfo));                        
                        return response;
                    }
                    else
                    {
                        return CreateNewHttpErrorReponse(HttpStatusCode.InternalServerError, ERR_001_MESSAGE);
                    }
                };
            }
        }
    }
}
