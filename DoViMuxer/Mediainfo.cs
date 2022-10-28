using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DoViMuxer
{
    internal class Mediainfo
    {
        public string? GlobalTitle { get; set; }
        public string? GlobalCopyright { get; set; }
        public string? GlobalComment { get; set; }
        public string? GlobalEncodingTool { get; set; }
        public bool GlobalHasCover { get; set; } = false;

        public int Index { get; set; }
        public string? FilePath { get; set; }
        public int DVProfile { get; set; }
        public int DVComId { get; set; } = 0;
        public string? ExtendedLanguageTag { get; set; } //扩展语言标签 en-US BCP-47 tags RFC 4646
        public string? LangCode { get; set; } //ISO 639-2
        public string? Name { get; set; }
        public long Delay { get; set; } = 0;

        public string? Id { get; set; }
        public string? Text { get; set; }
        public string? BaseInfo { get; set; }
        public string? Ext
        {
            get => BaseInfo?.Split(' ')[0];
        }
        public string? Bitrate { get; set; }
        public string? Resolution { get; set; }
        public string? Fps { get; set; }
        public string? Type { get; set; }
        public bool DolbyVison { get; set; }
        public bool HDR { get; set; }


        public override string? ToString()
        {
            return $"[{Index}]: " + ToShortString();
        }

        public string? ToShortString()
        {
            return string.Join(", ", new List<string?> { Type, BaseInfo, Resolution, Fps, Bitrate, LangCode, ExtendedLanguageTag, Name }.Where(i => !string.IsNullOrEmpty(i))) + (Delay==0?"": $", Delay: {Delay}ms");
        }

        public override bool Equals(object? obj)
        {
            return obj is Mediainfo mediainfo &&
                   GlobalTitle == mediainfo.GlobalTitle &&
                   GlobalCopyright == mediainfo.GlobalCopyright &&
                   GlobalComment == mediainfo.GlobalComment &&
                   GlobalEncodingTool == mediainfo.GlobalEncodingTool &&
                   Index == mediainfo.Index &&
                   FilePath == mediainfo.FilePath &&
                   DVProfile == mediainfo.DVProfile &&
                   DVComId == mediainfo.DVComId &&
                   LangCode == mediainfo.LangCode &&
                   ExtendedLanguageTag == mediainfo.ExtendedLanguageTag &&
                   Name == mediainfo.Name &&
                   Id == mediainfo.Id &&
                   Text == mediainfo.Text &&
                   BaseInfo == mediainfo.BaseInfo &&
                   Ext == mediainfo.Ext &&
                   Bitrate == mediainfo.Bitrate &&
                   Resolution == mediainfo.Resolution &&
                   Fps == mediainfo.Fps &&
                   Type == mediainfo.Type &&
                   DolbyVison == mediainfo.DolbyVison &&
                   HDR == mediainfo.HDR;
        }

        public override int GetHashCode()
        {
            HashCode hash = new HashCode();
            hash.Add(GlobalTitle);
            hash.Add(GlobalCopyright);
            hash.Add(GlobalComment);
            hash.Add(GlobalEncodingTool);
            hash.Add(Index);
            hash.Add(FilePath);
            hash.Add(DVProfile);
            hash.Add(DVComId);
            hash.Add(LangCode);
            hash.Add(ExtendedLanguageTag);
            hash.Add(Name);
            hash.Add(Id);
            hash.Add(Text);
            hash.Add(BaseInfo);
            hash.Add(Ext);
            hash.Add(Bitrate);
            hash.Add(Resolution);
            hash.Add(Fps);
            hash.Add(Type);
            hash.Add(DolbyVison);
            hash.Add(HDR);
            return hash.ToHashCode();
        }
    }
}
