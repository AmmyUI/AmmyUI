using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Reflection;
using Ammy.Build;
using Ammy.VisualStudio.Service.Extensions;
using Ammy.VisualStudio.Service.Settings;
using Microsoft.Win32;
using Process = System.Diagnostics.Process;

namespace Ammy.VisualStudio.Service.Compilation
{
    public class AdbService : INeedLogging
    {
        private const string AdbExe = "adb.exe";

        public bool SetupAdbForwarding()
        {
            const int port = RuntimeUpdateSender.SendPort;

            var executablePath = FindAdbExecutablePath();

            if (executablePath == null)
                return false;

            //var wasAdbRunning = Process.GetProcesses().FirstOrDefault(p => p.ProcessName == "adb") != null;
            
            RunAdb(executablePath, $"forward tcp:{port} tcp:{port}");

            // Kill adb if we spawned it
            //if (!wasAdbRunning)
            //    RunAdb(executablePath, "kill-server");

            return true;
        }

        private void RunAdb(string directoryPath, string arguments)
        {
            var process = new Process {
                StartInfo = {
                    FileName = directoryPath,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    Arguments = arguments
                }
            };

            process.Start();
            process.WaitForExit(2000);
        }

        private string FindAdbExecutablePath()
        {
            return FromSettings()
                ?? FromEnvironmentalVariable()
                ?? FromRegistry()
                ?? FromAppData();
        }

        private string FromSettings()
        {
            var path = AmmySettings.AdbPath;

            if (!string.IsNullOrEmpty(path) && File.Exists(path))
                return path;

            return null;
        }

        private string FromAppData()
        {
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var sdkToolsPath = Path.Combine(appDataPath, @"Local\Android\sdk\platform-tools");
            
            return GetExecutablePath(sdkToolsPath);
        }

        private string FromEnvironmentalVariable()
        {
            var androidHome = Environment.GetEnvironmentVariable("ANDROID_HOME");
            if (androidHome == null)
                return null;
            
            return GetExecutablePath(androidHome);
        }

        private string FromRegistry()
        {
            try {
                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\WOW6432Node\Android SDK Tools")) {
                    var path = key?.GetValue("Path") as string;
                    return GetExecutablePath(path);
                }
            } catch (Exception ex) {
                this.LogDebugException("Unable to retrieve Android SDK Path from registry", ex);
                return null;
            }
        }
        
        private static string GetExecutablePath(string directoryPath)
        {
            if (string.IsNullOrWhiteSpace(directoryPath))
                return null;

            var executablePath = Path.Combine(directoryPath, AdbExe);

            return File.Exists(executablePath) ? executablePath : null;
        }
    }
}