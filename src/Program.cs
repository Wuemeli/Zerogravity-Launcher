using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Reflection;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace Project_ZeroGravity_Loader
{
    internal static class Program
    {
        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        const int SW_HIDE = 0;
        const int SW_SHOW = 5;


        [STAThread]
        static async Task Main()
        {

            var handle = GetConsoleWindow();

            ShowWindow(handle, SW_HIDE);

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            //Use Squirrel.Windows with Github
            string updateUrl = "https://autoupdate.zerogravity.rip/version.txt";
            string downloadUrl = "https://autoupdate.zerogravity.rip/update.exe";

            try
            {
                string latestVersion = await GetLatestVersion(updateUrl);
                Log("Latest version from server: " + latestVersion);

                if (IsUpdateAvailable(latestVersion))
                {
                    Log("Update available. Starting download.");
                    await DownloadAndApplyUpdate(downloadUrl);
                }
                else
                {
                    Log("No update available. Launching application.");
                    Application.Run(new Login());
                }
            }
            catch (Exception ex)
            {
                Log("Error: " + ex.Message);
                MessageBox.Show("An error occurred: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        static async Task<string> GetLatestVersion(string url)
        {
            using (HttpClient client = new HttpClient())
            {
                return await client.GetStringAsync(url);
            }
        }

        static bool IsUpdateAvailable(string latestVersion)
        {
            string currentVersion = "2.1.3";
            Log("Current version: " + currentVersion);

            return string.Compare(currentVersion, latestVersion, StringComparison.OrdinalIgnoreCase) < 0;
        }

        static async Task DownloadAndApplyUpdate(string downloadUrl)
        {
            string tempFilePath = "update_new.exe";
            string newFilePath = "zerogravity.exe";
            string oldFilePath = "old.exe";
            string currentFilePath = Process.GetCurrentProcess().MainModule.FileName;

            try
            {
                if (File.Exists(oldFilePath))
                    File.Delete(oldFilePath);

                if (File.Exists(newFilePath))
                    File.Delete(newFilePath);

                Log("Downloading update...");

                using (HttpClient client = new HttpClient())
                {
                    byte[] fileBytes = await client.GetByteArrayAsync(downloadUrl);
                    File.WriteAllBytes(tempFilePath, fileBytes);
                }

                Log("Update downloaded successfully.");

                Process.Start(new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = $"/C timeout /t 3 && move \"{currentFilePath}\" \"{oldFilePath}\" && move \"{tempFilePath}\" \"{newFilePath}\" && start \"{newFilePath}\"",
                    UseShellExecute = true,
                    CreateNoWindow = true
                });

                Log("Update process started. Exiting application.");
                Environment.Exit(0);
            }
            catch (Exception ex)
            {
                Log("Update failed: " + ex.Message);
                MessageBox.Show("An error occurred during the update: " + ex.Message, "Update Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        static void Log(string message)
        {
            string logFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "update.log");
            File.AppendAllText(logFilePath, $"{DateTime.Now}: {message}{Environment.NewLine}");
        }
    }
}