using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CommandLine;

namespace CombineGarminVideo
{
    class Program
    {
        static async Task<int> Main(string[] args)
        {

            return await Parser
                .Default
                .ParseArguments<CommandLineOptions>(args)
                .MapResult(async (CommandLineOptions opts) =>
                {
                    try
                    {
                        return await ProcessClips(opts.InputFolder, opts.Delete, opts.OutputFolder);
                    }
                    catch
                    {
                        ConsoleColor.Red.WriteLine("Error!");
                        return -3; // Unhandled error
                    }
                },
                errs => Task.FromResult(-1)); // Invalid arguments
        }

        private static async Task<int> ProcessClips(string inputFolder, bool deleteFiles, string outputFolder)
        {
            if (!TryResolveSourcePath(inputFolder, out string sourcePath))
            {
                return -1;
            }

            if (!TryResolveOutputPath(outputFolder, out string destPath))
            {
                return -1;
            }

            var files = Directory.EnumerateFiles(sourcePath, "*.mp4").OrderBy(f => File.GetLastWriteTime(sourcePath)).ToList();
            List<string> batch = new List<string>();

            while (files.Count > 0)
            {
                batch.Add(files[0]);
                var curFileTime = File.GetLastWriteTime(files[0]);
                var nextFileTime = (files.Count > 1) ? File.GetLastWriteTime(files[1]) : DateTime.MaxValue;
                var secondsBetweenFiles = (nextFileTime - curFileTime).TotalSeconds;
                // If the next file is more than a minute away (well, 70 seconds just to be sure), run the batch.
                if (secondsBetweenFiles > 70)
                {
                    var batchName = $"{File.GetLastWriteTime(batch[0]).ToString("yyyyMMdd.HHmm")}";
                    var batchListFile = Path.Combine(destPath, $"{batchName}.txt");
                    using (StreamWriter outputFile = new StreamWriter(batchListFile, false))
                    {
                        foreach (var item in batch)
                        {
                            outputFile.WriteLine($"file '{item}'");
                        }
                    }
                    double totalSize = batch.Sum(file => new FileInfo(file).Length);

                    var batchOutPath = Path.Combine(destPath, $"{batchName}.mp4");
                    Console.WriteLine(batchOutPath);

                    ConsoleColor.Green.WriteLine($"Combining {Math.Floor(totalSize / 1048576)}mb from {batch.Count} files into {batchOutPath}");
                    var cmd = $"ffmpeg -y -safe 0 -f concat -i {batchListFile} -c copy -map 0:v -map 0:a -map 0:3? -copy_unknown -tag:2 gpmd {batchOutPath}";
                    RunCommand(sourcePath, cmd);

                    if (deleteFiles)
                    {
                        ConsoleColor.Green.WriteLine("Deleting source clips in this batch");
                        foreach(var f in batch)
                        {
                            File.Delete(f);
                            ConsoleColor.Green.WriteLine($"Deleted {f}");
                        }
                    }

                    File.Delete(batchListFile);
                    batch.Clear();

                }
                files.RemoveAt(0);
            }

            await Task.CompletedTask;
            return 0;

        }

        private static bool TryResolveOutputPath(string path, out string resolvedPath)
        {
            resolvedPath = path;
            if (string.IsNullOrEmpty(resolvedPath)) { resolvedPath = Environment.CurrentDirectory; }
            if (!Directory.Exists(resolvedPath))
            {
                ConsoleColor.Red.WriteLine($"Output path does not exist: {resolvedPath}");
                resolvedPath = null;
                return false;
            }
            return true;
        }

        private static bool TryResolveSourcePath(string path, out string resolvedPath)
        {
            resolvedPath = path;
            if (string.IsNullOrEmpty(resolvedPath)) { resolvedPath = Environment.CurrentDirectory; }

            if (!Directory.Exists(resolvedPath))
            {
                ConsoleColor.Red.WriteLine($"Input path does not exist: {resolvedPath}");
                resolvedPath = null;
                return false;
            }

            if (!Directory.GetFiles(resolvedPath, "*.mp4").Any())
            {
                ConsoleColor.Red.WriteLine($"No MP4 files in directory: {resolvedPath}");
                resolvedPath = null;
                return false;
            }
            return true;
        }

        private static void RunCommand(string directory, string command)
        {
            Process process = new System.Diagnostics.Process();
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.WindowStyle = ProcessWindowStyle.Normal;
            startInfo.FileName = "cmd.exe";
            startInfo.Arguments = $"/C {command}";
            startInfo.WorkingDirectory = directory;
            //startInfo.RedirectStandardOutput = true;
            process.StartInfo = startInfo;
            process.Start();
            process.WaitForExit();
            //return process.StandardOutput.ReadToEnd();
        }


    }
    
}
