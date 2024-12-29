using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using SharpCompress.Archives;
using SharpCompress.Archives.Rar;
using SharpCompress.Common;

namespace Project_ZeroGravity_Loader
{
    public partial class Loader : Form
    {
        private System.Windows.Forms.Timer fortniteCheckTimer;
        [DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
        private static extern IntPtr CreateRoundRectRgn
        (
            int nLeftRect,     // x-coordinate of upper-left corner
            int nTopRect,      // y-coordinate of upper-left corner
            int nRightRect,    // x-coordinate of lower-right corner
            int nBottomRect,   // y-coordinate of lower-right corner
            int nWidthEllipse, // width of ellipse
            int nHeightEllipse // height of ellipse
        );

        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        const int SW_HIDE = 0;
        const int SW_SHOW = 5;

        string appdata = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "ZeroGravity");

        IntPtr hWnd = GetConsoleWindow();

        public Loader()
        {
            InitializeComponent();
            InitializeFortniteCheckTimer();
            loadpath();
            this.FormBorderStyle = FormBorderStyle.None;
            Region = System.Drawing.Region.FromHrgn(CreateRoundRectRgn(0, 0, Width, Height, 15, 15));
        }

        private void InitializeFortniteCheckTimer()
        {
            fortniteCheckTimer = new System.Windows.Forms.Timer();
            fortniteCheckTimer.Interval = 1000;
            fortniteCheckTimer.Tick += CheckFortniteRunning;
            fortniteCheckTimer.Start();
        }

        private void CheckFortniteRunning(object sender, EventArgs e)
        {
            bool isFortniteRunning = Process.GetProcessesByName("FortniteClient-Win64-Shipping").Length > 0;

            if (isFortniteRunning)
            {
                guna2GradientButton4.Text = "Fortnite is running";
                guna2GradientButton4.Enabled = false;
                guna2GradientButton4.FillColor = Color.Gray;
            }
            else
            {
                guna2GradientButton4.Text = "Launch Game";
                guna2GradientButton4.Enabled = true;
                guna2GradientButton4.FillColor = Color.FromArgb(24, 24, 24);
            }
        }
        private void loadpath()
        {
            string appdata = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\ZeroGravity";

            if (File.Exists(Path.Combine(appdata, "path.txt")))
            {
                guna2TextBox1.Text = File.ReadAllText(Path.Combine(appdata, "path.txt"));
            }
        }

        public static void DownloadFile(string Url, string Path) => new WebClient().DownloadFile(Url, Path);

