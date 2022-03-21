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
                        return await ProcessClips(opts.FolderPath, opts.Delete);
                    }
                    catch
                    {
                        ConsoleColor.Red.WriteLine("Error!");
                        return -3; // Unhandled error
                    }
                },
                errs => Task.FromResult(-1)); // Invalid arguments
        }

        private static async Task<int> ProcessClips(string folderPath, bool deleteFiles)
        {
            if (!TryResolveSourcePath(folderPath, out string sourcePath))
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
                    using (StreamWriter outputFile = new StreamWriter(Path.Combine(sourcePath, $"{batchName}.txt"), false))
                    {
                        foreach (var item in batch)
                        {
                            outputFile.WriteLine($"file '{item}'");
                        }
                    }
                    double totalSize = batch.Sum(file => new FileInfo(file).Length);
                    ConsoleColor.Green.WriteLine($"Combining {Math.Floor(totalSize / 1048576)}mb from {batch.Count} files into {batchName}.mp4");
                    var cmd = $"ffmpeg -y -safe 0 -f concat -i {batchName}.txt -c copy -map 0:v -map 0:a -map 0:3? -copy_unknown -tag:2 gpmd {batchName}.mp4";
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

                    File.Delete(Path.Combine(sourcePath, $"{batchName}.txt"));
                    batch.Clear();

                }
                files.RemoveAt(0);
            }

            await Task.CompletedTask;
            return 0;

        }


        private static bool TryResolveSourcePath(string path, out string resolvedPath)
        {
            resolvedPath = path;
            if (string.IsNullOrEmpty(resolvedPath)) { resolvedPath = Environment.CurrentDirectory; }

            if (!Directory.Exists(resolvedPath))
            {
                ConsoleColor.Red.WriteLine($"Path does not exist: {resolvedPath}");
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
