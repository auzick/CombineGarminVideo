using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using FFMpegCore;

namespace CombineGarminVideo
{
    class Program
    {
        static void Main(string[] args)
        {
            var sourcePath = (args.Length > 0) ? args[0] ?? "" : "";

            if (string.IsNullOrEmpty(sourcePath))
                sourcePath = Environment.CurrentDirectory;

            if (!Directory.Exists(sourcePath))
            {
                Console.WriteLine($"Path does not exist: {sourcePath}");
                return;
            }

            var files = Directory.EnumerateFiles(sourcePath, "*.mp4").OrderBy(f => File.GetLastWriteTime(sourcePath)).ToList();
            if (files.Count == 0) return;

            List<string> batch = new List<string>();

            while (files.Count > 0)
            {
                batch.Add(files[0]);
                var curFileTime = File.GetLastWriteTime(files[0]);
                var nextFileTime = (files.Count > 1) ? File.GetLastWriteTime(files[1]) : DateTime.MaxValue;
                var secondsBetweenFiles = (nextFileTime - curFileTime).TotalSeconds;
                // If the next file is more than a minute away (well, 70 seconds just to be sure), run the batch.
                //Console.WriteLine($"file '{files[0]}'");
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
                    Console.WriteLine($"Combining {Math.Floor(totalSize / 1048576)}mb from {batch.Count} files into {batchName}.mp4");
                    var cmd = $"ffmpeg -y -safe 0 -f concat -i {batchName}.txt -c copy -map 0:v -map 0:a -map 0:3? -copy_unknown -tag:2 gpmd {batchName}.mp4";
                    RunCommand(sourcePath, cmd);
                    batch.Clear();
                    File.Delete(Path.Combine(sourcePath, $"{batchName}.txt"));
                }
                files.RemoveAt(0);
            }

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
