using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
// using System.Windows.Forms;
// using CommandLine;
// using CommandLine.Text;
using LibSidWiz;
using LibSidWiz.Outputs;
using LibSidWiz.Triggers;
using LibVgm;
using Channel = LibSidWiz.Channel;

namespace SidWizPlus
{
    class Program
    {
        public WaveformRenderer Renderer { get; set; }


        public void Main()
        {
            try
            {
                var settings = new Settings()
                {
                    MultidumperPath = @"C:\repo\SidWizPlus\SidWizPlus\bin\Debug\multidumper.exe",
                    VgmFile = @"C:\repo\SidWizPlus\SidWizPlus\bin\Debug\sonic.vgz"
                };

                // ReSharper disable once RedundantNameQualifier
                // using (var parser = new CommandLine.Parser(x =>
                // {
                //     x.CaseSensitive = false;
                //     x.IgnoreUnknownArguments = true;
                // }))
                // {
                //     if (!parser.ParseArguments(args, settings))
                //     {
                //         Console.Error.WriteLine(settings.GetUsage());
                //         return;
                //     }
                // }
    
                if (!settings.YouTubeOnly)
                {
                    if (settings.InputFiles == null)
                    {
                        RunMultiDumper(ref settings);
                    }
                    else
                    {
                        // We want to expand any wildcards in the input file list (and also fully qualify them)
                        var inputs = new List<string>();
                        foreach (var inputFile in settings.InputFiles)
                        {
                            if (File.Exists(inputFile))
                            {
                                inputs.Add(Path.GetFullPath(inputFile));
                                continue;
                            }
    
                            var pattern = Path.GetFileName(inputFile);
                            if (pattern == null)
                            {
                                throw new Exception($"Failed to match {inputFile}");
                            }
                            var pathPart = inputFile.Substring(0, inputFile.Length - pattern.Length);
                            var directory = pathPart.Length > 0
                                ? Path.GetFullPath(pathPart)
                                : Directory.GetCurrentDirectory();
                            var files = Directory.EnumerateFiles(directory, pattern).ToList();
                            if (files.Count == 0)
                            {
                                throw new Exception($"Failed to match {inputFile}");
                            }
                            inputs.AddRange(files.OrderByAlphaNumeric(x => x));
                        }
    
                        settings.InputFiles = inputs;
                    }
    
                    if (settings.InputFiles == null || !settings.InputFiles.Any())
                    {
                        //Console.Error.WriteLine(settings.GetUsage());
                        throw new Exception("No inputs specified");
                    }
    
                    var channels = settings.InputFiles
                        .AsParallel()
                        .Select(filename =>
                    {
                        var channel = new Channel
                        {
                            Filename = filename,
                            LineColor = ParseColor(settings.LineColor),
                            LineWidth =  settings.LineWidth,
                            FillColor = ParseColor(settings.FillColor),
                            Label = Channel.GuessNameFromMultidumperFilename(filename),
                            Algorithm = CreateTriggerAlgorithm(settings.TriggerAlgorithm),
                            TriggerLookaheadFrames = settings.TriggerLookahead,
                            ZeroLineWidth = settings.ZeroLineWidth,
                            ZeroLineColor = ParseColor(settings.ZeroLineColor),
                            LabelFont = settings.ChannelLabelsFont == null
                                ? null
                                : new Font(settings.ChannelLabelsFont, settings.ChannelLabelsSize),
                            LabelColor = ParseColor(settings.ChannelLabelsColor),
                            HighPassFilter = settings.HighPass
                        };
                        channel.LoadDataAsync().Wait();
                        channel.ViewWidthInMilliseconds = settings.ViewWidthMs;
                        return channel;
                    }).Where(ch => ch.SampleCount > 0).ToList();
    
                    if (settings.AutoScalePercentage > 0)
                    {
                        float max;
                        
                        if (settings.AutoScaleIgnoreYM2413Percussion)
                        {
                            var channelsToUse = channels.Where(channel => !(channel.Label.StartsWith("YM2413 ") && !channel.Label.StartsWith("YM2413 Tone"))).ToList();
                            if (channelsToUse.Count == 0)
                            {
                                // Fall back on overall max if all channels are percussion
                                max = channels.Max(ch => ch.Max);
                            }
                            else
                            {
                                max = channelsToUse.Max(ch => ch.Max);
                            }
                        }
                        else
                        {
                            max = channels.Max(ch => ch.Max);
                        }
                        var scale = settings.AutoScalePercentage / 100 / max;
                        foreach (var channel in channels)
                        {
                            channel.Scale = scale;
                        }
                    }
    
                    if (settings.ChannelLabelsFromVgm && settings.VgmFile != null)
                    {
                        TryGuessLabelsFromVgm(channels, settings.VgmFile);
                    }
    
                    if (settings.OutputFile != null)
                    {
                        // Emit normalized data to a WAV file for later mixing
                        if (settings.MasterAudioFile == null && !settings.NoMasterMix)
                        {
                            settings.MasterAudioFile = settings.OutputFile + ".wav";
                            //Mixer.MixToFile(channels, settings.MasterAudioFile, !settings.NoMasterMixReplayGain);
                        }
                    }
    
                    Render(settings, channels);
                    //
                    // foreach (var channel in channels)
                    // {
                    //     channel.Dispose();
                    // }
                }
    
            }
            catch (Exception e)
            {
                Console.Error.WriteLine($"Fatal: {e}");
            }
        }
    
