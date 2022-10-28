# DoViMuxer
Command line tool to make Dolby Vison mp4.

# Features
* Select tracks from variety media files (mp4, ts, mkv, hevc, aac, eac3...)
* Keep metadata / track delay from source file
* Set language and title for each track
* Set global metadata (cover, title, copyright, comment, encoding tool)
* Accepted Dolby Vision Profile 5 or Profile 8
* **Compatible with Apple devices** (ATV, iOS, iPadOS, macOS)

# Requirements
* [ffmpeg](https://ffmpeg.org/download.html) (>=5.0)  
    Read track info, extract stream
* [mp4box](https://gpac.wp.imt.fr/downloads/gpac-nightly-builds/) (>=2.1)  
    Mux video, audio, subtitle together
* [mediainfo](https://mediaarea.net/en/MediaInfo/Download) (latest CLI version)  
    Read metadata
* [mp4muxer](https://github.com/DolbyLaboratories/dlb_mp4base/tree/master/bin)  
    Mux ES to DoVi mp4

**Please ensure the version of these software, or the output may not be correct!**

# Workflow
1. Use ffmpeg to extract ES stream
2. Use mp4muxer to make DoVi mp4
3. Use mp4box to make final mp4

# Metadata
Now, DoViMuxer can read/set:
* Global Title
* Global Copyright 
* Global Comment
* Global EncodingTool
* Track Name
* Track Language ([ISO 639-2](https://www.loc.gov/standards/iso639-2/php/code_list.php))
* Track Extended Language Tag ([RFC 4646 language tag](https://datatracker.ietf.org/doc/rfc4646/), Details on [unicode.org](http://unicode.org/reports/tr35/#Unicode_Language_and_Locale_Identifiers))

# Command Line
```
Description:
  DoViMuxer v1.0. Tool to make Dolby Vison mp4.

Usage:
  DoViMuxer [<output>] [options]

Arguments:
  <output>  File output name []

Options:
  -i <FILE> (REQUIRED)            Add input(s)
  -map <file[:type[:index]]>      Select and re-order tracks. Example:
                                    -map 0:0   Input 0, track 0
                                    -map 0:a:0 Input 0, first audio track
  -meta <[type:]index:key=value>  Set mp4 track metadata to output file. Example:
                                    -meta a:0:lang=eng:name="English (Original)":elng="en-US"
                                    -meta 1:lang=jpn
                                  note: lang: ISO 639-2, elng: RFC 4646 tags
  -delay <[type:]index:time>      Set mp4 track delay (milliseconds) to output file. Example:
                                    -delay a:0:-5000
                                    -delay s:0:1000
  -cover <FILE>                   Set mp4 conver image
  -comment <comment>              Set mp4 comment
  -copyright <copyright>          Set mp4 copyright
  -title <title>                  Set mp4 title
  -tool <tool>                    Set mp4 encoding tool
  -ffmpeg <FILE>                  Set ffmpeg path
  -mp4box <FILE>                  Set mp4box path
  -mp4muxer <FILE>                Set mp4muxer path
  -mediainfo <FILE>               Set mediainfo path
  -y                              Overwrite [default: False]
  --debug                         Show details [default: False]
  --version                       Show version information
  -?, -h, --help                  Show help and usage information
```

# Examples
Read info:
```
DoViMuxer -i source.mp4 -i source2.eac3
```

Remux DoVi `mkv` to `mp4` and keep meta:
```
DoViMuxer -i source.mkv output.mp4
```

Remux DoVi `ts` to `mp4` and keep meta:
```
DoViMuxer -i source.ts output.mp4
```

Advance use:
```
DoviMuxer ^
-i v.mp4 ^
-i a.eac3 ^
-i zh.srt ^
-i zh-TW.srt ^
-i zh-HK.srt ^
-i en.srt ^
-meta a:0:lang=eng:name="English":elng="en-US" ^
-meta s:0:lang=chi:name="Chinese Simplified":elng="zh-CN" ^
-meta s:1:lang=chi:name="Chinese Traditional (TW)":elng="zh-TW" ^
-meta s:2:lang=chi:name="Chinese Traditional (HK)":elng="zh-HK" ^
-meta s:3:lang=chi:name="English":elng="en-US" ^
-title "DoVi EP01" -comment "This a Test" -tool "DoviMuxer v1.0.0" output.mp4
```

# Apple Devices Tips
* HEVC tag must be `hvc1` or `dvh1`
* Timed text `hdlr` set to `sbtl`
* Video tracks, audio tracks and subtitle tracks in **different** alternate group
* Show name depends on extended language tag, not `udta` name.

# Todo
* [ ] Dual layer DoVi
* [ ] Chapters copy 