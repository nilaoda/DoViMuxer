using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DoViMuxer
{
    internal class Utils
    {
        public static async Task RunCommandAsync(string name, string arg, bool showDetail = false)
        {
            if (showDetail) LogGray($"{name} {arg}");

            var p = Process.Start(new ProcessStartInfo()
            {
                FileName = name,
                Arguments = arg,
                UseShellExecute = false
            });
            await p!.WaitForExitAsync();
            if (p.ExitCode != 0) throw new ThreadStateException("Execution failed");
        }

        public static void LogColor(string message)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(message);
            Console.ResetColor();
        }

        public static void LogError(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Error.WriteLine(message);
            Console.ResetColor();
        }

        public static void LogGray(string message)
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine(message);
            Console.ResetColor();
        }

        public static List<string> FormatGlobalInfo(string? globalTitle, string? globalCopyright, string? globalComment, string? globalEncodingTool)
        {
            var list = new List<string>();
            if (!string.IsNullOrEmpty(globalTitle)) list.Add($"Title: {globalTitle}");
            if (!string.IsNullOrEmpty(globalCopyright)) list.Add($"Copyright: {globalCopyright}");
            if (!string.IsNullOrEmpty(globalComment)) list.Add($"Comment: {globalComment}");
            if (!string.IsNullOrEmpty(globalEncodingTool)) list.Add($"EncodingTool: {globalEncodingTool}");
            list.Add("");
            return list;
        }

        /// <summary>
        /// 寻找可执行程序
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string? FindExecutable(string name)
        {
            var fileExt = OperatingSystem.IsWindows() ? ".exe" : "";
            var searchPath = new[] { Environment.CurrentDirectory, Path.GetDirectoryName(Environment.ProcessPath) };
            var envPath = Environment.GetEnvironmentVariable("PATH")?.Split(Path.PathSeparator) ??
                          Array.Empty<string>();
            return searchPath.Concat(envPath).Select(p => Path.Combine(p, name + fileExt)).FirstOrDefault(File.Exists);
        }
    }
}
