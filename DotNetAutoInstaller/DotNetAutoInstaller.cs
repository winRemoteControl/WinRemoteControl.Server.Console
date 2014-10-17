/*
    DotNetAutoInstaller
  
    Copyright (c) 2013 Frederic Torres

    MIT License:
    ============
        Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation 
        files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, 
        modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software 
        is furnished to do so, subject to the following conditions:

        The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

        THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES 
        OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE 
        LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR 
        IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

    
    DotNetAutoInstaller.cs is a C# class to create auto installable C# Console and Windows application.
    See Readme.MARKDOWN

*/
using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Collections;
using System.Reflection;
using System.Drawing;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.Win32;
using System.Security.Principal;
using System.Runtime.InteropServices.ComTypes;

namespace DotNetAutoInstaller
{
    public enum Locations
    {
        LocalFolder,
        ApplicationDataFolder,
        TempFolder,
        Undefined,
    };

    /// <summary>
    /// The class that drive the auto installation
    /// </summary>
    internal class AutoInstaller
    {
        private bool _firstExecution                = false;
        private List<string> _errors                = new List<string>();
        
        public static Locations AssemblyLocation    = Locations.ApplicationDataFolder;
        public static Locations DataLocation        = Locations.ApplicationDataFolder;
        public static string SubFolder              = null;

        private static string TempFolderName;
        