        public static async void StartGame(string gamePath)
        {

            string appdata = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\ZeroGravity";

            //were using data.txt now
            string emailPath = Path.Combine(appdata, "email.txt");
            string passwordPath = Path.Combine(appdata, "password.txt");

            string email = File.ReadAllText(emailPath);
            string password = File.ReadAllText(passwordPath);

            if (!File.Exists(appdata + "\\FortniteLauncher.exe"))
                DownloadFile("https://backend.zerogravity.rip/assets/FortniteLauncher.txt", appdata + "\\FortniteLauncher.exe");

            File.Delete(gamePath + "\\Engine\\Binaries\\ThirdParty\\NVIDIA\\NVaftermath\\Win64\\GFSDK_Aftermath_Lib.x64.dll");

            if (File.Exists(gamePath + "\\EAC.zip"))
                File.Delete(gamePath + "\\EAC.zip");

            if (File.Exists(gamePath + "\\Engine\\Binaries\\ThirdParty\\NVIDIA\\NVaftermath\\Win64\\GFSDK_Aftermath_Lib.x64.dll"))
                File.Delete(gamePath + "\\Engine\\Binaries\\ThirdParty\\NVIDIA\\NVaftermath\\Win64\\GFSDK_Aftermath_Lib.x64.dll");

            if (!File.Exists(gamePath + "\\Engine\\Binaries\\ThirdParty\\NVIDIA\\NVaftermath\\Win64\\GFSDK_Aftermath_Lib.x64.dll"))
                DownloadFile("https://backend.zerogravity.rip/assets/redirect.txt", gamePath + "\\Engine\\Binaries\\ThirdParty\\NVIDIA\\NVaftermath\\Win64\\GFSDK_Aftermath_Lib.x64.dll");

            if (!File.Exists(gamePath + "\\EAC.zip"))
                DownloadFile("https://backend.zerogravity.rip/assets/EAC.txt", gamePath + "\\EAC.zip");

            File.Delete(gamePath + "\\EAC.exe");

            if (Directory.Exists(gamePath + "\\EasyAntiCheat"))
            {
                Directory.Delete(gamePath + "\\EasyAntiCheat", true);
                Console.WriteLine("Folder EasyAntiCheat deleted successfully.");
            }

            ZipFile.ExtractToDirectory(gamePath + "\\EAC.zip", gamePath);

            File.Delete(gamePath + "\\EAC.zip");

            Process launcher = new Process();
            launcher.StartInfo.FileName = appdata + "\\FortniteLauncher.exe";
            launcher.StartInfo.CreateNoWindow = true;
            launcher.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;

            Process shipping = new Process();
            shipping.StartInfo.FileName = gamePath + "\\EAC.exe";
            shipping.StartInfo.Arguments =
                $"-epicapp=Fortnite -epicenv=Prod -epiclocale=en-us -epicportal -skippatchcheck -nobe -fromfl=eac -fltoken=3db3ba5dcbd2e16703f3978d -caldera=eyJhbGciOiJFUzI1NiIsInR5cCI6IkpXVCJ9.eyJhY2NvdW50X2lkIjoiYmU5ZGE1YzJmYmVhNDQwN2IyZjQwZWJhYWQ4NTlhZDQiLCJnZW5lcmF0ZWQiOjE2Mzg3MTcyNzgsImNhbGRlcmFHdWlkIjoiMzgxMGI4NjMtMmE2NS00NDU3LTliNTgtNGRhYjNiNDgyYTg2IiwiYWNQcm92aWRlciI6IkVhc3lBbnRpQ2hlYXQiLCJub3RlcyI6IiIsImZhbGxiYWNrIjpmYWxzZX0.VAWQB67RTxhiWOxx7DBjnzDnXyyEnX7OljJm-j2d88G_WgwQ9wrE6lwMEHZHjBd1ISJdUO1UVUqkfLdU5nofBQ -AUTH_LOGIN={email} -AUTH_PASSWORD={password} -AUTH_TYPE=epic";
            shipping.StartInfo.UseShellExecute = true;
            shipping.StartInfo.RedirectStandardOutput = false;
            shipping.StartInfo.RedirectStandardError = false;


            launcher.Start();
            shipping.Start();
        }

        private void Loader_Load(object sender, EventArgs e)
        {

        }

