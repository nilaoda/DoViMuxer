using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DoViMuxer
{
    internal partial class Utils
    {
        [GeneratedRegex("libavutil\\s+(\\d+)\\. (\\d+)\\.")]
        private static partial Regex LibavutilRegex();
        [GeneratedRegex("GPAC version (\\d+)\\.")]
        private static partial Regex MP4boxRegex();
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

        public static void LogWarn(string message)
        {
            Console.ForegroundColor = ConsoleColor.DarkYellow;
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

        /// <summary>
        /// 检测ffmpeg是否识别杜比视界
        /// </summary>
        /// <returns></returns>
        public static bool CheckFFmpegDOVI(string bin)
        {
            try
            {
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = bin,
                        Arguments = "-version",
                        UseShellExecute = false,
                        RedirectStandardError = true,
                        RedirectStandardOutput = true,
                        CreateNoWindow = true
                    }
                };
                process.Start();
                string info = process.StandardOutput.ReadToEnd() + Environment.NewLine + process.StandardError.ReadToEnd();
                process.WaitForExit();
                var match = LibavutilRegex().Match(info);
                if (!match.Success) return false;
                if ((Convert.ToInt32(match.Groups[1].Value) == 57 && Convert.ToInt32(match.Groups[1].Value) >= 17)
                    || Convert.ToInt32(match.Groups[1].Value) > 57)
                {
                    return true;
                }
            }
            catch (Exception)
            {
            }
            return false;
        }

        /// <summary>
        /// 检测mp4box版本是否可用
        /// </summary>
        /// <returns></returns>
        public static bool CheckMP4Box(string bin)
        {
            try
            {
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = bin,
                        Arguments = "-version",
                        UseShellExecute = false,
                        RedirectStandardError = true,
                        RedirectStandardOutput = true,
                        CreateNoWindow = true
                    }
                };
                process.Start();
                string info = process.StandardOutput.ReadToEnd() + Environment.NewLine + process.StandardError.ReadToEnd();
                process.WaitForExit();
                var match = MP4boxRegex().Match(info);
                if (!match.Success) return false;
                if (Convert.ToInt32(match.Groups[1].Value) >= 2)
                {
                    return true;
                }
            }
            catch (Exception)
            {
            }
            return false;
        }
    }
}
