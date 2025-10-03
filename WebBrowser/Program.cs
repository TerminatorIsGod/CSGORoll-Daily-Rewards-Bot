using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Compression;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Text.Json;
using System.Numerics;
using System.Configuration;
using WebBrowser.Config;

namespace WebBrowser
{
    internal static class Program
    {
        private static readonly string RepoOwner = "TerminatorIsGod";
        private static readonly string RepoName = "CSGORoll-Daily-Rewards-Bot";
        public static readonly string CurrentVersion = "Release_v2.2.1";
        private static string newestVersion = "";

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            //System.Threading.Thread.Sleep(15000);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            CaseIDManager cid = new CaseIDManager();
            ConfigManager cm = new ConfigManager();

            bool autoUpdate = cm.GetConfigFile().autoUpdateProgram;
            //autoUpdate = false;

            if (autoUpdate)
            {
                ClearOldFiles();

                if (IsNewVersionAvailableAsync().GetAwaiter().GetResult())
                {
                    Console.WriteLine("New version available...");
                    UpdateProgramAsync().GetAwaiter().GetResult();
                }
                else
                {
                    Console.WriteLine("Latest version...");
                }
            }

            Form1 form1 = new Form1();
            form1.WindowState = FormWindowState.Minimized;
            Application.Run(form1);
        }

        private static void ClearOldFiles()
        {
            string exeDirectory = Path.GetDirectoryName(Application.ExecutablePath);

            List<string> directories = new List<string>()
            {
                Path.Combine(exeDirectory, "runtimes-old")
            };

            List<string> files = new List<string>()
            {
                Path.Combine(exeDirectory, "CSGORollDailyCollector-old.exe"),
                Path.Combine(exeDirectory, "CSGORollDailyCollector.exe-old.config"),
                Path.Combine(exeDirectory, "How to.txt"),
                Path.Combine(exeDirectory, "If you move this program, you need to rerun it!-old.txt"),
                Path.Combine(exeDirectory, "readme-old.txt"),
            };

            foreach(string str in directories)
            {
                if (Directory.Exists(str))
                {
                    Directory.Delete(str, true);
                }
            }

            foreach(string str in files)
            {
                if (File.Exists(str))
                {
                    File.Delete(str);
                }
            }
        }

        private static async Task<bool> IsNewVersionAvailableAsync()
        {
            using (HttpClient client = new HttpClient())
            {
                string url = $"https://api.github.com/repos/{RepoOwner}/{RepoName}/releases/latest";
                client.DefaultRequestHeaders.UserAgent.ParseAdd("C# App");

                HttpResponseMessage response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var jsonResponse = await response.Content.ReadAsStringAsync();
                var releaseInfo = JsonDocument.Parse(jsonResponse).RootElement;
                string latestVersion = releaseInfo.GetProperty("tag_name").GetString();
                newestVersion = latestVersion;

                return latestVersion != CurrentVersion;
            }
        }

        

        private static async Task UpdateProgramAsync()
        {
            string downloadUrl = $"https://github.com/{RepoOwner}/{RepoName}/releases/latest/download/CSGORollDailyCollector_{newestVersion}.zip";
            string tempFilePath = Path.Combine(Path.GetTempPath(), $"CSGORollDailyCollector_{newestVersion}.zip");

            string exeDirectory = Path.GetDirectoryName(Application.ExecutablePath);

            Dictionary<string, string> directoryNames = new Dictionary<string, string>
            {
                { Path.Combine(exeDirectory, "runtimes"), Path.Combine(exeDirectory, "runtimes-old") }
            };

            Dictionary<string, string> fileNames = new Dictionary<string, string>
            {
                { Path.Combine(exeDirectory, "CSGORollDailyCollector.exe"), Path.Combine(exeDirectory, "CSGORollDailyCollector-old.exe") },
                { Path.Combine(exeDirectory, "CSGORollDailyCollector.exe.config"), Path.Combine(exeDirectory, "CSGORollDailyCollector.exe-old.config") },
                { Path.Combine(exeDirectory, "How to setup multiple accounts.txt"), Path.Combine(exeDirectory, "How to setup multiple accounts-old.txt") },
                { Path.Combine(exeDirectory, "How to setup proxy.txt"), Path.Combine(exeDirectory, "How to setup proxy-old.txt") },
                { Path.Combine(exeDirectory, "readme.txt"), Path.Combine(exeDirectory, "readme-old.txt") },
                { Path.Combine(exeDirectory, "If you move this program, you need to rerun it!.txt"), Path.Combine(exeDirectory, "If you move this program, you need to rerun it!-old.txt") }
            };

            Console.WriteLine("Downloading the latest version...");
            using (HttpClient client = new HttpClient())
            using (var response = await client.GetAsync(downloadUrl))
            {
                response.EnsureSuccessStatusCode();
                using (var fs = new FileStream(tempFilePath, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    await response.Content.CopyToAsync(fs);
                }
            }

            Console.WriteLine("Renaming current program...");

            foreach (string directory in directoryNames.Keys)
            {
                if (Directory.Exists(directoryNames[directory]))
                {
                    Directory.Delete(directoryNames[directory], true);
                }

                if (Directory.Exists(directory))
                {
                    Directory.Move(directory, directoryNames[directory]);
                }
                
            }

            foreach (string file in fileNames.Keys)
            {
                if (File.Exists(fileNames[file]))
                {
                    File.Delete(fileNames[file]);
                }

                if (File.Exists(file))
                {
                    File.Move(file, fileNames[file]);
                }

            }

            Console.WriteLine("Extracting new version...");
            //ZipFile.ExtractToDirectory(tempFilePath, exeDirectory);

            using (ZipArchive archive = ZipFile.OpenRead(tempFilePath))
            {
                foreach (ZipArchiveEntry entry in archive.Entries)
                {
                    string destinationPath = Path.Combine(exeDirectory, entry.FullName);

                    string directoryPath = Path.GetDirectoryName(destinationPath);
                    if (directoryPath != null)
                    {
                        Directory.CreateDirectory(directoryPath);
                    }

                    // Skip directory entries
                    if (string.IsNullOrEmpty(entry.Name))
                        continue;

                    // Delete file if it already exists
                    if (File.Exists(destinationPath))
                    {
                        File.Delete(destinationPath);
                    }

                    entry.ExtractToFile(destinationPath);
                }
            }

            //Console.WriteLine("Starting new version and closing the current instance...");
            Process.Start(Path.Combine(exeDirectory, "CSGORollDailyCollector.exe"));
            Environment.Exit(0);
        }
    }
}
