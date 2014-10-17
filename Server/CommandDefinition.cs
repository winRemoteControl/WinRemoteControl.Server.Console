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
using System.Drawing;
using Newtonsoft.Json;

namespace WinRemoteControl.Server
{
    public enum CommandDefinitionActionType 
    {
        executeProgram,
        killProgram,
        sendKeys,
        grabScreenShot,
        executeProgramAsBatch,
    };

    public class CommandExecutionDefinition : System.JSON.JSonObject
    {
        public string ErrorMessage;
        public string Exception;
        public object ReturnValue;
        public string FileName;
        public Utils.Bound Bound = new Utils.Bound();
        public string OtherInfo;

        public CommandExecutionDefinition()
        {
        }
        public CommandExecutionDefinition(System.Exception ex)
        {
            this.ErrorMessage = ex.Message;
            this.Exception    = ex.ToString();
            this.ReturnValue  = null;
        }
        public string BoundAsJson()
        {
            return JsonConvert.SerializeObject(this.Bound);
        }
        [JsonIgnore]
        public bool Succeeded
        {
            get
            {
                return this.ErrorMessage == null && this.Exception == null;
            }
        }
    }

    public class CommandDefinition : System.JSON.JSonObject
    {
        public CommandDefinitionActionType Action;
        public string Program;
        public string CommandLine;
        public string Parameters;
        public int PID;
        public string Keys;
        public bool Wait;
        public bool Minimize;
        public bool Full;
        public bool ShowWindow;
        public int TimeOut;
        
        public string ToJSON()
        {
            return this.Serialize(false);
        }
        public bool Run()
        {
            return false;
        }
    }
}