        private class InstrumentState
        {
            public int Instrument { private get; set; }
            public int Ticks { get; private set; }
    
            private static readonly string[] Names = {
                "Custom Instrument",
                "Violin",
                "Guitar",
                "Piano",
                "Flute",
                "Clarinet",
                "Oboe",
                "Trumpet",
                "Organ",
                "Horn",
                "Synthesizer",
                "Harpsichord",
                "Vibraphone",
                "Synthesizer Bass",
                "Acoustic Bass",
                "Electric Guitar",
            };
    
            public string Name => Names[Instrument];
    
            public override string ToString() => $"{Name} ({TimeSpan.FromSeconds((double)Ticks / 44100)})";
    
            public void AddTime(int ticks)
            {
                Ticks += ticks;
            }
        }
    
        private class ChannelState
        {
            private readonly List<InstrumentState> _instruments = new List<InstrumentState>();
            private readonly Dictionary<int, InstrumentState> _instrumentsByChannel = new Dictionary<int, InstrumentState>();
            private InstrumentState _currentInstrument;
            public bool KeyDown { private get; set; }
    
            public void SetInstrument(int instrument)
            {
                InstrumentState state;
                if (!_instrumentsByChannel.TryGetValue(instrument, out state))
                {
                    state = new InstrumentState {Instrument = instrument};
                    _instruments.Add(state);
                    _instrumentsByChannel.Add(instrument, state);
                }
    
                _currentInstrument = state;
            }
    
            public void AddTime(int ticks)
            {
                if (KeyDown)
                {
                    _currentInstrument?.AddTime(ticks);
                }
            }
    
            public IEnumerable<InstrumentState> Instruments => _instruments;
    
            public override string ToString() => string.Join(", ", Instruments.Where(x => x.Ticks > 0));
        }

        private static ChannelState GetChannelState(int channelIndex, Dictionary<int, ChannelState> channelStates)
        {
            ChannelState channelState;
            if (!channelStates.TryGetValue(channelIndex, out channelState))
            {
                channelState = new ChannelState();
                channelStates.Add(channelIndex, channelState);
            }

            return channelState;
        }

