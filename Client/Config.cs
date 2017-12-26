using System;
using System.IO;

namespace ClassiCubeUpdater {
    class Config {
        /// <summary>
        /// URL of file containing latest version number.
        /// </summary>
        public const string UpdateUrl = "https://github.com/UnknownShadow200/ClassicalSharp/releases/download/0.99.9.8/release.v09998.d3d9.zip";

        /// <summary>
        /// URL of .zip containing latest version.
        /// </summary>
        public const string VersionUrl = "";

        /// <summary>
        /// Full path to directory where version.txt and game files are stored.
        /// </summary>
        public static readonly string GamePath = GetGameDirectory();

        /// <summary>
        /// Full path to file containing the currently installed version number.
        /// </summary>
        public static readonly String VersionFilePath = GamePath + "\\version.txt";

        /// <summary>
        /// Full path to program to execute to start game launcher.
        /// </summary>
        public static readonly string GameLauncherPath = GamePath + "\\Launcher.exe";

        /// <summary>
        /// Full path to program to execute to start game directly when arguments are provided.
        /// </summary>
        public static readonly string GameExecutablePath = GamePath + "\\ClassicalSharp.exe";

        private static string GetGameDirectory() {
            string path = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ClassiCube");
            if (!Directory.Exists(path)) {
                Directory.CreateDirectory(path);
            }
            return path;
        }
    }
}
