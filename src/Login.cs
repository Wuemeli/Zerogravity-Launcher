using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Xml.Linq;
using System.IO;



namespace Project_ZeroGravity_Loader
{
    public partial class Login : Form
    {
        string appdata = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "ZeroGravity");

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

        public Login()
        {
            InitializeComponent();
            LoadData();
            this.FormBorderStyle = FormBorderStyle.None;
            Region = System.Drawing.Region.FromHrgn(CreateRoundRectRgn(0, 0, Width, Height, 15, 15));
        }

        private void LoadData()
        {
            if (File.Exists(Path.Combine(appdata, "data.txt")))
            {
                string[] data = File.ReadAllLines(Path.Combine(appdata, "data.txt"));
                guna2TextBox1.Text = data[0];
                guna2TextBox2.Text = data[1];
            }
        }

        private async Task<bool> ValidateCredentialsAsync(string email, string password)
        {
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    var loginData = new
                    {
                        username = email,
                        password = password
                    };

                    var content = new StringContent(JsonConvert.SerializeObject(loginData), Encoding.UTF8, "application/json");

                    HttpResponseMessage response = await client.PostAsync("https://backend.zerogravity.rip/account/api/login", content);

                    if (response.IsSuccessStatusCode)
                    {
                        string responseBody = await response.Content.ReadAsStringAsync();
                        var responseJson = JObject.Parse(responseBody);

                        if (responseJson["status"]?.ToString() == "error")
                        {
                            string message = responseJson["message"]?.ToString();

                            if (message == "Account is banned")
                            {
                                MessageBox.Show("You have been banned.", "Access Denied", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                Close();
                                return false;
                            }

                            MessageBox.Show($"Error: {message}", "Login Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return false;
                        }

                        if (responseJson["status"]?.ToString() == "success")
                        {
                            return true;
                        }
                    }
                    else
                    {
                        MessageBox.Show($"HTTP error occurred: {response.StatusCode}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                return false;
            }
        }

        private void Login_Load(object sender, EventArgs e)
        {

        }

        private void guna2HtmlLabel3_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("https://discord.gg/zerogravitymp");
        }

        private void SaveData()
        {
            string dataPath = Path.Combine(appdata, "data.txt");
            string newEmail = guna2TextBox1.Text;
            string newPassword = guna2TextBox2.Text;

            if (!Directory.Exists(appdata))
            {
                Directory.CreateDirectory(appdata);
            }

            File.WriteAllText(dataPath, $"{newEmail}\n{newPassword}");
        }
        private async void guna2GradientButton1_Click(object sender, EventArgs e) 
        {
            string email = guna2TextBox1.Text;
            string password = guna2TextBox2.Text;

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Please enter both email and password.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            bool isValid = await ValidateCredentialsAsync(email, password);

            if (isValid)
            {
                SaveData();

                Loader loadermenu = new Loader();
                loadermenu.Show();
                this.Hide();
                loadermenu.FormClosed += (s, args) => this.Close();
            }
            else
            {
                MessageBox.Show("Invalid email or password.", "Login Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void guna2TextBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void guna2TextBox2_TextChanged(object sender, EventArgs e)
        {

        }
    }
}