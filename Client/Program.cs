using System.Threading;
using System.Windows.Forms;

namespace ClassiCubeUpdater {
    class Program {
        private static Thread statusThread;
        private static UpdaterDialog dialog;

        public static void Main(string[] args) {
            Application.EnableVisualStyles();
            statusThread = new Thread(new ThreadStart(UpdateStatus));
            statusThread.Start();
            Updater.Run();
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
