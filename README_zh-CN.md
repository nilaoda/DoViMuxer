### [English](README.md) | 简体中文

---

# DoViMuxer
一个用于制作杜比视界 mp4 的命令行工具.

# 功能
* 支持从各种各样的输入文件中选择轨道  (mp4, ts, mkv, hevc, aac, eac3...)
* 支持从输入文件中复制元数据和轨道延迟
* 支持为输出文件的每个轨道设置不同的语言和标题
* 支持设置全局元数据 (封面, 标题, 版权, 注释, 编码工具)
* 支持输入 SDR / HDR10 / 杜比视界 Profile 5 或 Profile 8
* **支持在苹果设备上原生播放** (ATV, iOS, iPadOS, macOS)

# 依赖的外部软件
* [ffmpeg](https://ffmpeg.org/download.html) (>=5.0)  
    读取轨道信息, 抽取轨道
* [mp4box](https://gpac.wp.imt.fr/downloads/gpac-nightly-builds/) (>=2.1)  
    混流视频、音轨和字幕
* [mediainfo](https://mediaarea.net/download/snapshots/binary/mediainfo/) (latest CLI version)  
    读取元数据
* [mp4muxer](https://github.com/DolbyLaboratories/dlb_mp4base/tree/master/bin)  
    将 ES 流封装到 mp4

**请确保你的软件版本满足要求, 否则可能无法生成正确的文件!**

# 工作流
1. 使用 ffmpeg 抽取 ES 流
2. 使用 mp4muxer 混流 mp4
3. 使用 mp4box 添加音轨和字幕

# 元数据
现在 DoViMuxer 可以识别或设置:
* 全局 标题
* 全局 版权 
* 全局 描述
* 全局 编码工具
* 轨道 标题
* 轨道 语言 ([ISO 639-2](https://www.loc.gov/standards/iso639-2/php/code_list.php))
* 轨道 扩展语言标记 ([RFC 4646 language 标签](https://datatracker.ietf.org/doc/rfc4646/), 可以在 [unicode.org](http://unicode.org/reports/tr35/#Unicode_Language_and_Locale_Identifiers) 查看详细信息)

# 命令行选项
```
Description:
  DoViMuxer. 一个用于制作杜比视界 mp4 的命令行工具.

Usage:
  DoViMuxer [<output>] [options]

Arguments:
  <output>  输出文件名 []

Options:
  -i <FILE> (REQUIRED)            添加输入文件
  -map <file[:type[:index]]>      轨道选择和重排序. 例如:
                                    -map 0:0   输入 0, 轨道 0
                                    -map 0:a:0 输入 0, 第0个音频轨道
  -meta <[type:]index:key=value>  设置 mp4 的轨道元数据. 例如:
                                    -meta a:0:lang=eng:name="English (Original)":elng="en-US"
                                    -meta 1:lang=jpn
                                  注意: lang: ISO 639-2, elng: RFC 4646 tags
  -delay <[type:]index:time>      设置 mp4 的轨道延迟 (毫秒). Example:
                                    -delay a:0:-5000
                                    -delay s:0:1000
  -forced <[s:]index>             设置 mp4 某字幕轨道为 [Forced]. Example:
                                    -forced s:3
  -default <[type:]index>         设置 mp4 某音频或字幕轨道为 [Default]. Example:
  -cover <FILE>                   设置 mp4 的封面图片
  -comment <comment>              设置 mp4 的注释
  -copyright <copyright>          设置 mp4 的版权
  -title <title>                  设置 mp4 的标题
  -tool <tool>                    设置 mp4 的编码工具
  -ffmpeg <FILE>                  设置 ffmpeg 路径
  -mp4box <FILE>                  设置 mp4box 路径
  -mp4muxer <FILE>                设置 mp4muxer 路径
  -mediainfo <FILE>               设置 mediainfo 路径
  -y                              覆盖文件 [default: False]
  -dvhe                           优先使用 'dvhe' 而不是 'dvh1' [default: False]
  -hev1                           优先使用 'hev1' 而不是 'hvc1' [default: False]
  --nochap                        跳过章节读取 [default: False]
  --debug                         展示详细信息 [default: False]
  --version                       Show version information
  -?, -h, --help                  Show help and usage information
```

# 示例
仅读取信息:
```
DoViMuxer -i source.mp4 -i source2.eac3
```

将 杜比视界 `mkv` 封装到 `mp4` 并保留元数据:
```
DoViMuxer -i source.mkv output.mp4
```

将 杜比视界 `ts` 封装到 `mp4` 并保留元数据:
```
DoViMuxer -i source.ts output.mp4
```

进阶示例:
```
DoviMuxer ^
-i v.mp4 ^
-i a.eac3 ^
-i zh.srt ^
-i zh-TW.srt ^
-i zh-HK.srt ^
-i en.srt ^
-meta a:0:lang=eng:name="English":elng="en-US" ^
-meta s:0:lang=chi:name="简体中文":elng="zh-Hans" ^
-meta s:1:lang=chi:name="繁体中文 (台湾)":elng="zh-Hant-TW" ^
-meta s:2:lang=chi:name="繁体中文 (香港)":elng="zh-Hant-HK" ^
-meta s:3:lang=chi:name="English":elng="en-US" ^
-title "DoVi EP01" -comment "测试" -tool "DoviMuxer v1.0.0" output.mp4
```

# RFC 4646 language tag
在苹果设备上, `elng` 决定了播放器将如何展示你的轨道名称.

标签列表: https://www.iana.org/assignments/language-subtag-registry/language-subtag-registry

一些常用标签:

|  Elng   | 显示名称 (简体中文系统)  |
|  ----  | ----  |
| zh-CN  | 中文 |
| zh-Hans  | 简体中文 |
| zh-Hant  | 繁体中文 |
| zh-Hant-TW  | 繁体中文（台湾） |
| zh-Hant-HK  | 繁体中文（香港） |
| en-US  | 英文 |
| ...  | ... |
| ...  | ... |

你可以利用一些技巧来让播放器更好的显示轨道名称. 例如, 同一语言的不同的音频编码 (`Dolby Digital Plus` and `AAC`) 在默认情况下没办法区分, 可以这样设置`elng`:
```
elng=zh-DDP => 中文（DDP）
elng=zh-AAC => 中文（AAC）
```

# 苹果设备的适配
* HEVC 标记 必须是 `hvc1` 或 `dvh1`
* 字幕文本 `hdlr` 设置为 `sbtl` 模式
* 视频、音频和字幕应当在 **不同** 的组别
* 显示名称取决于拓展标记语言, 而非 Window 上大多数播放器那样读取 `udta` 的 `name` 属性.

# Todo
* [ ] 适配双层杜比视界