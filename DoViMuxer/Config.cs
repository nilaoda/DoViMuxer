using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace DoViMuxer
{
    internal class Config
    {
        public string MP4Box { get; set; } = "mp4box";
        public string FFmpeg { get; set; } = "ffmpeg";
        public string MP4Muxer { get; set; } = "mp4muxer";
        public string Mediainfo { get; set; } = "Mediainfo";
    }
}
