using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DoViMuxer
{
    internal class Chapter
    {
        public double Start { get; set; }
        public double End { get; set; }
        public string Title { get; set; }

        public override string ToString()
        {
            return $"{FormatTime(Start)} {Title}";
        }

        private static string FormatTime(double time)
        {
            TimeSpan ts = new(0, 0, (int)time);
            string str = ts.Hours.ToString("00") + ":" + ts.Minutes.ToString("00") + ":" + ts.Seconds.ToString("00");
            return str;
        }
    }
}
