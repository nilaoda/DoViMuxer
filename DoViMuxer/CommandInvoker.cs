using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Binding;
using System.CommandLine.Builder;
using System.CommandLine.Parsing;
using System.Globalization;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;

namespace DoViMuxer
{
    internal class CommandInvoker
    {
        private readonly static Argument<string> Output = new(name: "output", description: "File output name", getDefaultValue: () => "");
        private readonly static Option<List<string>> Input = new(new string[] { "-i" }, description: "Add input(s)") { IsRequired = true, Arity = ArgumentArity.OneOrMore, AllowMultipleArgumentsPerToken = false, ArgumentHelpName = "FILE" };
        private readonly static Option<bool> Debug = new(new string[] { "--debug" }, description: "Show details", getDefaultValue: () => false);
        private readonly static Option<bool> NoChap = new(new string[] { "--nochap" }, description: "Skip chapters copy", getDefaultValue: () => false);
        private readonly static Option<bool> DvheFlag = new(new string[] { "-dvhe" }, description: "Prefer 'dvhe' rather than 'dvh1'", getDefaultValue: () => false);
        private readonly static Option<bool> Hev1Flag = new(new string[] { "-hev1" }, description: "Prefer 'hev1' rather than 'hvc1'", getDefaultValue: () => false);
        private readonly static Option<bool> Yes = new(new string[] { "-y" }, description: "Overwrite", getDefaultValue: () => false);
        private readonly static Option<List<string>?> Maps = new(new string[] { "-map" }, description: "Select and re-order tracks. Example:\r\n  -map 0:0   Input 0, track 0\r\n  -map 0:a:0 Input 0, first audio track") { Arity = ArgumentArity.OneOrMore, AllowMultipleArgumentsPerToken = false, ArgumentHelpName = "file[:type[:index]]" };
        private readonly static Option<string?> Cover = new(new string[] { "-cover" }, description: "Set mp4 cover image") { ArgumentHelpName = "FILE" };
        private readonly static Option<string?> Comment = new(new string[] { "-comment" }, description: "Set mp4 comment");
        private readonly static Option<string?> Copyright = new(new string[] { "-copyright" }, description: "Set mp4 copyright");
        private readonly static Option<string?> Title = new(new string[] { "-title" }, description: "Set mp4 title");
        private readonly static Option<string?> Tool = new(new string[] { "-tool" }, description: "Set mp4 encoding tool");
        private readonly static Option<List<string>?> Metas = new(new string[] { "-meta" }, description: "Set mp4 track metadata. Example:\r\n  -meta a:0:lang=eng:name=\"English (Original)\":elng=\"en-US\"\r\n  -meta 1:lang=jpn\r\nnote: lang: ISO 639-2, elng: RFC 4646 tags") { Arity = ArgumentArity.OneOrMore, AllowMultipleArgumentsPerToken = false, ArgumentHelpName = "[type:]index:key=value" };
        private readonly static Option<List<string>?> Delays = new(new string[] { "-delay" }, description: "Set mp4 track delay (milliseconds). Example:\r\n  -delay a:0:-5000\r\n  -delay s:0:1000") { Arity = ArgumentArity.OneOrMore, AllowMultipleArgumentsPerToken = false, ArgumentHelpName = "[type:]index:time" };
        private readonly static Option<List<string>?> Forceds = new(new string[] { "-forced" }, description: "Set mp4 subtitle track to [Forced]. Example:\r\n  -forced s:3") { Arity = ArgumentArity.OneOrMore, AllowMultipleArgumentsPerToken = false, ArgumentHelpName = "[s:]index" };
        private readonly static Option<List<string>?> Defaults = new(new string[] { "-default" }, description: "Set mp4 audio or subtitle track to [Default]. Example:\r\n  -default s:3\r\n  -default a:1") { Arity = ArgumentArity.OneOrMore, AllowMultipleArgumentsPerToken = false, ArgumentHelpName = "[type:]index" };
        private readonly static Option<string?> FFmpeg = new(new string[] { "-ffmpeg" }, description: "Set ffmpeg path") { ArgumentHelpName = "FILE"};
        private readonly static Option<string?> MP4Box = new(new string[] { "-mp4box" }, description: "Set mp4box path") { ArgumentHelpName = "FILE" };
        private readonly static Option<string?> MP4Muxer = new(new string[] { "-mp4muxer" }, description: "Set mp4muxer path") { ArgumentHelpName = "FILE" };
        private readonly static Option<string?> Mediainfo = new(new string[] { "-mediainfo" }, description: "Set mediainfo path") { ArgumentHelpName = "FILE" };

        class MyOptionBinder : BinderBase<MyOption>
        {
            protected override MyOption GetBoundValue(BindingContext bindingContext)
            {
                var option = new MyOption
                {
                    Output = bindingContext.ParseResult.GetValueForArgument(Output),
                    Inputs = bindingContext.ParseResult.GetValueForOption(Input)!,
                    Debug = bindingContext.ParseResult.GetValueForOption(Debug),
                    NoChap = bindingContext.ParseResult.GetValueForOption(NoChap),
                    Yes = bindingContext.ParseResult.GetValueForOption(Yes),
                    Maps = bindingContext.ParseResult.GetValueForOption(Maps),
                    Cover = bindingContext.ParseResult.GetValueForOption(Cover),
                    Comment = bindingContext.ParseResult.GetValueForOption(Comment),
                    Copyright = bindingContext.ParseResult.GetValueForOption(Copyright),
                    Title = bindingContext.ParseResult.GetValueForOption(Title),
                    Tool = bindingContext.ParseResult.GetValueForOption(Tool),
                    Metas = bindingContext.ParseResult.GetValueForOption(Metas),
                    FFmpeg = bindingContext.ParseResult.GetValueForOption(FFmpeg),
                    MP4Box = bindingContext.ParseResult.GetValueForOption(MP4Box),
                    MP4Muxer = bindingContext.ParseResult.GetValueForOption(MP4Muxer),
                    Mediainfo = bindingContext.ParseResult.GetValueForOption(Mediainfo),
                    Delays = bindingContext.ParseResult.GetValueForOption(Delays),
                    Forceds = bindingContext.ParseResult.GetValueForOption(Forceds),
                    Defaults = bindingContext.ParseResult.GetValueForOption(Defaults),
                    DvheFlag = bindingContext.ParseResult.GetValueForOption(DvheFlag),
                    Hev1Flag = bindingContext.ParseResult.GetValueForOption(Hev1Flag),
                };

                return option;
            }
        }

        public static async Task<int> InvokeArgs(string[] args, Func<MyOption, Task> action)
        {
            var rootCommand = new RootCommand("DoViMuxer. Tool to make Dolby Vison mp4.")
            {
                Input, Output, Maps, Metas, Delays, Forceds, Defaults,
                Cover, Comment, Copyright, Title, Tool,
                FFmpeg, MP4Box, MP4Muxer, Mediainfo,
                Yes, DvheFlag, Hev1Flag, NoChap, Debug
            };

            rootCommand.TreatUnmatchedTokensAsErrors = true;
            rootCommand.SetHandler(async (myOption) => await action(myOption), new MyOptionBinder());

            var parser = new CommandLineBuilder(rootCommand)
                .UseDefaults()
                .EnablePosixBundling(false)
                .UseExceptionHandler((e, context) =>
                {
                    Utils.LogError(e.Message);
                }, 1)
                .Build();

            return await parser.InvokeAsync(args);
        }
    }
}