        public AutoInstaller(Locations allLocation, string tempFolderName = null) 
        {
            AssemblyLocation        = allLocation;
            DataLocation            = allLocation;

            if(tempFolderName != null)
                TempFolderName = tempFolderName;

            this.Init();
        }
        public AutoInstaller(Locations assemblyLocation, Locations dataLocation, string tempFolderName = null) 
        {
            AssemblyLocation        = assemblyLocation;
            DataLocation            = dataLocation;

            if(tempFolderName != null)
                TempFolderName = tempFolderName;

            this.Init();
        }
        private void Init()
        {
            this._firstExecution    = this.IsFirstExecution();
            AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(__AssemblyResolve);
        }
        private static bool CopyFile(string s, string d)
        {   
            if(System.IO.File.Exists(s))
            {
                try
                {
                    System.IO.File.Copy(s, d);
                    return true;
                }
                catch
                {
                }
            }
            return false;
        }
        private static bool DeleteFileIfExist(string p)
        {   
            if(System.IO.File.Exists(p))
            {
                try
                {
                    System.IO.File.Delete(p);
                }
                catch
                {
                    return false;
                }
            }
            return true;
        }
        private static string CreateDir(string p)
        {   
            if(!System.IO.Directory.Exists(p))
                System.IO.Directory.CreateDirectory(p);

            return p;
        }
        private static string AssemblyPath
        {
            get
            {
                var p = AutoInstaller.ExecutableFolder;
                switch(AssemblyLocation)
                {
                        
                    case Locations.ApplicationDataFolder: p = Path.Combine(AutoInstaller.ApplicationDataFolder, "Bin");   break;
                    case Locations.LocalFolder:           p = AutoInstaller.ExecutableFolder;                             break;
                    case Locations.TempFolder:            p = AutoInstaller.GetTempFolder();                              break;
                }
                return CreateDir(p);
            }
        }
        private static string GetTempFolder()
        {
            var tempFullName = Path.Combine(Environment.GetEnvironmentVariable("TEMP"), TempFolderName);
            //try
            //{
            //    if (Directory.Exists(tempFullName))
            //    {
            //        Directory.Delete(tempFullName, true);
            //    }
            //}
            //catch(System.Exception ex)
            //{
            //    Debug.WriteLine(ex.ToString());
            //}
            //try
            //{
            //    if (!Directory.Exists(tempFullName))
            //    {
            //        Directory.CreateDirectory(tempFullName);
            //    }
            //}
            //catch(System.Exception ex)
            //{
            //    Debug.WriteLine(ex.ToString());
            //}
            return tempFullName;
        }
        public static string ApplicationDataFolder
        {
            get 
            {
                var p = String.Empty;
                switch(AssemblyLocation)
                {
                    case Locations.ApplicationDataFolder: 
                        var appName = System.Windows.Forms.Application.ProductName;
                        p           = CreateDir(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), appName));
                        p           = CreateDir(Path.Combine(p, GetVersion()));
                    break;
                    case Locations.LocalFolder: 
                        p = AutoInstaller.ExecutableFolder; 
                    break;
                    case Locations.TempFolder:
                        p = AutoInstaller.GetTempFolder();  
                    break;
                }
                return CreateDir(p);
            }
        }
        static Assembly __AssemblyResolve(object sender, ResolveEventArgs args)
        {
            var assemblyname        = args.Name.Split(',')[0];
            var assemblyFileName    = Path.Combine(AutoInstaller.AssemblyPath, assemblyname + ".dll");
            var assembly            = Assembly.LoadFrom(assemblyFileName);
            return assembly;
        }
        public bool ErrorFound 
        {
            get
            {
                return this._errors.Count > 0;
            }
        }
        private void AddError(string message)
        {
            this._errors.Add(message);
            Util.MsgBoxError(message);
        }
        public static string ProgramFilesFolder
        {
            get 
            {
                return Environment.Is64BitOperatingSystem ? Environment.GetEnvironmentVariable("ProgramFiles(x86)") :
                                                            Environment.GetEnvironmentVariable("ProgramFiles");
            }
        }
        private static string GetVersion(string assemblyName)
        {
            if(System.IO.File.Exists(assemblyName))
            {
                var v   = AssemblyName.GetAssemblyName(assemblyName).Version;
                var vs  = "{0}.{1}.{2}-{3}.{4}".format(v.Major, v.Minor, v.MajorRevision, v.MinorRevision, v.Build);
                return vs;
            }
            return null;
        }
        private static string GetVersion()
        {
            var v  = Assembly.GetEntryAssembly().GetName().Version;
            var vs = "{0}.{1}.{2}-{3}.{4}".format(v.Major, v.Minor, v.MajorRevision, v.MinorRevision, v.Build);
            return vs;
        }    
        private static string ExecutableFolder
        {
            get 
            {
                return System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            }
        }
        private static string InstallInfoFile
        {
            get
            {
                var fileName = Path.Combine(AutoInstaller.ApplicationDataFolder, "DotNetAutoInstaller.json");
                return fileName;
            }
        }
        private bool IsFirstExecution()
        {
            return !System.IO.File.Exists(AutoInstaller.InstallInfoFile);
        }
        const string InstallInfoJsonTemplate = @"{{ 
            ""InstallTime"" : ""{0}"",
            ""Username"" : ""{1}"",
            ""Machine"" : ""{2}""
        }}";
        private void SaveInstallInfoFile()
        {
            System.IO.File.WriteAllText(AutoInstaller.InstallInfoFile, InstallInfoJsonTemplate.format(DateTime.UtcNow, Environment.UserName, Environment.MachineName));
        }

        // - - - - - API SECTION - - - - - - -

        public AutoInstaller SetTempFolderName(string tempFolderName)
        {
            TempFolderName = tempFolderName;
            return this;
        }

        public AutoInstaller CreateShortcutToDesktop(string appName = null)
        {
            if(this._firstExecution && !this.ErrorFound)
            {
                if(appName == null)
                {
                    appName = System.Windows.Forms.Application.ProductName;
                }
                var linkFile    = Path.GetFileNameWithoutExtension(Assembly.GetExecutingAssembly().Location) + ".lnk";
                IShellLink link = (IShellLink)new ShellLink();

                link.SetDescription(appName);
                link.SetWorkingDirectory(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location));
                link.SetPath(System.Reflection.Assembly.GetExecutingAssembly().Location);

                IPersistFile file   = (IPersistFile)link;
                string desktopPath  = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
                file.Save(Path.Combine(desktopPath, linkFile), false);
            }
            return this;
        }
        public AutoInstaller SetDataSubFolder(string subFolder)
        {
            if(this._firstExecution && !this.ErrorFound)
            {
                SubFolder = subFolder;
            }
            return this;
        }
        public AutoInstaller CopyToProgramFiles(string appName = null)
        {
            if(this._firstExecution && !this.ErrorFound)
            {
                if(appName == null)
                    appName = System.Windows.Forms.Application.ProductName;

                var curExe      = Assembly.GetExecutingAssembly().Location;
                var appFolder   = CreateDir(Path.Combine(AutoInstaller.ProgramFilesFolder, appName));
                var exeName     = Path.GetFileName(curExe);
                var fullExeName = Path.Combine(appFolder, exeName);
                
                // File already exist we are going to compare the version, we use the .net version
                if(System.IO.File.Exists(fullExeName))
                {
                    var fileAlreadyThereVersion = AutoInstaller.GetVersion(fullExeName);

                    if(string.Compare(fileAlreadyThereVersion, AutoInstaller.GetVersion())==-1)
                    {
                        this.RequireElevatedPrivileges();

                        if(AutoInstaller.DeleteFileIfExist(fullExeName))
                        {
                            // No error so we will copy the file, see below
                        }
                        else
                        {
                            this.AddError("Cannot delete file:{0}".format(fullExeName));
                        }
                    }
                    else
                    {
                        // Already installed do nothing. Plus when the app is copied and re started
                        // we will hit this else.
                        return this;
                    }
                }
                if(!this.ErrorFound)
                {
                    this.RequireElevatedPrivileges();

                    if(AutoInstaller.CopyFile(curExe, fullExeName))
                    {
                        this.StartProcessAndShutDown(fullExeName, "", 0);
                    }
                    else
                    {
                        this.AddError("Cannot copy file:'{0}' to '{1}'".format(curExe, fullExeName));
                    }
                }
            }
            return this;
        }
        public AutoInstaller DeployAssemblies(params string[] assemblyFilenames)
        {
            //if(this._firstExecution && !this.ErrorFound)
            if(!this.ErrorFound)
            {
                DS.Resources.SaveBinaryResourceAsFiles(Assembly.GetExecutingAssembly(), AutoInstaller.AssemblyPath, assemblyFilenames);
            }
            return this;
        }
        public AutoInstaller DeployFiles(Locations location, params string[] textFiles)
        {
           if(this._firstExecution && !this.ErrorFound)
            {
                string p = null;
                switch(location)
                {
                    case Locations.ApplicationDataFolder : p = AutoInstaller.ApplicationDataFolder; break;
                    case Locations.LocalFolder: p = AutoInstaller.ExecutableFolder; break;
                }
                if(SubFolder != null)
                {
                    p = CreateDir(Path.Combine(p, SubFolder));
                }
                DS.Resources.SaveBinaryResourceAsFiles(Assembly.GetExecutingAssembly(), p, textFiles);
            }
            return this;
        }
        public AutoInstaller DeployFiles(params string[] textFiles)
        {
            return DeployFiles(Locations.ApplicationDataFolder, textFiles);
        }
        public AutoInstaller RequireElevatedPrivileges()
        {
            if(this._firstExecution && !this.ErrorFound)
            {
                UACHelper.UacHelper.StartProcessWithElevatedPrivilegeIfNeeded();
            }
            return this;
        }
        public void Finish()
        {
            if(this._firstExecution && !this.ErrorFound)
            {
                this.SaveInstallInfoFile();              
            }
        }
        private void StartProcessAndShutDown(string program, string commandLine, int exitCode = 0)
        {
            var p = ExecuteProgram(program, commandLine, false, false);
            System.Environment.Exit(exitCode);
        }
        public AutoInstaller RebootOnFirstExecution(int exitCode = 0)
        {
            if(this._firstExecution && !this.ErrorFound)
            {
                this.Finish();
                var p = ExecuteProgram(Assembly.GetExecutingAssembly().Location, System.Environment.CommandLine, false, false);
                System.Environment.Exit(exitCode);
            }
            return this;
        }
        internal static Process ExecuteProgram(string program, string commandLine, Boolean minimize = false, bool wait = false, bool useShellExecute = true)
        {
            try
            {
                var path                            = System.IO.Path.GetDirectoryName(program);
                var processStartInfo                = new ProcessStartInfo(program, commandLine);
                processStartInfo.ErrorDialog        = false;
                processStartInfo.UseShellExecute    = useShellExecute;
                processStartInfo.WorkingDirectory   = path;
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
        }
    }


    #region Util
    internal static class Constants 
    {
        public const string Version = "0.1";
        public const string DotNetAutoInstaller = "Dot Net Auto Installer";
    }
    internal static class Util 
    {
        public static void MsgBox(string message) 
        {
            System.Windows.Forms.MessageBox.Show(message, Constants.DotNetAutoInstaller);
        }
        public static void MsgBoxError(System.Exception ex, string message = null)
        {
            if (message == null)
            {
                System.Windows.Forms.MessageBox.Show("Error:{0}\r\n{1}".format(ex.Message, ex.ToString()), Constants.DotNetAutoInstaller, System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
            }
            else 
            {
                System.Windows.Forms.MessageBox.Show("Error:{0}\r\n{1}".format(message, ex.Message, System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error));
            }
        }
        public static void MsgBoxError(string message)
        {
            System.Windows.Forms.MessageBox.Show("Error:{0}".format(message), Constants.DotNetAutoInstaller, System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);            
        }
        public static bool MsgBoxYesNo(string message)
        {
            return System.Windows.Forms.MessageBox.Show(message, Constants.DotNetAutoInstaller, System.Windows.Forms.MessageBoxButtons.YesNo, System.Windows.Forms.MessageBoxIcon.Question) == System.Windows.Forms.DialogResult.Yes;
        }
    }
    #endregion

    #region DynamicSugar
    /// <summary>
    /// Comes from the DynamicSugar.net library, to avoid any dependencies in DotNetAutoInstaller,
    /// we include this code.
    /// </summary>
    internal static class ExtensionMethods_Format
    {
        /// <summary>
        ///  Replaces the format item in the string with the string representation
        ///  of a corresponding object in a specified array.
        /// </summary>
        /// <param name="s"></param>
        /// <param name="args">An object array that contains zero or more objects to format</param>
        /// <returns>A copy of format in which the format items have been replaced by the string
        /// representation of the corresponding objects in args.
        /// </returns>
        internal static string format(this string s, params object[] args)
        {
            return string.Format(s, args);
        }
    }
    /// <summary>
    /// Dynamic Sharp Helper Class, dedicated methods to work with text resource file
    /// Comes from the DynamicSugar.net library, to avoid any dependencies in DotNetAutoInstaller,
    /// we include this code.
    /// </summary>
    internal static partial class DS
    {
        internal static class Resources
        {
            /// <summary>
            /// Return the fully qualified name of the resource file
            /// </summary>
            /// <param name="resourceFileName">File name of the resource</param>
            /// <returns></returns>
            private static string GetResourceFullName(string resourceFileName, Assembly assembly)
            {
                foreach (var resource in assembly.GetManifestResourceNames())
                    if (resource.EndsWith("." + resourceFileName))
                        return resource;
                throw new System.ApplicationException("Resource '{0}' not find in assembly '{1}'".format(resourceFileName, Assembly.GetExecutingAssembly().FullName));
            }
            /// <summary>
            /// Return the content of a text file embed as a resource.
            /// The function takes care of finding the fully qualify name, in the current
            /// assembly.
            /// </summary>
            /// <param name="resourceFileName">The file name of the resource</param>
            /// <returns></returns>
            internal static string GetTextResource(string resourceFileName, Assembly assembly)
            {
                var resourceFullName = GetResourceFullName(resourceFileName, assembly);

                using (var _textStreamReader = new StreamReader(assembly.GetManifestResourceStream(resourceFullName)))
                    return _textStreamReader.ReadToEnd();
            }
            /// <summary>
            /// Return the content of a file embed as a resource.
            /// The function takes care of finding the fully qualify name, in the current
            /// assembly.
            /// </summary>
            /// <param name="resourceFileName"></param>
            /// <param name="assembly"></param>
            /// <returns></returns>
            internal static byte[] GetBinaryResource(string resourceFileName, Assembly assembly)
            {
                var resourceFullName = GetResourceFullName(resourceFileName, assembly);
                var stream = assembly.GetManifestResourceStream(resourceFullName);
                byte[] data = new Byte[stream.Length];
                stream.Read(data, 0, (int)stream.Length);
                return data;
            }
#if !MONOTOUCH
            /// <summary>
            /// Return a image embed as a resource.
            /// The function takes care of finding the fully qualify name, in the current
            /// assembly.
            /// </summary>
            internal static Bitmap GetBitmapResource(string resourceFileName, Assembly assembly)
            {
                return ByteArrayToBitMap(GetBinaryResource(resourceFileName, assembly));
            }
            /// <summary>
            /// Convert a byte array into a bitmap
            /// </summary>
            /// <param name="b"></param>
            /// <returns></returns>
            private static Bitmap ByteArrayToBitMap(byte[] b)
            {
                MemoryStream ms = new MemoryStream(b);
                var img = System.Drawing.Image.FromStream(ms);
                return img as System.Drawing.Bitmap;
            }
#endif
            /// <summary>
            /// Save a buffer of byte into a file
            /// </summary>
            /// <param name="byteArray"></param>
            /// <param name="fileName"></param>
            /// <returns></returns>
            private static bool SaveByteArrayToFile(byte[] byteArray, string fileName)
            {
                try
                {
                    using (Stream fileStream = File.Create(fileName))
                    {
                        fileStream.Write(byteArray, 0, byteArray.Length);
                        fileStream.Close();
                        return true;
                    }
                }
                catch
                {
                    return false;
                }
            }
            /// <summary>
            /// Save resources as a local files
            /// </summary>
            /// <param name="assembly">Assembly where to get the resource</param>
            /// <param name="path">Local folder</param>
            /// <param name="resourceFileNames">Resource name and filename</param>
            /// <returns></returns>
            internal static Dictionary<string, string> SaveBinaryResourceAsFiles(Assembly assembly, string path, params string[] resourceFileNames)
            {
                var dic = new Dictionary<string, string>();

                foreach (var r in resourceFileNames)
                    dic[r] = SaveBinaryResourceAsFile(assembly, path, r);

                return dic;
            }
            /// <summary>
            /// Save a resource as a local file
            /// </summary>
            /// <param name="resourceFileName">Resource name and filename</param>
            /// <param name="assembly">Assembly where to get the resource</param>
            /// <param name="path">Local folder</param>
            /// <returns></returns>
            internal static string SaveBinaryResourceAsFile(Assembly assembly, string path, string resourceFileName)
            {
                var outputFileName = Path.Combine(path, resourceFileName);
                if (System.IO.File.Exists(outputFileName))
                    return outputFileName;

                if (!System.IO.Directory.Exists(path))
                    System.IO.Directory.CreateDirectory(path);

                var buffer = GetBinaryResource(resourceFileName, assembly);

                SaveByteArrayToFile(buffer, outputFileName);
                return outputFileName;
            }
        }
    }

    #endregion

    #region UACHelper
    namespace UACHelper {
    
            /// <summary>
        /// Comes from http://stackoverflow.com/questions/1220213/detect-if-running-with-elevated-privileges/1220234#1220234
        /// </summary>
        public static class UacHelper
        {
            private const string uacRegistryKey = "Software\\Microsoft\\Windows\\CurrentVersion\\Policies\\System";
            private const string uacRegistryValue = "EnableLUA";

            private static uint STANDARD_RIGHTS_READ = 0x00020000;
            private static uint TOKEN_QUERY = 0x0008;
            private static uint TOKEN_READ = (STANDARD_RIGHTS_READ | TOKEN_QUERY);

            [DllImport("advapi32.dll", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            static extern bool OpenProcessToken(IntPtr ProcessHandle, UInt32 DesiredAccess, out IntPtr TokenHandle);

            [DllImport("advapi32.dll", SetLastError = true)]
            public static extern bool GetTokenInformation(IntPtr TokenHandle, TOKEN_INFORMATION_CLASS TokenInformationClass, IntPtr TokenInformation, uint TokenInformationLength, out uint ReturnLength);

            public enum TOKEN_INFORMATION_CLASS
            {
                TokenUser = 1,
                TokenGroups,
                TokenPrivileges,
                TokenOwner,
                TokenPrimaryGroup,
                TokenDefaultDacl,
                TokenSource,
                TokenType,
                TokenImpersonationLevel,
                TokenStatistics,
                TokenRestrictedSids,
                TokenSessionId,
                TokenGroupsAndPrivileges,
                TokenSessionReference,
                TokenSandBoxInert,
                TokenAuditPolicy,
                TokenOrigin,
                TokenElevationType,
                TokenLinkedToken,
                TokenElevation,
                TokenHasRestrictions,
                TokenAccessInformation,
                TokenVirtualizationAllowed,
                TokenVirtualizationEnabled,
                TokenIntegrityLevel,
                TokenUIAccess,
                TokenMandatoryPolicy,
                TokenLogonSid,
                MaxTokenInfoClass
            }

            public enum TOKEN_ELEVATION_TYPE
            {
                TokenElevationTypeDefault = 1,
                TokenElevationTypeFull,
                TokenElevationTypeLimited
            }

            public static bool IsUacEnabled
            {
                get
                {
                    RegistryKey uacKey = Registry.LocalMachine.OpenSubKey(uacRegistryKey, false);
                    bool result = uacKey.GetValue(uacRegistryValue).Equals(1);
                    return result;
                }
            }

            public static bool IsRunningInsideVisualStudio()
            {
                var processName = System.IO.Path.GetFileNameWithoutExtension(Assembly.GetExecutingAssembly().Location);
                return System.Environment.CommandLine.Contains(processName+".vshost.exe");
            }

            public static void StartProcessWithElevatedPrivilegeIfNeeded()
            {
                if(UACHelper.UacHelper.IsUacEnabled && (!UACHelper.UacHelper.IsProcessElevated)) 
                {
                    StartProcessWithElevatedPrivilege();
                }
            }

            public static void StartProcessWithElevatedPrivilege()
            {
                var p               = new ProcessStartInfo();
                p.FileName          = Assembly.GetExecutingAssembly().Location;
                var parameters      = System.Environment.GetCommandLineArgs().ToList();
                parameters.RemoveAt(0);

                p.Arguments         = "";
                foreach(var a in parameters)
                {
                    p.Arguments += String.Format("\"{0}\" ", a);
                }

                p.UseShellExecute   = true;
                p.Verb              = "runas"; // Provides Run as Administrator
            
                if (Process.Start(p) != null)
                { 
                    Environment.Exit(0);
                }
                else
                {
                    Environment.Exit(1);
                }
            }

            public static bool IsProcessElevated
            {
                get
                {
                    if (IsUacEnabled)
                    {
                        IntPtr tokenHandle;
                        if (!OpenProcessToken(Process.GetCurrentProcess().Handle, TOKEN_READ, out tokenHandle))
                        {
                            throw new ApplicationException("Could not get process token.  Win32 Error Code: " + Marshal.GetLastWin32Error());
                        }

                        TOKEN_ELEVATION_TYPE elevationResult = TOKEN_ELEVATION_TYPE.TokenElevationTypeDefault;

                        int elevationResultSize = Marshal.SizeOf((int)elevationResult);
                        uint returnedSize = 0;
                        IntPtr elevationTypePtr = Marshal.AllocHGlobal(elevationResultSize);

                        bool success = GetTokenInformation(tokenHandle, TOKEN_INFORMATION_CLASS.TokenElevationType, elevationTypePtr, (uint)elevationResultSize, out returnedSize);
                        if (success)
                        {
                            elevationResult = (TOKEN_ELEVATION_TYPE)Marshal.ReadInt32(elevationTypePtr);
                            bool isProcessAdmin = elevationResult == TOKEN_ELEVATION_TYPE.TokenElevationTypeFull;
                            return isProcessAdmin;
                        }
                        else
                        {
                            throw new ApplicationException("Unable to determine the current elevation.");
                        }
                    }
                    else
                    {
                        WindowsIdentity identity = WindowsIdentity.GetCurrent();
                        WindowsPrincipal principal = new WindowsPrincipal(identity);
                        bool result = principal.IsInRole(WindowsBuiltInRole.Administrator);
                        return result;
                    }
                }
            }
        }
    }
    #endregion


#region Shortcut
    // http://stackoverflow.com/questions/4897655/create-shortcut-on-desktop-c-sharp
    [ComImport]
    [Guid("00021401-0000-0000-C000-000000000046")]
    internal class ShellLink
    {
    }

    [ComImport]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("000214F9-0000-0000-C000-000000000046")]
    internal interface IShellLink
    {
        void GetPath([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszFile, int cchMaxPath, out IntPtr pfd, int fFlags);
        void GetIDList(out IntPtr ppidl);
        void SetIDList(IntPtr pidl);
        void GetDescription([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszName, int cchMaxName);
        void SetDescription([MarshalAs(UnmanagedType.LPWStr)] string pszName);
        void GetWorkingDirectory([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszDir, int cchMaxPath);
        void SetWorkingDirectory([MarshalAs(UnmanagedType.LPWStr)] string pszDir);
        void GetArguments([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszArgs, int cchMaxPath);
        void SetArguments([MarshalAs(UnmanagedType.LPWStr)] string pszArgs);
        void GetHotkey(out short pwHotkey);
        void SetHotkey(short wHotkey);
        void GetShowCmd(out int piShowCmd);
        void SetShowCmd(int iShowCmd);
        void GetIconLocation([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszIconPath, int cchIconPath, out int piIcon);
        void SetIconLocation([MarshalAs(UnmanagedType.LPWStr)] string pszIconPath, int iIcon);
        void SetRelativePath([MarshalAs(UnmanagedType.LPWStr)] string pszPathRel, int dwReserved);
        void Resolve(IntPtr hwnd, int fFlags);
        void SetPath([MarshalAs(UnmanagedType.LPWStr)] string pszFile);
    }
#endregion
}



