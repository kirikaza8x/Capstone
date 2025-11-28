// using System;
// using System.Diagnostics;
// using Microsoft.Extensions.Logging;

// namespace SharedLibrary.Utils
// {
//     public class AutoMigration
//     {
//         private readonly ILogger _logger;

//         public AutoMigration(ILogger logger)
//         {
//             _logger = logger;

//             // bin/Debug/netX → API project folder
//             var apiProjectDir = Directory.GetParent(AppContext.BaseDirectory)!.Parent!.Parent!.Parent!.FullName;

//             // Solution root = one level up from API project
//             var solutionDir = Directory.GetParent(apiProjectDir)!.FullName;

//             SolutionDir = solutionDir;
//             ApiProjectDir = apiProjectDir;
            
//             // Use relative paths from API directory (where dotnet ef runs from manually)
//             ProjectPath = @"..\ClothingStore.Infrastructure\ClothingStore.Infrastructure.csproj";
//             StartupProject = @"ClothingStore.BE.API.csproj";
//         }

//         public string SolutionDir { get; }
//         public string ApiProjectDir { get; }

//         // Migration settings
//         public string MigrationName { get; set; } = "AutoMigration";
//         public string MsBuildProjectExtensionsPath { get; set; } = "Build/obj";
//         public string ProjectPath { get; set; }
//         public string StartupProject { get; set; }

//         public string OutputDir { get; set; } = "Migrations";
//         public string DbContextName { get; set; } = "UsersDbContext";

//         // Connection string props (not used in CLI, but useful if extended later)
//         private string Host { get; set; } = "localhost";
//         private int Port { get; set; } = 5432;
//         private string Database { get; set; } = "clothing_store";
//         private string Username { get; set; } = "postgres";
//         private string Password { get; set; } = "123456789";

//         private void ExecuteMigrationCommand(string command, string additionalArgs = "")
//         {
//             _logger.LogInformation("AutoMigration settings:");
//             _logger.LogInformation("  SolutionDir: {SolutionDir} (Exists: {Exists})", SolutionDir, Directory.Exists(SolutionDir));
//             _logger.LogInformation("  ApiProjectDir: {ApiProjectDir} (Exists: {Exists})", ApiProjectDir, Directory.Exists(ApiProjectDir));
//             _logger.LogInformation("  ProjectPath: {ProjectPath}", ProjectPath);
//             _logger.LogInformation("  StartupProject: {StartupProject}", StartupProject);
//             _logger.LogInformation("  DbContextName: {DbContextName}", DbContextName);
//             _logger.LogInformation("  OutputDir: {OutputDir}", OutputDir);
//             _logger.LogInformation("  MsBuildProjectExtensionsPath: {MsBuildProjectExtensionsPath}", MsBuildProjectExtensionsPath);
            
//             string arguments;

//             if (command == "add")
//             {
//                 arguments = $"ef migrations add {additionalArgs} " +
//                             $"--context {DbContextName} " +
//                             $"--output-dir \"{OutputDir}\" " +
//                             $"--project \"{ProjectPath}\" " +
//                             $"--startup-project \"{StartupProject}\" " +
//                             $"--verbose";
//             }
//             else if (command == "update")
//             {
//                 arguments = $"ef database update {additionalArgs} " +
//                             $"--context {DbContextName} " +
//                             $"--project \"{ProjectPath}\" " +
//                             $"--startup-project \"{StartupProject}\" " +
//                             $"--verbose";
//             }
//             else
//             {
//                 throw new ArgumentException($"Unsupported command: {command}");
//             }

//             _logger.LogInformation("Executing command: dotnet {Args}", arguments);
//             _logger.LogInformation("Working directory: {WorkingDir}", ApiProjectDir);

//             var allOutput = new System.Text.StringBuilder();

//             try
//             {
//                 using var process = new Process
//                 {
//                     StartInfo = new ProcessStartInfo
//                     {
//                         FileName = "cmd.exe",
//                         Arguments = $"/c dotnet {arguments}",
//                         WorkingDirectory = ApiProjectDir,
//                         RedirectStandardOutput = true,
//                         RedirectStandardError = true,
//                         UseShellExecute = false,
//                         CreateNoWindow = true
//                     }
//                 };

//                 process.OutputDataReceived += (sender, e) =>
//                 {
//                     if (!string.IsNullOrEmpty(e.Data))
//                     {
//                         _logger.LogInformation("{Message}", e.Data);
//                         allOutput.AppendLine(e.Data);
//                     }
//                 };

//                 process.ErrorDataReceived += (sender, e) =>
//                 {
//                     if (!string.IsNullOrEmpty(e.Data))
//                     {
//                         _logger.LogError("{Message}", e.Data);
//                         allOutput.AppendLine(e.Data);
//                     }
//                 };

//                 process.Start();
//                 process.BeginOutputReadLine();
//                 process.BeginErrorReadLine();
//                 process.WaitForExit();

//                 if (process.ExitCode != 0)
//                 {
//                     var output = allOutput.ToString();
//                     _logger.LogError("Migration {Command} failed with exit code {ExitCode}. Full output:\n{Output}", 
//                         command, process.ExitCode, output);
                    
//                     // Write to file for inspection
//                     var logFile = Path.Combine(ApiProjectDir, $"migration_error_{DateTime.Now:yyyyMMdd_HHmmss}.log");
//                     File.WriteAllText(logFile, output);
//                     _logger.LogError("Full error output written to: {LogFile}", logFile);
                    
//                     throw new Exception($"Migration {command} process failed with exit code {process.ExitCode}. Check logs above for details.");
//                 }

//                 _logger.LogInformation("Migration {Command} completed successfully.", command);
//             }
//             catch (Exception ex)
//             {
//                 _logger.LogError(ex, "Migration {Command} process failed.", command);
//                 throw;
//             }
//         }

//         public void GenerateMigration()
//         {
//             var migrationName = MigrationName + "_" + DateTime.Now.ToString("yyyyMMddHHmmss");
//             ExecuteMigrationCommand("add", migrationName);
//         }

//         public void ApplyMigration()
//         {
//             _logger.LogInformation("Applying pending migrations to the database");
//             ExecuteMigrationCommand("update");
//         }

//         public void RollbackMigration(string targetMigration = "0")
//         {
//             _logger.LogInformation("Rolling back to migration: {TargetMigration}", targetMigration);
//             ExecuteMigrationCommand("update", targetMigration);
//         }
//     }
// }