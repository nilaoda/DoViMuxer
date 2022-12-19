using System;
using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace DoViMuxer
{
    //[JsonSerializable(typeof(Config))]
    //internal partial class JsonContext : JsonSerializerContext { }

    internal partial class Program
    {
        [GeneratedRegex("\\<font face=\"Serif\" size=\"18\"\\>([\\s\\S]*?)\\<\\/font\\>")]
        private static partial Regex TagRegex();

        static async Task Main(string[] args)
        {
            Console.ResetColor();
            await CommandInvoker.InvokeArgs(args, DoWorkAsync);
        }

        private async static Task DoWorkAsync(MyOption option)
        {
            Console.WriteLine("DoViMuxer v1.0.6");
            var config = new Config();
            config.MP4Box = option.MP4Box ?? Utils.FindExecutable("mp4box") ?? Utils.FindExecutable("MP4box") ?? config.MP4Box;
            config.MP4Muxer = option.MP4Muxer ?? Utils.FindExecutable("mp4muxer") ?? config.MP4Muxer;
            config.FFmpeg = option.FFmpeg ?? Utils.FindExecutable("ffmpeg") ?? config.FFmpeg;
            config.Mediainfo = option.Mediainfo ?? Utils.FindExecutable("Mediainfo") ?? Utils.FindExecutable("mediainfo") ?? config.Mediainfo;

            if (!File.Exists(config.MP4Box)) throw new FileNotFoundException("mp4box not found! https://gpac.wp.imt.fr/downloads/pac-nightly-builds/");
            if (!File.Exists(config.FFmpeg)) throw new FileNotFoundException("ffmpeg not found! https://ffmpeg.org/download.html");
            if (!File.Exists(config.Mediainfo)) throw new FileNotFoundException("mediainfo not found! https://mediaarea.net/en/MediaInfo/Download");
            if (!File.Exists(config.MP4Muxer)) throw new FileNotFoundException("mp4muxer not found! https://github.com/DolbyLaboratories/dlb_mp4base/tree/master/bin");

            if (!Utils.CheckFFmpegDOVI(config.FFmpeg)) throw new Exception("ffmpeg version must >= 5.0! https://ffmpeg.org/download.html");
            if (!Utils.CheckMP4Box(config.MP4Box)) throw new Exception("mp4box version must >= 2.0! https://gpac.wp.imt.fr/downloads/pac-nightly-builds/");

            if (option.Debug)
            {
                Utils.LogGray($"ffmpeg: {config.FFmpeg}");
                Utils.LogGray($"mp4box: {config.MP4Box}");
                Utils.LogGray($"mediainfo: {config.Mediainfo}");
                Utils.LogGray($"mp4muxer: {config.MP4Muxer}");
            }

            var input = option.Inputs;

            if (!input.All(File.Exists))
            {
                throw new FileNotFoundException("Input file not found!");
            }

            Utils.LogColor("Reading inputs...");

            var dic = new Dictionary<int, List<Mediainfo>>();
            var chapterDic = new Dictionary<int, List<Chapter>>();

            for (int i = 0; i < input.Count; i++)
            {
                if (!option.NoChap)
                {
                    //读取chapter
                    chapterDic[i] = await MediainfoUtil.ReadChapterAsync(config.FFmpeg, input[i]);
                }

                var mediaInfos = await MediainfoUtil.ReadInfoAsync(config.FFmpeg, input[i]);
                await MediainfoUtil.ReadMediainfoAsync(config.Mediainfo, mediaInfos, option.Debug);
                //如果同一个文件中所有轨道的Delay都相同，则全部置为0
                if (mediaInfos.Select(m => m.Delay).Distinct().Count() == 1)
                {
                    mediaInfos.ForEach(m => m.Delay = 0);
                }
                //如果视频有Delay，则给其他轨道做偏移
                var v = mediaInfos.FirstOrDefault(m => m.Type == "Video");
                if (v != null && v.Delay != 0)
                {
                    mediaInfos.ForEach(m => { if (m.Delay != 0 && !m.Equals(v)) m.Delay -= v.Delay; });
                    v.Delay = 0;
                }

                dic[i] = mediaInfos;
                if (mediaInfos.Any())
                {
                    Console.WriteLine($"Input {i}: {Path.GetFullPath(input[i])}");
                    var m = mediaInfos.First();
                    Utils.FormatGlobalInfo(m.GlobalTitle, m.GlobalCopyright, m.GlobalComment, m.GlobalEncodingTool).ForEach(s => Console.WriteLine("\t" + s));
                    foreach (var item in mediaInfos)
                    {
                        Console.WriteLine($"\t{item}");
                    }
                }
            }

            //写入章节信息
            var chapterTxt = "";
            if (chapterDic.Any(c => c.Value.Count > 0))
            {
                var list = chapterDic.First(c => c.Value.Count > 0).Value;
                StringBuilder w = new();
                Console.WriteLine();
                foreach (var item in list)
                {
                    w.AppendLine($"{item}");
                    Console.WriteLine($"\tChapter: {item}");
                }
                chapterTxt = w.ToString();
            }

            var allMediainfos = dic.SelectMany(d => d.Value);
            var tmpFiles = new List<string>();
            var selectedTracks = new List<Mediainfo>();


            if (option.Maps != null && option.Maps.Count > 0)
            {
                foreach (var item in option.Maps)
                {
                    foreach (var t in FilterByUserMap(item, dic))
                    {
                        selectedTracks.Add(new Mediainfo(t));
                    }
                }
            }
            else
            {
                //默认全部
                selectedTracks.AddRange(allMediainfos);
            }

            //设置每个类型自己的序号
            SetTypeIndex(selectedTracks);

            //分析用户自定义meta
            if (option.Metas != null && option.Metas.Count > 0)
            {
                foreach (var meta in option.Metas)
                {
                    SetMetaFromUser(meta, selectedTracks);
                }
            }

            //分析用户自定义delay
            if (option.Delays != null && option.Delays.Count > 0)
            {
                foreach (var delay in option.Delays)
                {
                    SetDelayFromUser(delay, selectedTracks);
                }
            }

            //分析用户自定义Forced
            if (option.Forceds != null && option.Forceds.Count > 0)
            {
                foreach (var force in option.Forceds)
                {
                    SetForcedFromUser(force, selectedTracks);
                }
            }

            //分析用户自定义Default
            if (option.Defaults != null && option.Defaults.Count > 0)
            {
                foreach (var def in option.Defaults)
                {
                    SetDefaultFromUser(def, selectedTracks);
                }
            }

            var title = option.Title ?? selectedTracks.FirstOrDefault()?.GlobalTitle;
            var comment = option.Comment ?? selectedTracks.FirstOrDefault()?.GlobalComment;
            var tools = option.Tool ?? selectedTracks.FirstOrDefault()?.GlobalEncodingTool;
            var copyright = option.Copyright ?? selectedTracks.FirstOrDefault()?.GlobalCopyright;


            Utils.LogColor("\r\nOutput info:");
            Utils.FormatGlobalInfo(title, copyright, comment, tools).ForEach(s => Console.WriteLine("\t" + s));
            for (int i = 0; i < selectedTracks.Count; i++)
            {
                var item = selectedTracks[i];
                Console.WriteLine($"\t[{i}]: {input.IndexOf(item.FilePath!)}, {item.ToShortString()}");
            }

            if (chapterDic.Any(c => c.Value.Count > 0))
            {
                var list = chapterDic.First(c => c.Value.Count > 0).Value;
                Console.WriteLine();
                foreach (var item in list)
                {
                    Console.WriteLine($"\tChapter: {item}");
                }
            }

            //校验视频流
            CheckVideo(selectedTracks, out Mediainfo vTrack);

            var now = DateTime.Now.Ticks;

            var cover = option.Cover;

            if (!vTrack.GlobalHasCover && !string.IsNullOrEmpty(cover) && !File.Exists(cover))
            {
                throw new Exception("Cover not exists: " + cover);
            }

            if (string.IsNullOrEmpty(option.Output))
            {
                throw new Exception("Must set output file!");
            }

            //输出文件
            var output = option.Output.ToLower().EndsWith(".mp4") ? option.Output : option.Output + ".mp4";

            if (input.Select(Path.GetFullPath).Contains(Path.GetFullPath(output)))
            {
                throw new Exception("The output file cannot be the same as the input file!");
            }

            if (File.Exists(output) && !option.Yes)
            {
                throw new Exception($"File '{Path.GetFullPath(output)}' already exists! add option -y to overwrite.");
            }

            Console.WriteLine($"\r\nOutput 0: {Path.GetFullPath(output)}");

#if DEBUG
            Console.WriteLine("\r\nPress ENTER to continue...");
            Console.ReadKey();
#endif

            var startTime = DateTime.Now;

            if (vTrack.GlobalHasCover && cover == null)
            {
                //抽取封面
                Utils.LogColor("\r\nExtract cover image...");
                await Utils.RunCommandAsync(config.FFmpeg, $"-nostdin -loglevel error -i \"{vTrack.FilePath}\" -map 0:v:1 -vframes 1 -y -f image2 -pix_fmt rgb24 \"{now}.png\"", option.Debug);
                cover = $"{now}.png";
                tmpFiles.Add(cover);
            }

            Utils.LogColor("\r\nExtract video track...");
            var videoExt = vTrack.H264 ? "h264" : "hevc";
            await Utils.RunCommandAsync(config.FFmpeg, $"-nostdin -loglevel warning -i \"{vTrack.FilePath}\" -c copy -vbsf {(vTrack.H264 ? "h264_mp4toannexb" : "hevc_mp4toannexb")} -f {videoExt} \"{now}.{videoExt}\"", option.Debug);
            tmpFiles.Add($"{now}.{videoExt}");

            var selectedAudios = selectedTracks.Where(m => m.Type == "Audio");
            for (int i = 0; i < selectedAudios.Count(); i++)
            {
                Utils.LogColor($"{(i == 0 ? "\r\n" : "")}Extract audio track {i}...");
                var aTrack = selectedAudios.ElementAt(i);
                await Utils.RunCommandAsync(config.FFmpeg, $"-nostdin -loglevel warning -i \"{aTrack.FilePath}\" -map_metadata -1 -map 0:{aTrack.IndexOfFile} -c copy \"{now}_Audio{i}.{aTrack.Ext}\"", option.Debug);
                tmpFiles.Add($"{now}_Audio{i}.{aTrack.Ext}");
            }

            var selectedSubtitle = selectedTracks.Where(m => m.Type == "Subtitle");
            for (int i = 0; i < selectedSubtitle.Count(); i++)
            {
                Utils.LogColor($"{(i == 0 ? "\r\n" : "")}Extract subtile track {i}...");
                var sTrack = selectedSubtitle.ElementAt(i);
                await Utils.RunCommandAsync(config.FFmpeg, $"-nostdin -loglevel warning -i \"{sTrack.FilePath}\" -map_metadata -1 -map 0:{sTrack.IndexOfFile} \"{now}_Subtitle{i}.srt\"", option.Debug);
                var text = File.ReadAllText($"{now}_Subtitle{i}.srt");
                //remove font tags
                text = TagRegex().Replace(text, "$1");
                File.WriteAllText($"{now}_Subtitle{i}.srt", text, new UTF8Encoding(false));
                tmpFiles.Add($"{now}_Subtitle{i}.srt");
            }

            var tmpVideoName = "";
            if (vTrack.DolbyVison)
            {
                var tag = vTrack.DVProfile == 5 ? "dvh1flag" : "hvc1flag";

                Utils.LogColor($"\r\nMux {videoExt} to mp4...");
                tmpVideoName = $"{now}.{videoExt}.mp4";
                await Utils.RunCommandAsync(config.MP4Muxer, $"-i \"{now}.{videoExt}\" -o \"{tmpVideoName}\" --{tag} 0 --dv-profile {vTrack.DVProfile} {(vTrack.DVComId == 0 ? "" : $" --dv-bl-compatible-id {vTrack.DVComId} ")} --mpeg4-comp-brand mp42,iso6,isom,msdh,dby1 --overwrite", option.Debug);
            }
            else
            {
                tmpVideoName = $"{now}.{videoExt}";
            }

            tmpFiles.Add(tmpVideoName);

            if (selectedAudios.Any() || selectedSubtitle.Any())
                Utils.LogColor("\r\nAdd audio / subtitle to mp4...");

            //最终mp4的流序号，用于写入udta信息，跳过视频，从2开始递增
            var mp4Index = 2;
            var sb = new StringBuilder();
            //添加音频
            for (int i = 0; i < selectedAudios.Count(); i++, mp4Index++)
            {
                var track = selectedAudios.ElementAt(i);
                sb.Append($" -add \"{now}_Audio{i}.{track.Ext}#1:name=:lang={track.LangCode}:group=2{(track.Default ? "" : ":disable")}\" ");
                if (!string.IsNullOrEmpty(track.Name))
                    sb.Append($" -udta {mp4Index}:type=name:str=\"{track.Name}\" ");
                if (!string.IsNullOrEmpty(track.ExtendedLanguageTag))
                    sb.Append($" -lang {mp4Index}=\"{track.ExtendedLanguageTag}\" ");
                if (track.Delay != 0)
                    sb.Append($" -delay {mp4Index}={track.Delay} ");
            }
            //添加字幕
            for (int i = 0; i < selectedSubtitle.Count(); i++, mp4Index++)
            {
                var track = selectedSubtitle.ElementAt(i);
                sb.Append($" -add \"{now}_Subtitle{i}.srt#1:name=:lang={track.LangCode}:hdlr=sbtl:group=3{(track.Default ? "" : ":disable")}{(track.Forced ? ":txtflags=0xC0000000" : "")}\" ");
                if (!string.IsNullOrEmpty(track.Name))
                    sb.Append($" -udta {mp4Index}:type=name:str=\"{track.Name}\" ");
                if (!string.IsNullOrEmpty(track.ExtendedLanguageTag))
                    sb.Append($" -lang {mp4Index}=\"{track.ExtendedLanguageTag}\" ");
                if (track.Delay != 0)
                    sb.Append($" -delay {mp4Index}={track.Delay} ");
            }

            //添加章节信息
            var chapArg = "";
            if (!string.IsNullOrEmpty(chapterTxt))
            {
                File.WriteAllText($"{now}.txt", chapterTxt);
                tmpFiles.Add($"{now}.txt");
                chapArg = $" -chap {now}.txt ";
            }

            await Utils.RunCommandAsync(config.MP4Box, $"-inter 500 -for-test {chapArg} -noprog -add \"{tmpVideoName}#1:name=:group=1\" {sb} -brand mp42isom -ab iso6 -ab msdh -ab dby1 -itags tool=\"{tools}\":title=\"{title}\":comment=\"{comment}\":copyright=\"{copyright}\":cover=\"{cover}\" -new \"{output}\"", option.Debug);

            Utils.LogColor("\r\nClean temp files...");
            foreach (var item in tmpFiles)
            {
                if (File.Exists(item)) File.Delete(item);
            }

            var endTime = DateTime.Now;
            var dur = endTime - startTime;
            Console.WriteLine();
            Console.WriteLine($"StartTime: {startTime}");
            Console.WriteLine($"  EndTime: {endTime}");
            Console.WriteLine($"     Cost: {dur}");
        }

        private static void SetTypeIndex(List<Mediainfo> list)
        {
            var vIndex = 0;
            var aIndex = 0;
            var sIndex = 0;
            foreach (var item in list)
            {
                if (item.Type == "Video") item.IndexOfType = vIndex++;
                else if (item.Type == "Audio") item.IndexOfType = aIndex++;
                else if (item.Type == "Subtitle") item.IndexOfType = sIndex++;
            }

            //设置音频默认轨道
            if (list.Where(i => i.Type == "Audio").All(i => !i.Default))
            {
                var _i = list.FindIndex(i => i.Type == "Audio");
                if (_i != -1) { list[_i].Default = true; }
            }
            //清除多余的
            else if (list.Where(i => i.Type == "Audio").Count(i => i.Default) > 1) 
            {
                var _i = list.FindIndex(i => i.Type == "Audio" && i.Default);
                if (_i != -1 && _i + 1 < list.Count) 
                {
                    for (int i = _i + 1; i < list.Count; i++) 
                    {
                        list[i].Default = false;
                    }
                }
            }


            //清除多余的字幕默认轨道
            if (list.Where(i => i.Type == "Subtitle").Count(i => i.Default) > 1)
            {
                var _i = list.FindIndex(i => i.Type == "Subtitle" && i.Default);
                if (_i != -1 && _i + 1 < list.Count)
                {
                    for (int i = _i + 1; i < list.Count; i++)
                    {
                        list[i].Default = false;
                    }
                }
            }
        }

        private static void SetDelayFromUser(string input, List<Mediainfo> list)
        {
            if (string.IsNullOrEmpty(input)) return;

            //-delay s:0:1000
            var firstChar = input.First();
            if (firstChar == 'a' || firstChar == 'v' || firstChar == 's')
            {
                var _index = Convert.ToInt32(input[2..].Split(':').First());
                var _type = firstChar switch { 'a' => "Audio", 'v' => "Video", 's' => "Subtitle", _ => "Error" };
                if (!list.Any(l => l.Type == _type)) throw new Exception($"Can not find output type {_type}");
                if (list.Where(l => l.Type == _type).Count() <= _index) throw new Exception($"Can not find output {_type} track index: {_index}");
                foreach (var item in list)
                {
                    if (item.Type == _type && item.IndexOfType == _index)
                    {
                        item.Delay = Convert.ToInt64(input.Split(':').Last());
                        break;
                    }
                }
                return;
            }

            //-delay 0:1000
            var index = Convert.ToInt32(input.Split(':').First());
            if (list.Count <= index) throw new Exception("Can not find output track index: " + index);
            list[index].Delay = Convert.ToInt64(input.Split(':').Last());
        }

        private static void SetForcedFromUser(string input, List<Mediainfo> list)
        {
            if (string.IsNullOrEmpty(input)) return;

            //-forced s:0
            var firstChar = input.First();
            if (firstChar == 's')
            {
                var _index = Convert.ToInt32(input[2..].Split(':').First());
                var _type = "Subtitle";
                if (!list.Any(l => l.Type == _type)) throw new Exception($"Can not find output type {_type}");
                if (list.Where(l => l.Type == _type).Count() <= _index) throw new Exception($"Can not find output {_type} track index: {_index}");
                foreach (var item in list)
                {
                    if (item.Type == _type && item.IndexOfType == _index)
                    {
                        item.Forced = true;
                        break;
                    }
                }
                return;
            }

            //-forced 3
            var index = Convert.ToInt32(input);
            if (list.Count <= index) throw new Exception("Can not find output track index: " + index);
            if (list[index].Type != "Subtitle") throw new Exception("Can not set Forced for " + list[index].Type);
            list[index].Forced = true;
        }

        private static void SetDefaultFromUser(string input, List<Mediainfo> list)
        {
            if (string.IsNullOrEmpty(input)) return;

            //-default s:0
            var firstChar = input.First();
            if (firstChar == 'a' || firstChar == 's')
            {
                var _index = Convert.ToInt32(input[2..].Split(':').First());
                var _type = firstChar switch { 'a' => "Audio", 's' => "Subtitle", _ => "Error" };
                if (!list.Any(l => l.Type == _type)) throw new Exception($"Can not find output type {_type}");
                if (list.Where(l => l.Type == _type).Count() <= _index) throw new Exception($"Can not find output {_type} track index: {_index}");
                foreach (var item in list)
                {
                    if (item.Type == _type && item.IndexOfType == _index)
                        item.Default = true;
                    else if (item.Type == _type)
                        item.Default = false;
                }
                return;
            }

            //-default 3
            var index = Convert.ToInt32(input);
            if (list.Count <= index) throw new Exception("Can not find output track index: " + index);
            var type = list[index].Type;
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].Type == type && i == index)
                    list[i].Default = true;
                else if (list[i].Type == type)
                    list[i].Default = false;
            }
        }

        private static void SetMetaFromUser(string input, List<Mediainfo> list)
        {
            if (string.IsNullOrEmpty(input)) return;

            //-meta a:0:lang=:name=:elng=
            var firstChar = input.First();
            if (firstChar == 'a' || firstChar == 'v' || firstChar == 's')
            {
                var _index = Convert.ToInt32(input[2..].Split(':').First());
                var _type = firstChar switch { 'a' => "Audio", 'v' => "Video", 's' => "Subtitle", _ => "Error" };
                if (!list.Any(l => l.Type == _type)) throw new Exception($"Can not find output type {_type}");
                if (list.Where(l => l.Type == _type).Count() <= _index) throw new Exception($"Can not find output {_type} track index: {_index}");
                foreach (var item in list)
                {
                    if (item.Type == _type && item.IndexOfType == _index)
                    {
                        var parser = new ComplexParamParser(input);
                        item.LangCode = parser.GetValue("lang") ?? item.LangCode;
                        item.ExtendedLanguageTag = parser.GetValue("elng") ?? item.ExtendedLanguageTag;
                        if (item.Type != "Video")
                            item.Name = parser.GetValue("name") ?? item.Name;
                        break;
                    }
                }
                return;
            }

            //-meta 0:lang=:name=:elng=
            var index = Convert.ToInt32(input.Split(':').First());
            if (list.Count <= index) throw new Exception("Can not find output track index: " + index);
            if (list.Count > index)
            {
                var parser = new ComplexParamParser(input);
                list[index].LangCode = parser.GetValue("lang") ?? list[index].LangCode;
                list[index].ExtendedLanguageTag = parser.GetValue("elng") ?? list[index].ExtendedLanguageTag;
                if (list[index].Type != "Video")
                    list[index].Name = parser.GetValue("name") ?? list[index].Name;
            }
        }

        private static IEnumerable<Mediainfo> FilterByUserMap(string input, Dictionary<int, List<Mediainfo>> dic)
        {
            try
            {
                var arr = input.Split(':');
                if (arr.Length == 1)
                {
                    // -map 0
                    return dic[Convert.ToInt32(arr[0])];
                }
                else if (arr.Length == 2)
                {
                    // -map 0:v
                    var type = arr[1] switch
                    {
                        "a" => "Audio",
                        "v" => "Video",
                        "s" => "Subtitle",
                        _ => "Error"
                    };
                    var result = dic[Convert.ToInt32(arr[0])].Where(m => m.Type == type);
                    if (result.Any()) return result;
                    throw new Exception($"Failed to select {type} from input {arr[0]}");
                }
                else if (arr.Length == 3)
                {
                    // -map 0:a:0 
                    var type = arr[1] switch
                    {
                        "a" => "Audio",
                        "v" => "Video",
                        "s" => "Subtitle",
                        _ => "Error"
                    };
                    var result = dic[Convert.ToInt32(arr[0])].Where(m => m.Type == type);
                    if (result.Any() && result.Count() > Convert.ToInt32(arr[2])) 
                    {
                        result = new List<Mediainfo>() { result.ElementAt(Convert.ToInt32(arr[2])) };
                    }
                    if (result.Any()) return result;
                    throw new Exception($"Failed to select {type}({arr[2]}) from input {arr[0]}, ");
                }
                throw new Exception(input);
            }
            catch (Exception ex)
            {
                throw new Exception("Bad user map! " + ex.Message);
            }
        }

        private static void CheckVideo(IEnumerable<Mediainfo> list, out Mediainfo video)
        {
            var vTracks = list.Where(m => m.Type == "Video");
            var vCount = vTracks.Count();
            var vTrack = vTracks.FirstOrDefault();
            if (vCount > 1)
            {
                throw new Exception("Up to 1 video track!");
            }
            else if (vTrack == null)
            {
                throw new Exception("Must input video track!");
            }
            if (vTrack.DolbyVison && vTrack.DVProfile != 5 && vTrack.DVProfile != 8)
            {
                throw new Exception("Must input dolby vision (P5 or P8) track!");
            }
            video = vTrack;
        }
    }
}