        private static void TryGuessLabelsFromVgm(List<Channel> channels, string vgmFile)
        {
            var file = new VgmFile();
            file.LoadFromFile(vgmFile);
    
            var channelStates = new Dictionary<int, ChannelState>();

    
            foreach (var command in file.Commands())
            {

                if (command is VgmFile.WaitCommand)
                {
                    // Wait
                    foreach (var channelState in channelStates.Values)
                    {
                        channelState.AddTime(((VgmFile.WaitCommand)command).Ticks);
                    }
                }

                else if (command is VgmFile.AddressDataCommand){
                    
                    if (((VgmFile.AddressDataCommand)command).Address >= 0x30 && ((VgmFile.AddressDataCommand)command).Address <= 0x38)
                    {
                        // YM2413 instrument
                        GetChannelState(((VgmFile.AddressDataCommand)command).Address & 0xf, channelStates).SetInstrument(((VgmFile.AddressDataCommand)command).Data >> 4);
                    }
                    else if (((VgmFile.AddressDataCommand)command).Address >= 0x20 && ((VgmFile.AddressDataCommand)command).Address <= 0x28)
                    {
                        // YM2413 key down
                        var channelState = GetChannelState(((VgmFile.AddressDataCommand)command).Address & 0xf, channelStates);
                        channelState.KeyDown = (((VgmFile.AddressDataCommand)command).Data & 0x10) != 0;
                    }
                    
                }
                
            }
    
            foreach (var kvp in channelStates.OrderBy(x => x.Key))
            {
                Console.WriteLine($"YM2413 channel {kvp.Key}: {kvp.Value}");
            }
    
            foreach (var channel in channels.Where(c => c.Label.StartsWith("YM2413 Tone ")))
            {
                var match = Regex.Match(channel.Label, "^YM2413 Tone (?<index>[0-9])$");
                if (!match.Success)
                {
                    continue;
                }
                var index = Convert.ToInt32(match.Groups["index"].Value) - 1;
                ChannelState channelState;
                if (channelStates.TryGetValue(index, out  channelState))
                {
                    var instruments = channelState.Instruments
                        .Where(x => x.Ticks > 0)
                        .Select(x => x.Name);
                    channel.Label += ": " + string.Join("/\x200b", instruments);
    
                    Console.WriteLine($"Channel {index} is {channel.Label}");
                }
            }
        }
    
        private static Color ParseColor(string value)
        {
            // If it looks like hex, use that.
            // We support 3, 6 or 8 hex chars.
            var match = Regex.Match(value, "^#?(?<hex>[0-9a-fA-F]{3}([0-9a-fA-F]{3})?([0-9a-fA-F]{2})?)$");
            if (match.Success)
            {
                var hex = match.Groups["hex"].Value;
                if (hex.Length == 3)
                {
                    hex = $"{hex[0]}{hex[0]}{hex[1]}{hex[1]}{hex[2]}{hex[2]}";
                }
    
                if (hex.Length == 6)
                {
                    hex = "ff" + hex;
                }
                int alpha = Convert.ToInt32(hex.Substring(0, 2), 16);
                int red = Convert.ToInt32(hex.Substring(2, 2), 16);
                int green = Convert.ToInt32(hex.Substring(4, 2), 16);
                int blue = Convert.ToInt32(hex.Substring(6, 2), 16);
                return Color.FromArgb(alpha, red, green, blue);
            }
            // Then try named colors
            var property = typeof(Color)
                .GetProperties(BindingFlags.Static | BindingFlags.DeclaredOnly | BindingFlags.Public)
                .FirstOrDefault(p =>
                    p.PropertyType == typeof(Color) &&
                    p.Name.Equals(value, StringComparison.InvariantCultureIgnoreCase));
            if (property == null)
            {
                throw new Exception($"Could not parse color {value}");
            }
    
            return (Color)property.GetValue(null);
        }
    
        private static void RunMultiDumper(ref Settings settings)
        {
            if (settings.MultidumperPath == null || settings.VgmFile == null || settings.InputFiles != null)
            {
                return;
            }
            // We normalize the VGM path here because we need to know its directory...
            settings.VgmFile = Path.GetFullPath(settings.VgmFile);
            // Check if we have WAVs. Note that we use "natural" sorting to make sure 10 comes after 9.
            settings.InputFiles = Directory.EnumerateFiles(
                    Path.GetDirectoryName(settings.VgmFile) ?? string.Empty,
                    Path.GetFileNameWithoutExtension(settings.VgmFile) + " - *.wav")
                .OrderByAlphaNumeric(s => s)
                .ToList();
            if (!settings.InputFiles.Any())
            {
                Console.WriteLine("Running MultiDumper...");
                // Let's run it
                var wrapper = new MultiDumperWrapper(settings.MultidumperPath);
                var song = wrapper.GetSongs(settings.VgmFile).First();
                var filenames = wrapper.Dump(song, d => Console.Write($"\r{d:P0}"));
                settings.InputFiles = filenames.OrderByAlphaNumeric(s => s).ToList();
                Console.WriteLine($" done. {settings.InputFiles.Count} files found.");
            }
            else
            {
                Console.WriteLine($"Skipping MultiDumper as {settings.InputFiles.Count} files were already present.");
            }
        }
    
