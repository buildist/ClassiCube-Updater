using System;
using System.IO;
using System.Net;
using System.Text;
using Ionic.Zip;
using System.Diagnostics;
using System.ComponentModel;

namespace ClassiCubeUpdater {
    static class Updater {
        static bool error = false;
        static Status status = Status.NONE;
        static string message = "Checking for update...";
        static float progress;
        static WebClient client = null;

        public static UpdaterStatus GetStatus() {
            return new UpdaterStatus(status, progress, message);
        }

        private static void Error(string message, Exception ex) {
            if (error)
                return;
            error = true;
            status = Status.ERROR;
            Updater.message = message;
            if (ex != null) {
                try {
                    FileStream stream = File.Open("error.log", FileMode.Create);
                    BinaryWriter w = new BinaryWriter(stream);
                    w.Write("ERROR: " + message + "\r\n");
                    w.Write(ex.ToString() + "\r\n");
                    w.Write(ex.StackTrace + "\r\n");
                    w.Close();
                } catch (Exception ex2) {
                }
            }
            Console.WriteLine(message);
        }

        private static void Error(string message) {
            Error(message, null);
        }

        public static void Run(String[] args) {
            try {
                bool shouldDownloadUpdate = false;
                bool installationProblem = false;
                bool networkProblem = false;
                int currentVersion = 0;
                // TODO use Config.VersionUurl
                string currentVersionStr = "1"; //HttpGet(Config.VersionUrl);

                try {
                    currentVersion = int.Parse(currentVersionStr);
                } catch (Exception ex) { networkProblem = true; }

                bool locked = CheckLock();
                if (!File.Exists(Config.VersionFilePath)) {
                    shouldDownloadUpdate = true;
                    installationProblem = true;
                } else if (!File.Exists(Config.GameLauncherPath)) {
                    shouldDownloadUpdate = true;
                    installationProblem = true;
                } else if (!networkProblem) {
                    int installedVersion = 0;
                    try {
                        BinaryReader r = new BinaryReader(File.Open(Config.VersionFilePath, FileMode.Open));
                        installedVersion = r.ReadInt32();
                        r.Close();
                    } catch (Exception ex) {
                        installationProblem = true;
                    }
                    if (installedVersion != currentVersion)
                        shouldDownloadUpdate = true;
                }
                if (installationProblem && networkProblem)
                    Error("Could not connect to the update server.");
                else if (!locked && shouldDownloadUpdate && !networkProblem) {
                    status = Status.IN_PROGRESS;

                    Update(delegate () {
                        if (!error) {
                            FileStream versionStream = File.Open(Config.VersionFilePath, FileMode.Create);
                            new BinaryWriter(versionStream).Write(currentVersion);
                            versionStream.Close();
                        }
                    }, args);
                } else
                    status = StartGame(args) ? Status.SUCCESS : Status.ERROR;
            } catch (Exception ex) {
                Error("Unexpected error occured", ex);
            }
        }

        private static bool StartGame(string[] args) {
            try {
                ProcessStartInfo info = new ProcessStartInfo();
                info.FileName = args != null ? Config.GameExecutablePath : Config.GameLauncherPath;
                info.WorkingDirectory = Config.GamePath;
                info.Arguments = args != null ? string.Join(" ", args) : "";
                Process.Start(info);
                return true;
            } catch (Exception ex) {
                Error("Could not start the game", ex);
                return false;
            }
        }

        delegate void OnSuccessCallback();

        private static void Update(OnSuccessCallback onSuccess, String[] args) {
            try {
                string updateFilePath = Config.GamePath + "\\update.zip";

                client = new WebClient();
                client.DownloadProgressChanged += new DownloadProgressChangedEventHandler(delegate (object sender, DownloadProgressChangedEventArgs e) {
                    progress = (float)e.ProgressPercentage / 100;
                });
                client.DownloadFileCompleted += new AsyncCompletedEventHandler(delegate (object sender, AsyncCompletedEventArgs e) {
                    if (e.Error != null) {
                        Error("Could not download update", e.Error);
                        return;
                    }

                    try {
                        client.Dispose();
                        client = null;
                        ZipFile zip = new ZipFile(updateFilePath);
                        zip.ExtractAll(Config.GamePath, ExtractExistingFileAction.OverwriteSilently);
                        zip.Dispose();
                        File.Delete(updateFilePath);
                        onSuccess();
                    } catch (Exception ex) {
                        Error("Could not install update", ex);
                    }
                    status = StartGame(args) ? Status.SUCCESS : Status.ERROR;
                });
                client.DownloadFileAsync(new Uri(Config.UpdateUrl), updateFilePath);
            } catch (Exception ex) {
                Error("Could not download update", ex);
            }
        }

        private static string HttpGet(string path) {
            try {
                Uri uri = new Uri(path);
                HttpWebRequest r = (HttpWebRequest)WebRequest.Create(uri);
                r.Credentials = CredentialCache.DefaultCredentials;
                r.Timeout = 5000;
                HttpWebResponse response = (HttpWebResponse)r.GetResponse();
                StreamReader reader = new StreamReader(response.GetResponseStream());
                string result = reader.ReadToEnd().Trim();
                reader.Close();
                response.Close();
                return result;
            } catch (Exception ex) {
                Console.WriteLine(ex.StackTrace);
                return "";
            }
        }

        private static string HttpPost(string path, string arguments) {
            try {
                Uri uri = new Uri(path);
                HttpWebRequest r = (HttpWebRequest)WebRequest.Create(uri);
                r.Method = "POST";
                r.Credentials = CredentialCache.DefaultCredentials;
                r.Timeout = 5000;
                ASCIIEncoding encoding = new ASCIIEncoding();
                byte[] bytes = encoding.GetBytes(arguments);
                Stream request = r.GetRequestStream();
                r.ContentLength = bytes.Length;
                request.Write(bytes, 0, bytes.Length);
                request.Close();
                HttpWebResponse response = (HttpWebResponse)r.GetResponse();
                StreamReader reader = new StreamReader(response.GetResponseStream());
                string result = reader.ReadToEnd().Trim();
                reader.Close();
                response.Close();
                return result;
            } catch (Exception ex) {
                Console.WriteLine(ex.StackTrace);
                return "";
            }
        }

        private static bool CheckLock() {
            if (!File.Exists(Config.GamePath + "\\lock"))
                return false;
            else {
                try {
                    File.Delete(Config.GamePath + "\\lock");
                    return false;
                } catch (Exception ex) {
                    return true;
                }
            }
        }
    }
}
