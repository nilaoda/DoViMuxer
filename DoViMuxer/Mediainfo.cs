﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
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

        public int IndexOfFile { get; set; } = -1;//它在所属文件中的index
        public int IndexOfType { get; set; } = -1;//它类型的index 第x条音轨 （在list中有意义）
        public string? FilePath { get; set; }
        public int DVProfile { get; set; } = -1;
        public int DVComId { get; set; } = 0;
        public string? ExtendedLanguageTag { get; set; } //扩展语言标签 en-US BCP-47 tags RFC 4646
        public string? LangCode { get; set; } //ISO 639-2
        public string? Name { get; set; }
        public long Delay { get; set; } = 0;
        public bool Default { get; set; } = false;
        public bool Forced { get; set; } = false;

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
        public bool H264 { get; set; }


        public override string? ToString()
        {
            return $"[{IndexOfFile}]: " + ToShortString();
        }

        public string? ToShortString()
        {
            return string.Join(", ", new List<string?> { Type, BaseInfo, Resolution, Fps, Bitrate, LangCode, ExtendedLanguageTag, Name }.Where(i => !string.IsNullOrEmpty(i)))
                + (Delay == 0 ? "" : $", Delay: {Delay}ms")
                + (Forced ? " [Forced]" : "")
                + (Type != "Video" && Default ? " [Default]" : "");
        }

        public override bool Equals(object? obj)
        {
            return obj is Mediainfo mediainfo &&
                   GlobalTitle == mediainfo.GlobalTitle &&
                   GlobalCopyright == mediainfo.GlobalCopyright &&
                   GlobalComment == mediainfo.GlobalComment &&
                   GlobalEncodingTool == mediainfo.GlobalEncodingTool &&
                   GlobalHasCover == mediainfo.GlobalHasCover &&
                   IndexOfFile == mediainfo.IndexOfFile &&
                   IndexOfType == mediainfo.IndexOfType &&
                   FilePath == mediainfo.FilePath &&
                   DVProfile == mediainfo.DVProfile &&
                   DVComId == mediainfo.DVComId &&
                   ExtendedLanguageTag == mediainfo.ExtendedLanguageTag &&
                   LangCode == mediainfo.LangCode &&
                   Name == mediainfo.Name &&
                   Delay == mediainfo.Delay &&
                   Default == mediainfo.Default &&
                   Forced == mediainfo.Forced &&
                   Id == mediainfo.Id &&
                   Text == mediainfo.Text &&
                   BaseInfo == mediainfo.BaseInfo &&
                   Ext == mediainfo.Ext &&
                   Bitrate == mediainfo.Bitrate &&
                   Resolution == mediainfo.Resolution &&
                   Fps == mediainfo.Fps &&
                   Type == mediainfo.Type &&
                   DolbyVison == mediainfo.DolbyVison &&
                   HDR == mediainfo.HDR &&
                   H264 == mediainfo.H264;
        }

        public override int GetHashCode()
        {
            HashCode hash = new HashCode();
            hash.Add(GlobalTitle);
            hash.Add(GlobalCopyright);
            hash.Add(GlobalComment);
            hash.Add(GlobalEncodingTool);
            hash.Add(GlobalHasCover);
            hash.Add(IndexOfFile);
            hash.Add(IndexOfType);
            hash.Add(FilePath);
            hash.Add(DVProfile);
            hash.Add(DVComId);
            hash.Add(ExtendedLanguageTag);
            hash.Add(LangCode);
            hash.Add(Name);
            hash.Add(Delay);
            hash.Add(Default);
            hash.Add(Forced);
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
            hash.Add(H264);
            return hash.ToHashCode();
        }

        public Mediainfo() { }
        public Mediainfo(Mediainfo m)
        {
            GlobalTitle = m.GlobalTitle;
            GlobalCopyright = m.GlobalCopyright;
            GlobalComment = m.GlobalComment;
            GlobalEncodingTool = m.GlobalEncodingTool;
            GlobalHasCover = m.GlobalHasCover;
            IndexOfFile = m.IndexOfFile;
            IndexOfType = m.IndexOfType;
            FilePath = m.FilePath;
            DVProfile = m.DVProfile;
            DVComId = m.DVComId;
            ExtendedLanguageTag = m.ExtendedLanguageTag;
            LangCode = m.LangCode;
            Name = m.Name;
            Delay = m.Delay;
            Default = m.Default;
            Forced = m.Forced;
            Id = m.Id;
            Text = m.Text;
            BaseInfo = m.BaseInfo;
            Bitrate = m.Bitrate;
            Resolution = m.Resolution;
            Fps = m.Fps;
            Type = m.Type;
            DolbyVison = m.DolbyVison;
            HDR = m.HDR;
            H264 = m.H264;
        }
    }
}
