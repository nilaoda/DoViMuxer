using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DoViMuxer
{
    internal class MyOption
    {
        /// <summary>
        /// See: <see cref="CommandInvoker.Input"/>.
        /// </summary>
        public required List<string> Inputs { get; set; }
        /// <summary>
        /// See: <see cref="CommandInvoker.Maps"/>.
        /// </summary>
        public List<string>? Maps { get; set; }
        /// <summary>
        /// See: <see cref="CommandInvoker.Metas"/>.
        /// </summary>
        public List<string>? Metas { get; set; }
        /// <summary>
        /// See: <see cref="CommandInvoker.Delays"/>.
        /// </summary>
        public List<string>? Delays { get; set; }
        /// <summary>
        /// See: <see cref="CommandInvoker.Output"/>.
        /// </summary>
        public string? Output { get; set; }
        /// <summary>
        /// See: <see cref="CommandInvoker.Debug"/>.
        /// </summary>
        public bool Debug { get; set; }
        /// <summary>
        /// See: <see cref="CommandInvoker.Yes"/>.
        /// </summary>
        public bool Yes { get; set; }
        /// <summary>
        /// See: <see cref="CommandInvoker.Title"/>.
        /// </summary>
        public string? Title { get; set; }
        /// <summary>
        /// See: <see cref="CommandInvoker.Cover"/>.
        /// </summary>
        public string? Cover { get; set; }
        /// <summary>
        /// See: <see cref="CommandInvoker.Comment"/>.
        /// </summary>
        public string? Comment { get; set; }
        /// <summary>
        /// See: <see cref="CommandInvoker.Tool"/>.
        /// </summary>
        public string? Tool { get; set; }
        /// <summary>
        /// See: <see cref="CommandInvoker.Copyright"/>.
        /// </summary>
        public string? Copyright { get; set; }
        /// <summary>
        /// See: <see cref="CommandInvoker.FFmpeg"/>.
        /// </summary>
        public string? FFmpeg { get; set; }
        /// <summary>
        /// See: <see cref="CommandInvoker.MP4Box"/>.
        /// </summary>
        public string? MP4Box { get; set; }
        /// <summary>
        /// See: <see cref="CommandInvoker.MP4Muxer"/>.
        /// </summary>
        public string? MP4Muxer { get; set; }
        /// <summary>
        /// See: <see cref="CommandInvoker.Mediainfo"/>.
        /// </summary>
        public string? Mediainfo { get; set; }
    }
}