        private void Render(Settings settings, IReadOnlyCollection<Channel> channels)
        {
            Console.WriteLine("Generating background image...");
    
            var backgroundImage = new BackgroundRenderer(settings.Width, settings.Height, ParseColor(settings.BackgroundColor));
            if (settings.BackgroundImageFile != null)
            {
                using (var bm = Image.FromFile(settings.BackgroundImageFile))
                {
                    backgroundImage.Add(new ImageInfo(bm, ContentAlignment.MiddleCenter, true, DockStyle.None, 0.5f));
                }
            }
    
            if (!string.IsNullOrEmpty(settings.LogoImageFile))
            {
                using (var bm = Image.FromFile(settings.LogoImageFile))
                {
                    backgroundImage.Add(new ImageInfo(bm, ContentAlignment.BottomRight, false, DockStyle.None, 1));
                }
            }
    
            if (settings.VgmFile != null)
            {
                var gd3 = Gd3Tag.LoadFromVgm(settings.VgmFile);
                var gd3Text = gd3.ToString();
                if (gd3Text.Length > 0)
                {
                    backgroundImage.Add(new TextInfo(gd3Text, settings.Gd3Font, settings.Gd3FontSize, ContentAlignment.BottomLeft, FontStyle.Regular,
                        DockStyle.Bottom, ParseColor(settings.Gd3FontColor)));
                }
            }
    
            if (settings.MaximumAspectRatio > 0.0)
            {
                Console.WriteLine($"Determining column count for maximum aspect ratio {settings.MaximumAspectRatio}:");
                for (var columns = 1; columns < 100; ++columns)
                {
                    var width = backgroundImage.WaveArea.Width / columns;
                    var rows = channels.Count / columns + (channels.Count % columns == 0 ? 0 : 1);
                    var height = backgroundImage.WaveArea.Height / rows;
                    var ratio = (double) width / height;
                    Console.WriteLine($"- {columns} columns => {width} x {height} pixels => ratio {ratio}");
                    if (ratio < settings.MaximumAspectRatio)
                    {
                        settings.Columns = columns;
                        break;
                    }
                }
            }
    

            Renderer = new WaveformRenderer
            {
                BackgroundImage = backgroundImage.Image,
                Columns = settings.Columns,
                FramesPerSecond = settings.FramesPerSecond,
                Width = settings.Width,
                Height = settings.Height,
                SamplingRate = channels.First().SampleRate,
                RenderingBounds = backgroundImage.WaveArea
            };

            if (settings.GridLineWidth > 0)
            {
                foreach (var channel in channels)
                {
                    channel.BorderColor = ParseColor(settings.GridColor);
                    channel.BorderWidth = settings.GridLineWidth;
                    channel.BorderEdges = settings.GridBorder;
                }
            }
    
            // Add the data to the renderer
            foreach (var channel in channels)
            {
                Renderer.AddChannel(channel);
            }
    
            var outputs = new List<IGraphicsOutput>();
            // if (settings.FfMpegPath != null)
            // {
            //     Console.WriteLine("Adding FFMPEG renderer...");
            //     outputs.Add(new FfmpegOutput(settings.FfMpegPath, settings.OutputFile, settings.Width, settings.Height, settings.FramesPerSecond, settings.FfMpegExtraOptions, settings.MasterAudioFile));
            // }
            //
            // if (settings.PreviewFrameskip > 0)
            // {
            //     Console.WriteLine("Adding preview renderer...");
            //     outputs.Add(new PreviewOutput(settings.PreviewFrameskip, true));
            // }
    
            // try
            // {
            //     Console.WriteLine("Rendering...");
            //     var sw = Stopwatch.StartNew();
            //     Renderer.Render(outputs);
            //     sw.Stop();
            //     int numFrames = (int) (channels.Max(x => x.Length).TotalSeconds * settings.FramesPerSecond);
            //     Console.WriteLine($"Rendering complete in {sw.Elapsed:g}, average {numFrames / sw.Elapsed.TotalSeconds:N} fps");
            // }
            // catch (Exception ex)
            // {
            //     // Should mean it was cancelled
            //     Console.WriteLine($"Rendering cancelled: {ex.Message}");
            // }
            // finally
            // {
            //     foreach (var graphicsOutput in outputs)
            //     {
            //         graphicsOutput.Dispose();
            //     }
            // }
        }
    
