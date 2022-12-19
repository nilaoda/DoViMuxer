using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace DoViMuxer
{
    internal partial class MediainfoUtil
    {
        [GeneratedRegex("  Stream #.*")]
        private static partial Regex TextRegex();
        [GeneratedRegex("#0:\\d(\\[0x\\w+?\\])")]
        private static partial Regex IdRegex();
        [GeneratedRegex("\\((\\w{2,3})\\):")]
        private static partial Regex LangRegex();
        [GeneratedRegex(": (\\w+): (.*)")]
        private static partial Regex TypeRegex();
        [GeneratedRegex("(.*?)(,|$)")]
        private static partial Regex BaseInfoRegex();
        [GeneratedRegex(" \\/ 0x\\w+")]
        private static partial Regex ReplaceRegex();
        [GeneratedRegex("\\d{2,}x\\d+")]
        private static partial Regex ResRegex();
        [GeneratedRegex("\\d+ kb\\/s")]
        private static partial Regex BitrateRegex();
        [GeneratedRegex("(\\d+(\\.\\d+)?) fps")]
        private static partial Regex FpsRegex();
        [GeneratedRegex("DOVI configuration record.*profile: (\\d).*compatibility id: (\\d)")]
        private static partial Regex DoViRegex();
        [GeneratedRegex("\\[CHAPTER\\]\\nTIMEBASE=(.*?)\\nSTART=(\\d+)\\nEND=(\\d+)\\ntitle=(.*)")]
        private static partial Regex ChapterRegex();

        public static async Task<List<Mediainfo>> ReadInfoAsync(string binary, string file)
        {
            var result = new List<Mediainfo>();

            if (string.IsNullOrEmpty(file) || !File.Exists(file)) return result;

            string cmd = "-hide_banner -i \"" + file + "\"";
            var p = Process.Start(new ProcessStartInfo()
            {
                FileName = binary,
                Arguments = cmd,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false
            })!;
            var output = p.StandardError.ReadToEnd();
            await p.WaitForExitAsync();

            var index = 0;

            foreach (Match stream in TextRegex().Matches(output))
            {
                var info = new Mediainfo()
                {
                    FilePath = file,
                    IndexOfFile = index++,
                    Text = TypeRegex().Match(stream.Value).Groups[2].Value.TrimEnd(),
                    Id = IdRegex().Match(stream.Value).Groups[1].Value,
                    Type = TypeRegex().Match(stream.Value).Groups[1].Value,
                };

                if (LangRegex().IsMatch(stream.Value))
                {
                    info.LangCode = LangRegex().Match(stream.Value).Groups[1].Value;
                }

                if (info.Type == "Video" && stream.Value.Contains("(attached pic)"))
                {
                    result.ForEach(a => { if (a.Type == "Video") a.GlobalHasCover = true; });
                }

                if (info.Type != "Video" && info.Type != "Audio" && info.Type != "Subtitle")
                {
                    index++;
                    continue;
                }

                if (info.Type == "Video")
                {
                    if (info.Text.StartsWith("hevc"))
                    {
                        info.H264 = false;
                    }
                    else if (info.Text.StartsWith("h264"))
                    {
                        info.H264 = true;
                    }
                    else
                    {
                        index++;
                        continue;
                    }
                }

                if (info.Type == "Video" && DoViRegex().IsMatch(output))
                {
                    info.DVProfile = Convert.ToInt32(DoViRegex().Match(output).Groups[1].Value);
                    info.DVComId = Convert.ToInt32(DoViRegex().Match(output).Groups[2].Value);
                }

                info.Resolution = ResRegex().Match(info.Text).Value;
                info.Bitrate = BitrateRegex().Match(info.Text).Value;
                info.Fps = FpsRegex().Match(info.Text).Value;
                info.BaseInfo = BaseInfoRegex().Match(info.Text).Groups[1].Value;
                info.BaseInfo = ReplaceRegex().Replace(info.BaseInfo, "");
                info.HDR = info.Text.Contains("/bt2020/");

                if (info.BaseInfo.Contains("dvhe")
                    || info.BaseInfo.Contains("dvh1")
                    || info.BaseInfo.Contains("DOVI")
                    || info.Type.Contains("dvvideo")
                    || DoViRegex().IsMatch(output)
                    )
                    info.DolbyVison = true;

                result.Add(info);
            }

            if (result.Count == 0)
            {
                result.Add(new Mediainfo()
                {
                    Type = "Unknown"
                });
            }

            return result;
        }

        public static async Task ReadMediainfoAsync(string binary, List<Mediainfo> mediainfos, bool showDetail = false)
        {
            if (mediainfos.Count == 0) return;

            try
            {
                string file = mediainfos.First().FilePath!;
                string cmd = "--Output=JSON --Language=raw \"" + file + "\"";

                if (showDetail) Utils.LogGray($"{binary} {cmd}");

                var p = Process.Start(new ProcessStartInfo()
                {
                    FileName = binary,
                    Arguments = cmd,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    StandardErrorEncoding = Encoding.UTF8,
                    StandardOutputEncoding = Encoding.UTF8,
                    UseShellExecute = false
                })!;
                var output = p.StandardOutput.ReadToEnd().Trim();
                await p.WaitForExitAsync();
                var json = JsonNode.Parse(output)!.AsObject()!;
                var media = json["media"]!["track"]![0]!;
                var globalTitle = media["Title"]?.GetValue<string>();
                var globalCopyright = media["Copyright"]?.GetValue<string>();
                var globalComment = media["Comment"]?.GetValue<string>();
                var globalEncodingTool = media["Encoded_Application"]?.GetValue<string>();

                var infoArray = json["media"]!["track"]!.AsArray();
                var vArray = json["media"]!["track"]!.AsArray().Where(a => a?["@type"]!.GetValue<string>() == "Video");
                var aArray = json["media"]!["track"]!.AsArray().Where(a => a?["@type"]!.GetValue<string>() == "Audio");
                var sArray = json["media"]!["track"]!.AsArray().Where(a => a?["@type"]!.GetValue<string>() == "Text");

                foreach (var item in mediainfos)
                {
                    item.GlobalComment = globalComment;
                    item.GlobalEncodingTool = globalEncodingTool;
                    item.GlobalTitle = globalTitle;
                    item.GlobalCopyright = globalCopyright;
                }

                var aTracks = mediainfos.Where(m => m.Type == "Audio").ToList();
                var sTracks = mediainfos.Where(m => m.Type == "Subtitle").ToList();

                for (int i = 0; i < mediainfos.Count; i++)
                {
                    var ele = mediainfos[i];
                    var mediainfoType = ele.Type == "Subtitle" ? "Text" : ele.Type;
                    var index = ele.Type == "Subtitle" ? sTracks.IndexOf(ele) : ele.Type == "Audio" ? aTracks.IndexOf(ele) : 0;
                    var node = infoArray.Where(a => a?["@type"]?.GetValue<string>() == mediainfoType).ElementAtOrDefault(index)?.AsObject();
                    if (mediainfoType == "Video") node = infoArray.FirstOrDefault(a => a?["@type"]?.GetValue<string>() == mediainfoType)?.AsObject();
                    
                    if (node != null)
                    {
                        if (ele.Type != "Video") ele.Name = node["Title"]?.GetValue<string>();
                        if (ele.Type != "Video") ele.ExtendedLanguageTag = node["Language"]?.GetValue<string>();
                        if (node.ContainsKey("Default"))
                        {
                            ele.Default = node["Default"]!.GetValue<string>() == "Yes";
                        }
                        if (node.ContainsKey("Forced"))
                        {
                            ele.Forced = node["Forced"]!.GetValue<string>() == "Yes";
                        }
                        if (node.ContainsKey("Delay"))
                        {
                            var str = node["Delay"]!.GetValue<string>();
                            var d = 1000 * double.Parse(str, System.Globalization.NumberStyles.AllowDecimalPoint, System.Globalization.CultureInfo.InvariantCulture);
                            ele.Delay = (long)d;
                        }
                        else if (node.ContainsKey("extra") && node["extra"]!.AsObject().ContainsKey("Source_Delay"))
                        {
                            ele.Delay = Convert.ToInt64(node["extra"]!["Source_Delay"]!.GetValue<string>());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Utils.LogWarn("mediainfo: " + ex.Message);
            }
        }

        public static async Task<List<Chapter>> ReadChapterAsync(string binary, string file, bool showDetail = false)
        {
            var list = new List<Chapter>();
            try
            {
                var outFile = Path.GetTempFileName() + ".txt";
                var cmd = $"-loglevel quiet -i \"{file}\" -f ffmetadata -y \"{outFile}\"";

                if (showDetail) Utils.LogGray($"{binary} {cmd}");

                var p = Process.Start(new ProcessStartInfo()
                {
                    FileName = binary,
                    Arguments = cmd,
                    UseShellExecute = false
                })!;
                await p.WaitForExitAsync();
                await Task.Delay(100);

                if (File.Exists(outFile))
                {
                    var text = await File.ReadAllTextAsync(outFile);
                    if (ChapterRegex().IsMatch(text))
                    {
                        foreach (Match m in ChapterRegex().Matches(text))
                        {
                            var timescaleStr = m.Groups[1].Value;
                            var timescale = 1.0;
                            if (timescaleStr.Contains("/"))
                            {
                                timescale = Convert.ToInt32(timescaleStr.Split('/')[0]) / (double)Convert.ToInt32(timescaleStr.Split('/')[1]);
                            }

                            var start = Convert.ToInt64(m.Groups[2].Value) * timescale;
                            var end = Convert.ToInt64(m.Groups[3].Value) * timescale;
                            var title = m.Groups[4].Value;
                            list.Add(new Chapter()
                            {
                                Title = title,
                                Start = start,
                                End = end,
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Utils.LogWarn("read chapter: " + ex.Message);
            }
            return list;
        }
    }
}
