using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Texturizer
{
    class Program
    {
        private static readonly long WarningSize = 10_000_000;
        
        static void Main(string[] args)
        {
            var cmd = args[0];
            var targetDir = Directory.GetCurrentDirectory();

            if (cmd.Equals("warn"))
                FindFiles(targetDir, f => f.Length > WarningSize).ForEach(f => Console.WriteLine($"{f.Length / 1024 / 1024}MB - {f.FullName}"));  
            if (cmd.Equals("converttga"))          
                FindFiles(targetDir, f => f.Extension == ".tga").ForEach(f => ConvertToPng(f.FullName));                
            if (cmd.Equals("converttif"))          
                FindFiles(targetDir, f => f.Extension == ".tif").ForEach(f => ConvertTifToPng(f.FullName));
            if (cmd.Equals("resize2k"))
                FindFiles(targetDir, f => f.Extension == ".png").ForEach(f => ResizeTo2k(f.FullName));
        }

        private static void ResizeTo2k(string file)
        {
            var process = ExecuteCommand($"magick {file} -resize 2048x2048 {file}");
            if (process.ExitCode == 0)
            {
                Console.WriteLine($"Resized {file} to {file}");
            }
            else
            {
                Console.WriteLine($"Failed to resize");
            }
        }

        private static void ConvertTifToPng(string file)
        {
            var sw = Stopwatch.StartNew();
            var newFilename = file.Split('.')[0] + ".png";
            var process = ExecuteCommand($"magick {file} {newFilename}");
            if (process.ExitCode == 0)
            {
                Console.WriteLine($"Converted {file} to {newFilename} in {sw.ElapsedMilliseconds}");
                File.Delete(file);
            }
            else
            {
                Console.WriteLine($"Failed to convert");
            }
        }

        private static void ConvertToPng(string file)
        {
            var sw = Stopwatch.StartNew();
            var newFilename = file.Split('.')[0] + ".png";
            var process = ExecuteCommand($"magick {file} -flip {newFilename}");
            if (process.ExitCode == 0)
            {
                Console.WriteLine($"Converted {file} to {newFilename} in {sw.ElapsedMilliseconds}");
                File.Delete(file);
            }
            else
            {
                Console.WriteLine($"Failed to convert");
            }
        }

        private static Process ExecuteCommand(string cmd)
        {
            var process = new Process();
            var startInfo = new ProcessStartInfo
            {
                WindowStyle = ProcessWindowStyle.Hidden,
                FileName = "cmd.exe",
                Arguments = $"/C {cmd}"
            };
            process.StartInfo = startInfo;
            process.Start();
            process.WaitForExit();
            return process;
        }
        
        private static List<FileInfo> FindFiles(string dir, Func<FileInfo, bool> condition) =>
            // TODO: Change this to only current folder, or to be a parameter
            Directory.GetFiles(dir, "*.*", SearchOption.AllDirectories)
                .Select(x => new FileInfo(x))
                .Where(condition)
                .ToList();
    }
}