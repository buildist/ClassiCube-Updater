using Microsoft.Win32;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Security.Principal;
using System.Threading;
using System.Windows.Forms;

namespace ClassiCubeUpdater {
    class Program {
        const string protocolName = "mc";

        private static Thread statusThread;
        private static UpdaterDialog dialog;

        public static void Main(string[] args) {
            bool isRestarting = RegisterProtocolHandler();
            if (isRestarting)
                return;

            string[] gameArguments;
            if (args.Length == 1) {
                 gameArguments = ParseUrl(args[0]);
            } else {
                gameArguments = null;
            }

            Application.EnableVisualStyles();
            statusThread = new Thread(new ThreadStart(UpdateStatus));
            statusThread.Start();
            Updater.Run(gameArguments);
        }

        private static string[] ParseUrl(string url) {
            try {
                string[] parts = url.Split(new string[] { "://" }, StringSplitOptions.None)[1].Split('/');
                string address = parts[0];
                string user = parts[1];
                string mppass = parts[2];
                string[] ipAndPort = address.Split(':');
                return new string[] { user, mppass, ipAndPort[0], ipAndPort[1] };
            } catch (Exception ex) {
                return null;
            }
        }

        private static bool RegisterProtocolHandler() {
            try {
                RegistryKey key = Registry.ClassesRoot.OpenSubKey(protocolName);

                if (key == null) {
                    WindowsPrincipal pricipal = new WindowsPrincipal(WindowsIdentity.GetCurrent());
                    bool hasAdminRights = pricipal.IsInRole(WindowsBuiltInRole.Administrator);
                    String programPath = Assembly.GetExecutingAssembly().Location;

                    if (!hasAdminRights) {
                        return RequestAdminRights(programPath);
                    }

                    key = Registry.ClassesRoot.CreateSubKey(protocolName);
                    key.SetValue(null, "ClassiCube");
                    key.SetValue("URL Protocol", String.Empty);

                    Registry.ClassesRoot.CreateSubKey(protocolName + "\\Shell");
                    Registry.ClassesRoot.CreateSubKey(protocolName + "\\Shell\\open");

                    RegistryKey commandKey = Registry.ClassesRoot.CreateSubKey(protocolName + "\\Shell\\open\\command");
                    commandKey.SetValue(null, "\"" + programPath + "\" %1");
                }
            } catch(Exception ex) {
                Console.WriteLine("Failed to install protocol handler");
                Console.WriteLine(ex);
            }

            return false;
        }

        private static bool RequestAdminRights(String programPath) {
            ProcessStartInfo processInfo = new ProcessStartInfo();
            processInfo.Verb = "runas";
            processInfo.FileName = programPath;

            try {
                Process.Start(processInfo);
                return true;
            } catch (Win32Exception) {
                return false;
            }
        }


        private static void UpdateStatus() {
            while (true) {
                UpdaterStatus status = Updater.GetStatus();
                switch (status.Status) {
                    case Status.ERROR:
                        MessageBox.Show("An error occured while starting the game. Details on the problem have been saved to error.log.");
                        return;
                    case Status.IN_PROGRESS:
                        if (dialog == null) {
                            dialog = new UpdaterDialog();
                            dialog.Show();
                        }
                        dialog.progress.Value = (int)(100 * status.Progress);
                        break;
                    case Status.SUCCESS:
                        if (dialog != null)
                            dialog.Hide();
                        return;
                }
                Thread.Sleep(100);
            }
        }
    }
}
