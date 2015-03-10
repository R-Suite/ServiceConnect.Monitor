using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.ServiceProcess;
using System.Threading.Tasks;

namespace R.MessageBus.Monitor.Installer
{
    [RunInstaller(true)]
    public partial class Installer : System.Configuration.Install.Installer
    {

        public Installer() 
        {
            InitializeComponent();
        }

        public override void Install(IDictionary mySavedState) 
        {
            InitializeComponent();

            Trace.WriteLine("R.MessageBus.Monitor: Entering Install.");

            // Update config
            var config = File.ReadAllText(Path.GetDirectoryName(Context.Parameters["assemblypath"]) + @"\R.MessageBus.Monitor.exe.config");
            config = config.Replace("localhost", Environment.MachineName);
            File.WriteAllText(Path.GetDirectoryName(Context.Parameters["assemblypath"]) + @"\R.MessageBus.Monitor.exe.config", config);

            try
            {
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo("cmd.exe", "/c sc delete R.MessageBus.Monitor")
                    {
                        CreateNoWindow = true
                    }
                };

                process.Start();
                process.WaitForExit();

                process = new Process
                {
                    StartInfo = new ProcessStartInfo("cmd.exe", string.Format("/c sc create R.MessageBus.Monitor binpath= \"{0}\" start= auto", Path.GetDirectoryName(Context.Parameters["assemblypath"]) + @"\R.MessageBus.Monitor.exe"))
                    {
                        CreateNoWindow = true
                    }
                };

                process.Start();
                process.WaitForExit();

                process = new Process
                {
                    StartInfo = new ProcessStartInfo("cmd.exe", "/c net start R.MessageBus.Monitor")
                    {
                        CreateNoWindow = true
                    }
                };

                process.Start();
                process.WaitForExit();
            }
            catch (Exception ex)
            {
                Trace.WriteLine("Error creating windows service", ex.StackTrace);
            }

            Process.Start(string.Format("http://{0}:{1}", Environment.MachineName, 8100));
        }
    }
}