        private void guna2GradientButton4_Click(object sender, EventArgs e) // Launch Game button
        {
            string appdata = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\ZeroGravity";
            string gamePathFile = Path.Combine(appdata, "path.txt");

            if (!File.Exists(gamePathFile) || string.IsNullOrWhiteSpace(File.ReadAllText(gamePathFile)))
            {
                MessageBox.Show("Please set a path by pressing the settings icon, then try launching the game again.", "Path Not Set", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            StartGame(File.ReadAllText(gamePathFile));
        }

        private void guna2Button7_Click(object sender, EventArgs e)
        {
            //Home: Library
            home_page.Visible = true;
            library_page.Visible = true;
            notes_page.Visible = false;
            settings_page.Visible = false;
        }

        private void guna2Button6_Click(object sender, EventArgs e)
        {
            //Home: Notes
            home_page.Visible = true;
            library_page.Visible = true;
            notes_page.Visible = true;
            settings_page.Visible = false;
        }

        private void guna2Button4_Click(object sender, EventArgs e)
        {
            //Home: Settings
            home_page.Visible = true;
            library_page.Visible = true;
            notes_page.Visible = true;
            settings_page.Visible = true;
        }

        private void guna2Button5_Click(object sender, EventArgs e)
        {
            //Library: Home
            home_page.Visible = true;
            library_page.Visible = false;
            notes_page.Visible = false;
            settings_page.Visible = false;
        }

        private void guna2Button2_Click(object sender, EventArgs e)
        {
            //Library: Notes
            home_page.Visible = true;
            library_page.Visible = true;
            notes_page.Visible = true;
            settings_page.Visible = false;
        }

        private void guna2Button1_Click(object sender, EventArgs e)
        {
            //Library: Notes
            home_page.Visible = true;
            library_page.Visible = true;
            notes_page.Visible = true;
            settings_page.Visible = true;
        }

        private void guna2Button16_Click(object sender, EventArgs e)
        {
            //Notes: Home
            home_page.Visible = true;
            library_page.Visible = false;
            notes_page.Visible = false;
            settings_page.Visible = false;
        }

        private void guna2Button15_Click(object sender, EventArgs e)
        {
            //Notes: Library
            home_page.Visible = true;
            library_page.Visible = true;
            notes_page.Visible = false;
            settings_page.Visible = false;
        }

        private void guna2Button13_Click(object sender, EventArgs e)
        {
            //Notes: Settings
            home_page.Visible = true;
            library_page.Visible = true;
            notes_page.Visible = true;
            settings_page.Visible = true;
        }

        private void guna2Button18_Click(object sender, EventArgs e)
        {
            //Settings: Home
            home_page.Visible = true;
            library_page.Visible = false;
            notes_page.Visible = false;
            settings_page.Visible = false;
        }

        private void guna2Button17_Click(object sender, EventArgs e)
        {
            //Settings: Library
            home_page.Visible = true;
            library_page.Visible = true;
            notes_page.Visible = false;
            settings_page.Visible = false;
        }

        private void guna2Button12_Click(object sender, EventArgs e)
        {
            //Settings: Notes
            home_page.Visible = true;
            library_page.Visible = true;
            notes_page.Visible = true;
            settings_page.Visible = false;
        }
        private void Savepath()
        {
            string emailPath = Path.Combine(appdata, "path.txt");
            string newpath = guna2TextBox1.Text;

            if (!Directory.Exists(appdata))
            {
                Directory.CreateDirectory(appdata);
            }

            if (File.Exists(emailPath))
            {
                string currentEmail = File.ReadAllText(emailPath);
                if (currentEmail != newpath)
                {
                    File.WriteAllText(emailPath, newpath);
                }
            }
            else
            {
                File.WriteAllText(emailPath, newpath);
            }
        }

        private void guna2GradientButton2_Click(object sender, EventArgs e)
        {
            Savepath();
        }

        private void guna2TextBox1_TextChanged(object sender, EventArgs e)
        {
            guna2TextBox1.Text = Path.Combine(appdata, "6.31");
        }

        private async void guna2Button9_Click(object sender, EventArgs eventArgs)
        {
            var handle = GetConsoleWindow();
            string downloadUrl = "https://public.simplyblk.xyz/6.31.rar";
            string tempFilePath = Path.Combine(Path.GetTempPath(), "downloaded.rar");
            string extractPath = Path.Combine(appdata, "6.31");

            try
            {
                ShowWindow(handle, SW_SHOW);
                using (WebClient client = new WebClient())
                {
                    client.DownloadProgressChanged += (s, e) =>
                    {
                        double downloadedGB = e.BytesReceived / (1024.0 * 1024.0 * 1024.0);
                        double totalGB = e.TotalBytesToReceive / (1024.0 * 1024.0 * 1024.0);
                        double progressPercentage = e.ProgressPercentage;

                        Console.Write($"\rDownloaded: {downloadedGB:F2} GB / {totalGB:F2} GB ({progressPercentage:F2}%)");
                    };

                    client.DownloadFileCompleted += (s, e) =>
                    {
                        if (e.Error != null)
                        {
                            Console.WriteLine($"\nDownload failed: {e.Error.Message}");
                            return;
                        }
                        Console.WriteLine("\nDownload completed successfully.");
                    };

                    await client.DownloadFileTaskAsync(new Uri(downloadUrl), tempFilePath);
                }

                Console.WriteLine("Extracting the contents...");
                using (var archive = RarArchive.Open(tempFilePath))
                {
                    foreach (var entry in archive.Entries)
                    {
                        if (!entry.IsDirectory)
                        {
                            Console.WriteLine($"Extracting {entry.Key}...");
                            entry.WriteToDirectory(extractPath, new ExtractionOptions
                            {
                                ExtractFullPath = true,
                                Overwrite = true
                            });
                        }
                    }
                }
                Console.WriteLine("Extraction completed successfully.");
                ShowWindow(handle, SW_HIDE);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\nAn error occurred: {ex.Message}");
            }
            finally
            {
                if (File.Exists(tempFilePath))
                {
                    File.Delete(tempFilePath);
                }
            }
        }


        private void settings_page_Paint(object sender, PaintEventArgs e)
        {

        }

        private void guna2GradientPanel14_Paint(object sender, PaintEventArgs e)
        {

        }

        private void guna2HtmlLabel26_Click(object sender, EventArgs e)
        {

        }

        private void guna2Button11_Click(object sender, EventArgs e)
        {

        }

        private void guna2HtmlLabel25_Click(object sender, EventArgs e)
        {

        }

        private void guna2HtmlLabel22_Click(object sender, EventArgs e)
        {

        }
    }
}