        private static ITriggerAlgorithm CreateTriggerAlgorithm(string name)
        {
            var type = AppDomain.CurrentDomain
                .GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .FirstOrDefault(t =>
                    typeof(ITriggerAlgorithm).IsAssignableFrom(t) &&
                    t.Name.ToLowerInvariant().Equals(name.ToLowerInvariant()));
            if (type == null)
            {
                throw new Exception($"Unknown trigger algorithm \"{name}\"");
            }
            return Activator.CreateInstance(type) as ITriggerAlgorithm;
        }
        
    }

    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Local")]
    public class Settings
    {
        public List<string> InputFiles { get; set; }

        public string VgmFile { get; set; }

        public string MasterAudioFile { get; set; }

        // ReSharper disable once StringLiteralTypo
        public bool NoMasterMix { get; set; }

        // ReSharper disable once StringLiteralTypo
        public bool NoMasterMixReplayGain { get; set; }

        public string OutputFile { get; set; }

        public int Width { get; set; } = 640;

        public int Height { get; set; } = 480;

        public int Columns { get; set; } = 1;

        // ReSharper disable once StringLiteralTypo
        public double MaximumAspectRatio { get; set; } = -1.0;

        // ReSharper disable once StringLiteralTypo
        public int ViewWidthMs { get; set; } = 35;

        public int FramesPerSecond { get; set; } = 60;

        // ReSharper disable once StringLiteralTypo
        public float LineWidth { get; set; } = 3;

        // ReSharper disable once StringLiteralTypo
        public string LineColor { get; set; } = "white";

        // ReSharper disable once StringLiteralTypo
        public string FillColor { get; set; } = "transparent";

        // ReSharper disable once StringLiteralTypo
        public float AutoScalePercentage { get; set; }

        // ReSharper disable once StringLiteralTypo
        public bool AutoScaleIgnoreYM2413Percussion { get; set; }

        // ReSharper disable once StringLiteralTypo
        public bool ChannelLabelsFromVgm { get; set; }

        // ReSharper disable once StringLiteralTypo
        public string TriggerAlgorithm { get; set; } = nameof(PeakSpeedTrigger);

        // ReSharper disable once StringLiteralTypo
        public int TriggerLookahead { get; set; } = 0;

        // ReSharper disable once StringLiteralTypo
        public bool HighPass { get; set; } = false;

        // ReSharper disable once StringLiteralTypo
        public int PreviewFrameskip { get; set; }

        // ReSharper disable once StringLiteralTypo
        public string FfMpegPath { get; set; }
        // ReSharper disable once StringLiteralTypo
        public string FfMpegExtraOptions { get; set; } = "";

        // ReSharper disable once StringLiteralTypo
        // ReSharper disable once IdentifierTypo
        public string MultidumperPath { get; set; }

        // ReSharper disable once StringLiteralTypo
        public string BackgroundColor { get; set; } = "black";

        public string BackgroundImageFile { get; set; }

        public string LogoImageFile { get; set; }

        // ReSharper disable once StringLiteralTypo
        public string GridColor { get; set; } = "white";
        // ReSharper disable once StringLiteralTypo
        public float GridLineWidth { get; set; } = 0;
        // ReSharper disable once StringLiteralTypo
        public bool GridBorder { get; set; } = true;
        // ReSharper disable once StringLiteralTypo
        public string ZeroLineColor { get; set; } = "white";
        // ReSharper disable once StringLiteralTypo
        public float ZeroLineWidth { get; set; } = 0;

        // ReSharper disable once StringLiteralTypo
        public string Gd3Font { get; set; } = "Tahoma";
        public float Gd3FontSize { get; set; } = 16;
        public string Gd3FontColor { get; set; } = "white";

        // ReSharper disable once StringLiteralTypo
        public string ChannelLabelsFont { get; set; }
        // ReSharper disable once StringLiteralTypo
        public float ChannelLabelsSize { get; set; }
        // ReSharper disable once StringLiteralTypo
        public string ChannelLabelsColor { get; set; } = "white";

        // ReSharper disable once StringLiteralTypo
        public bool YouTubeOnly { get; set; }

    }
}
