using System;
using System.Threading;
using System.Windows.Forms;

namespace ClassiCubeUpdater {
    class Program {
        private static Thread statusThread;
        private static UpdaterDialog dialog;

        public static void Main(string[] args) {
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
