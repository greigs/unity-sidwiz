using System;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing;
using System.Drawing.Design;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using LibSidWiz.Triggers;


namespace LibSidWiz
{

    public struct Padding
    {
        /// <summary>Provides a <see cref="T:System.Windows.Forms.Padding" /> object with no padding.</summary>
        public static readonly Padding Empty = new Padding(0);
        private bool _all;
        private int _top;
        private int _left;
        private int _right;
        private int _bottom;

        /// <summary>Initializes a new instance of the <see cref="T:System.Windows.Forms.Padding" /> class using the supplied padding size for all edges.</summary>
        /// <param name="all">The number of pixels to be used for padding for all edges.</param>
        public Padding(int all)
        {
            this._all = true;
            this._top = this._left = this._right = this._bottom = all;
        }

        /// <summary>Initializes a new instance of the <see cref="T:System.Windows.Forms.Padding" /> class using a separate padding size for each edge.</summary>
        /// <param name="left">The padding size, in pixels, for the left edge.</param>
        /// <param name="top">The padding size, in pixels, for the top edge.</param>
        /// <param name="right">The padding size, in pixels, for the right edge.</param>
        /// <param name="bottom">The padding size, in pixels, for the bottom edge.</param>
        public Padding(int left, int top, int right, int bottom)
        {
            this._top = top;
            this._left = left;
            this._right = right;
            this._bottom = bottom;
            this._all = this._top == this._left && this._top == this._right && this._top == this._bottom;
        }

        /// <summary>Gets or sets the padding value for all the edges.</summary>
        /// <returns>The padding, in pixels, for all edges if the same; otherwise, -1.</returns>
        [RefreshProperties(RefreshProperties.All)]
        public int All
        {
            get
            {
                return !this._all ? -1 : this._top;
            }
            set
            {
                if (this._all && this._top == value)
                    return;
                this._all = true;
                this._top = this._left = this._right = this._bottom = value;
            }
        }

        /// <summary>Gets or sets the padding value for the bottom edge.</summary>
        /// <returns>The padding, in pixels, for the bottom edge.</returns>
        [RefreshProperties(RefreshProperties.All)]
        public int Bottom
        {
            get
            {
                return this._all ? this._top : this._bottom;
            }
            set
            {
                if (!this._all && this._bottom == value)
                    return;
                this._all = false;
                this._bottom = value;
            }
        }

        /// <summary>Gets or sets the padding value for the left edge.</summary>
        /// <returns>The padding, in pixels, for the left edge.</returns>
        [RefreshProperties(RefreshProperties.All)]
        public int Left
        {
            get
            {
                return this._all ? this._top : this._left;
            }
            set
            {
                if (!this._all && this._left == value)
                    return;
                this._all = false;
                this._left = value;
            }
        }

        /// <summary>Gets or sets the padding value for the right edge.</summary>
        /// <returns>The padding, in pixels, for the right edge.</returns>
        [RefreshProperties(RefreshProperties.All)]
        public int Right
        {
            get
            {
                return this._all ? this._top : this._right;
            }
            set
            {
                if (!this._all && this._right == value)
                    return;
                this._all = false;
                this._right = value;
            }
        }

        /// <summary>Gets or sets the padding value for the top edge.</summary>
        /// <returns>The padding, in pixels, for the top edge.</returns>
        [RefreshProperties(RefreshProperties.All)]
        public int Top
        {
            get
            {
                return this._top;
            }
            set
            {
                if (!this._all && this._top == value)
                    return;
                this._all = false;
                this._top = value;
            }
        }

        /// <summary>Gets the combined padding for the right and left edges.</summary>
        /// <returns>Gets the sum, in pixels, of the <see cref="P:System.Windows.Forms.Padding.Left" /> and <see cref="P:System.Windows.Forms.Padding.Right" /> padding values.</returns>
        [Browsable(false)]
        public int Horizontal
        {
            get
            {
                return this.Left + this.Right;
            }
        }

        /// <summary>Gets the combined padding for the top and bottom edges.</summary>
        /// <returns>Gets the sum, in pixels, of the <see cref="P:System.Windows.Forms.Padding.Top" /> and <see cref="P:System.Windows.Forms.Padding.Bottom" /> padding values.</returns>
        [Browsable(false)]
        public int Vertical
        {
            get
            {
                return this.Top + this.Bottom;
            }
        }

        /// <summary>Gets the padding information in the form of a <see cref="T:System.Drawing.Size" />.</summary>
        /// <returns>A <see cref="T:System.Drawing.Size" /> containing the padding information.</returns>
        [Browsable(false)]
        public Size Size
        {
            get
            {
                return new Size(this.Horizontal, this.Vertical);
            }
        }

        /// <summary>Computes the sum of the two specified <see cref="T:System.Windows.Forms.Padding" /> values.</summary>
        /// <param name="p1">A <see cref="T:System.Windows.Forms.Padding" />.</param>
        /// <param name="p2">A <see cref="T:System.Windows.Forms.Padding" />.</param>
        /// <returns>A <see cref="T:System.Windows.Forms.Padding" /> that contains the sum of the two specified <see cref="T:System.Windows.Forms.Padding" /> values.</returns>
        public static Padding Add(Padding p1, Padding p2)
        {
            return p1 + p2;
        }

        /// <summary>Subtracts one specified <see cref="T:System.Windows.Forms.Padding" /> value from another.</summary>
        /// <param name="p1">A <see cref="T:System.Windows.Forms.Padding" />.</param>
        /// <param name="p2">A <see cref="T:System.Windows.Forms.Padding" />.</param>
        /// <returns>A <see cref="T:System.Windows.Forms.Padding" /> that contains the result of the subtraction of one specified <see cref="T:System.Windows.Forms.Padding" /> value from another.</returns>
        public static Padding Subtract(Padding p1, Padding p2)
        {
            return p1 - p2;
        }


        /// <summary>Performs vector addition on the two specified <see cref="T:System.Windows.Forms.Padding" /> objects, resulting in a new <see cref="T:System.Windows.Forms.Padding" />.</summary>
        /// <param name="p1">The first <see cref="T:System.Windows.Forms.Padding" /> to add.</param>
        /// <param name="p2">The second <see cref="T:System.Windows.Forms.Padding" /> to add.</param>
        /// <returns>A new <see cref="T:System.Windows.Forms.Padding" /> that results from adding <paramref name="p1" /> and <paramref name="p2" />.</returns>
        public static Padding operator +(Padding p1, Padding p2)
        {
            return new Padding(p1.Left + p2.Left, p1.Top + p2.Top, p1.Right + p2.Right, p1.Bottom + p2.Bottom);
        }

        /// <summary>Performs vector subtraction on the two specified <see cref="T:System.Windows.Forms.Padding" /> objects, resulting in a new <see cref="T:System.Windows.Forms.Padding" />.</summary>
        /// <param name="p1">The <see cref="T:System.Windows.Forms.Padding" /> to subtract from (the minuend).</param>
        /// <param name="p2">The <see cref="T:System.Windows.Forms.Padding" /> to subtract from (the subtrahend).</param>
        /// <returns>The <see cref="T:System.Windows.Forms.Padding" /> result of subtracting <paramref name="p2" /> from <paramref name="p1" />.</returns>
        public static Padding operator -(Padding p1, Padding p2)
        {
            return new Padding(p1.Left - p2.Left, p1.Top - p2.Top, p1.Right - p2.Right, p1.Bottom - p2.Bottom);
        }

        /// <summary>Tests whether two specified <see cref="T:System.Windows.Forms.Padding" /> objects are equivalent.</summary>
        /// <param name="p1">A <see cref="T:System.Windows.Forms.Padding" /> to test.</param>
        /// <param name="p2">A <see cref="T:System.Windows.Forms.Padding" /> to test.</param>
        /// <returns>
        /// <see langword="true" /> if the two <see cref="T:System.Windows.Forms.Padding" /> objects are equal; otherwise, <see langword="false" />.</returns>
        public static bool operator ==(Padding p1, Padding p2)
        {
            return p1.Left == p2.Left && p1.Top == p2.Top && p1.Right == p2.Right && p1.Bottom == p2.Bottom;
        }

        /// <summary>Tests whether two specified <see cref="T:System.Windows.Forms.Padding" /> objects are not equivalent.</summary>
        /// <param name="p1">A <see cref="T:System.Windows.Forms.Padding" /> to test.</param>
        /// <param name="p2">A <see cref="T:System.Windows.Forms.Padding" /> to test.</param>
        /// <returns>
        /// <see langword="true" /> if the two <see cref="T:System.Windows.Forms.Padding" /> objects are different; otherwise, <see langword="false" />.</returns>
        public static bool operator !=(Padding p1, Padding p2)
        {
            return !(p1 == p2);
        }


        /// <summary>Returns a string that represents the current <see cref="T:System.Windows.Forms.Padding" />.</summary>
        /// <returns>A <see cref="T:System.String" /> that represents the current <see cref="T:System.Windows.Forms.Padding" />.</returns>
        public override string ToString()
        {
            return "{Left=" + this.Left.ToString((IFormatProvider)CultureInfo.CurrentCulture) + ",Top=" + this.Top.ToString((IFormatProvider)CultureInfo.CurrentCulture) + ",Right=" + this.Right.ToString((IFormatProvider)CultureInfo.CurrentCulture) + ",Bottom=" + this.Bottom.ToString((IFormatProvider)CultureInfo.CurrentCulture) + "}";
        }

        private void ResetAll()
        {
            this.All = 0;
        }

        private void ResetBottom()
        {
            this.Bottom = 0;
        }

        private void ResetLeft()
        {
            this.Left = 0;
        }

        private void ResetRight()
        {
            this.Right = 0;
        }

        private void ResetTop()
        {
            this.Top = 0;
        }

        internal void Scale(float dx, float dy)
        {
            this._top = (int)((double)this._top * (double)dy);
            this._left = (int)((double)this._left * (double)dx);
            this._right = (int)((double)this._right * (double)dx);
            this._bottom = (int)((double)this._bottom * (double)dy);
        }

        internal bool ShouldSerializeAll()
        {
            return this._all;
        }

    }
    /// <summary>
    /// Wraps a single "voice", and also deals with loading the data into memory
    /// </summary>
    public class Channel: IDisposable
    {
        private SampleBuffer _samples;
        private SampleBuffer _samplesForTrigger;
        private string _filename;
        private string _externalTriggerFilename;
        private ITriggerAlgorithm _algorithm;
        private int _triggerLookaheadFrames;
        private Color _lineColor = Color.White;
        private string _label = "";
        private float _lineWidth = 3;
        private float _scale = 1.0f;
        private int _viewWidthInSamples = 1500;
        private Color _fillColor = Color.Transparent;
        private float _zeroLineWidth;
        private Color _zeroLineColor = Color.Transparent;
        private Font _labelFont;
        private Color _labelColor = Color.Transparent;
        private Color _borderColor = Color.Transparent;
        private float _borderWidth;
        private ContentAlignment _labelAlignment = ContentAlignment.TopLeft; 
        private Padding _labelMargins = new Padding(0, 0, 0, 0);
        private bool _invertedTrigger;
        private bool _borderEdges = true;
        private Color _backgroundColor = Color.Transparent;
        private bool _clip;
        private Sides _side = Sides.Mix;
        private bool _smoothLines = true;
        private bool _filter;
        private bool _renderIfSilent;

        public enum Sides
        {
            Left,
            Right,
            Mix
        }

        public event Action<Channel, bool> Changed;

        public Task<bool> LoadDataAsync(CancellationToken token = new CancellationToken())
        {
            return Task.Factory.StartNew(() =>
            {
                try
                {
                    ErrorMessage = "";

                    if (string.IsNullOrEmpty(Filename))
                    {
                        _samples = null;
                        SampleCount = 0;
                        Max = 0;
                        Length = TimeSpan.Zero;
                        Loading = false;
                        IsEmpty = true;
                        return false;
                    }

                    IsEmpty = false;

                    Console.WriteLine($"- Reading {Filename}");
                    _samples = new SampleBuffer(Filename, Side, HighPassFilter);
                    SampleRate = _samples.SampleRate;
                    Length = _samples.Length;

                    token.ThrowIfCancellationRequested();

                    _samples.Analyze();

                    SampleCount = _samples.Count;

                    token.ThrowIfCancellationRequested();

                    Max = Math.Max(Math.Abs(_samples.Max), Math.Abs(_samples.Min));

                    Console.WriteLine($"- Peak sample amplitude for {Filename} is {Max}");

                    if (string.IsNullOrEmpty(ExternalTriggerFilename))
                    {
                        // Point at the same SampleBuffer
                        _samplesForTrigger = _samples;
                    }
                    else
                    {
                        _samplesForTrigger = new SampleBuffer(ExternalTriggerFilename, Side, HighPassFilter);
                    }

                    Loading = false;
                    return true;
                }
                catch (TaskCanceledException)
                {
                    // Blank out if cancelled
                    Max = 0;
                    SampleRate = 0;
                    Length = TimeSpan.Zero;
                    _samples?.Dispose();
                    _samples = null;
                    Loading = false;
                    return false;
                }
                catch (Exception ex)
                {
                    ErrorMessage = ex.ToString();
                    Max = 0;
                    SampleRate = 0;
                    Length = TimeSpan.Zero;
                    _samples?.Dispose();
                    _samples = null;
                    Loading = false;
                    return false;
                }
                finally
                {
                    Changed?.Invoke(this, false);
                }
            }, token);
        }

        [Category("Data")]
        [Description("The full text of any error message when loading the file")]
        
        public string ErrorMessage { get; private set; }

        [Category("Data")]
        [Description("The filename to be rendered")]
        public string Filename
        {
            get { return _filename; }
            set
            {
                bool needReload = value != _filename;
                _filename = value;
                Changed?.Invoke(this, needReload);
                if (_filename != "" && string.IsNullOrEmpty(_label))
                {
                    Label = GuessNameFromMultidumperFilename(_filename);
                }
            }
        }

        [Category("Triggering")]
        [Description("The filename to use for oscilloscope triggering. Leave blank to use the channel's sound data.")]
        public string ExternalTriggerFilename
        {
            get { return _externalTriggerFilename; }
            set
            {
                bool needReload = value != _externalTriggerFilename;
                _externalTriggerFilename = value;
                Changed?.Invoke(this, needReload);
                if (_filename != "" && string.IsNullOrEmpty(_label))
                {
                    Label = GuessNameFromMultidumperFilename(_filename);
                }
            }
        }

        [Category("Data")]
        [Description("The channel to use from the file (if stereo)")]
        public Sides Side
        {
            get { return _side; }
            set
            {
                bool needReload = value != _side;
                _side = value;
                Changed?.Invoke(this, needReload);
                LoadDataAsync();
            }
        }

        [Category("Data")]
        [Description("If enabled, high pass filtering will be used to remove DC offsets")]
        public bool HighPassFilter
        {
            get { return _filter; }
            set
            {
                bool needReload = value != _filter;
                _filter = value;
                Changed?.Invoke(this, needReload);
                LoadDataAsync();
            }
        }

        [Category("Triggering")]
        [Description("The algorithm to use for rendering")]
        [TypeConverter(typeof(TriggerAlgorithmTypeConverter))]
        //[JsonConverter(typeof(TriggerAlgorithmJsonConverter))]
        public ITriggerAlgorithm Algorithm
        {
            get { return _algorithm; }
            set
            {
                _algorithm = value;
                Changed?.Invoke(this, false);
            }
        }

        [Category("Triggering")]
        [Description("How many frames to allow the triggering algorithm to look ahead. Zero means only look within the current frame. Set to larger numbers to support sync to low frequencies, but too large numbers can cause erroneous matches.")]
        public int TriggerLookaheadFrames
        {
            get { return _triggerLookaheadFrames; }
            set
            {
                _triggerLookaheadFrames = value;
                Changed?.Invoke(this, false);
            }
        }

        [Category("Appearance")]
        [Description("The line colour")]
        public Color LineColor
        {
            get { return _lineColor; }
            set
            {
                _lineColor = value;
                Changed?.Invoke(this, false);
            }
        }

        [Category("Appearance")]
        [Description("The line width, in pixels. Fractional values are supported.")]
        public float LineWidth
        {
            get { return _lineWidth; }
            set
            {
                _lineWidth = value;
                Changed?.Invoke(this, false);
            }
        }

        [Category("Appearance")]
        [Description("The fill colour. Set to transparent to have no fill.")]
        public Color FillColor
        {
            get { return _fillColor; }
            set
            {
                _fillColor = value;
                Changed?.Invoke(this, false);
            }
        }

        [Category("Appearance")]
        [Description("Whether to draw lines pixelated (false) or anti-aliased (true)")]
        public bool SmoothLines
        {
            get { return _smoothLines; }
            set
            {
                _smoothLines = value;
                Changed?.Invoke(this, false);
            }
        }

        [Category("Appearance")]
        [Description("The width of the zero line")]
        public float ZeroLineWidth
        {
            get { return _zeroLineWidth; }
            set
            {
                _zeroLineWidth = value;
                Changed?.Invoke(this, false);
            }
        }

        [Category("Appearance")]
        [Description("The margins for the chanel label")]
        public Padding LabelMargins
        {
            get { return _labelMargins; }
            set
            {
                _labelMargins = value;
                Changed?.Invoke(this, false);
            }
        }

        [Category("Appearance")]
        [Description("The color of the zero line")]
        public Color ZeroLineColor
        {
            get { return _zeroLineColor; }
            set
            {
                _zeroLineColor = value;
                Changed?.Invoke(this, false);
            }
        }

        [Category("Appearance")]
        [Description("The color of the border")]
        public Color BorderColor
        {
            get { return _borderColor; }
            set
            {
                _borderColor = value;
                Changed?.Invoke(this, false);
            }
        }

        [Category("Appearance")]
        [Description("The width of the border")]
        public float BorderWidth
        {
            get { return _borderWidth; }
            set
            {
                _borderWidth = value;
                Changed?.Invoke(this, false);
            }
        }

        [Category("Appearance")]
        [Description("Whether to draw the outer edges of any border boxes")]
        public bool BorderEdges
        {
            get { return _borderEdges; }
            set
            {
                _borderEdges = value;
                Changed?.Invoke(this, false);
            }
        }

        [Category("Appearance")]
        [Description("A background colour for the channel. This is layered above any background image, and can be transparent.")]
        public Color BackgroundColor
        {
            get { return _backgroundColor; }
            set
            {
                _backgroundColor = value;
                Changed?.Invoke(this, false);
            }
        }

        [Category("Appearance")]
        [Description("The label for the channel")]
        public string Label
        {
            get { return _label; }
            set
            {
                _label = value;
                Changed?.Invoke(this, false);
            }
        }

        [Category("Appearance")]
        [Description("The font for the channel label")]
        public Font LabelFont
        {
            get { return _labelFont; }
            set
            {
                _labelFont = value;
                Changed?.Invoke(this, false);
            }
        }

        [Category("Appearance")]
        [Description("The color for the channel label")]
        public Color LabelColor
        {
            get { return _labelColor; }
            set
            {
                _labelColor = value;
                Changed?.Invoke(this, false);
            }
        }

        [Category("Appearance")]
        [Description("The alignment for the channel label")]
        public ContentAlignment LabelAlignment
        {
            get {
                return _labelAlignment;
            }
            set
            {
                _labelAlignment = value;
                Changed?.Invoke(this, false);
            }
        }

        // [Category("Appearance")]
        // [Description("The margins for the chanel label")]
        // public Padding LabelMargins
        // {
        //     get { return _labelMargins; }
        //     set
        //     {
        //         _labelMargins = value;
        //         Changed?.Invoke(this, false);
        //     }
        // }

        [Category("Adjustment")]
        [Description("Vertical scaling. This may be set by the auto-scaler.")]
        public float Scale
        {
            get { return _scale; }
            set
            {
                _scale = value;
                Changed?.Invoke(this, false);
            }
        }

        [Category("Adjustment")]
        [Description("Whether to constrain the waveform to its screen area when scaled past 100%")]
        public bool Clip
        {
            get { return _clip; }
            set
            {
                _clip = value;
                Changed?.Invoke(this, false);
            }
        }

        [Category("Adjustment")]
        [Description("View width, in ms")]
        
        public float ViewWidthInMilliseconds
        {
            get { return SampleRate == 0 ? 0 : (float) _viewWidthInSamples * 1000 / SampleRate; }
            set
            {
                _viewWidthInSamples = (int) (value / 1000 * SampleRate);
                Changed?.Invoke(this, false);
            }
        }

        [Category("Adjustment")]
        [Description("View width, in samples")]
        public int ViewWidthInSamples
        {
            get { return _viewWidthInSamples; }
            set
            {
                _viewWidthInSamples = value;
                Changed?.Invoke(this, false);
            }
        }

        [Category("Triggering")]
        [Description("Set to true to trigger in the opposite direction")]
        public bool InvertedTrigger
        {
            get { return _invertedTrigger; }
            set
            {
                _invertedTrigger = value;
                Changed?.Invoke(this, false);
            }
        }

        [Category("Data")]
        [Description("Peak amplitude for the channel")]
        
        public float Max { get; private set; }

        [Browsable(false)]
        
        public int SampleCount { get; private set; }

        [Category("Data")]
        [Description("Duration of the channel")]
        
        public TimeSpan Length { get; private set; }

        [Category("Data")]
        [Description("Sampling rate of the channel")]
        
        public int SampleRate { get; private set; }

        [Category("Appearance")]
        [Description("Whether to render silent channels normally. If false, a warning message is shown instead.")]
        public bool RenderIfSilent
        {
            get { return _renderIfSilent; }
            set
            {
                _renderIfSilent = value;
                Changed?.Invoke(this, false);
            }
        }

        // ReSharper disable once CompareOfFloatsByEqualityOperator
        [Browsable(false)]
        
        public bool IsSilent => Max == 0.0;

        [Browsable(false)]
        
        public bool Loading { get; private set; } = true;

        [Browsable(false)]
        
        public bool IsEmpty { get; private set; }

        [Browsable(false)]
        
        internal Rectangle Bounds { get; set; }

        internal float GetSample(int sampleIndex, bool forTrigger = true)
        {
            var source = forTrigger ? _samplesForTrigger : _samples;
            return sampleIndex < 0 || sampleIndex >= source.Count ? 0 : source[sampleIndex] * Scale * (forTrigger && InvertedTrigger ? -1 : 1);
        }

        internal int GetTriggerPoint(int frameIndexSamples, int frameSamples, int previousTriggerPoint)
        {
            return Algorithm.GetTriggerPoint(this, frameIndexSamples, frameIndexSamples + frameSamples * (TriggerLookaheadFrames + 1), previousTriggerPoint);
        }

        public static string GuessNameFromMultidumperFilename(string filename)
        {
            var namePart = Path.GetFileNameWithoutExtension(filename);
            try
            {
                if (namePart == null)
                {
                    return filename;
                }

                var index = namePart.IndexOf(" - YM2413 #", StringComparison.Ordinal);
                if (index > -1)
                {
                    index = int.Parse(namePart.Substring(index + 11));
                    if (index < 9)
                    {
                        return $"YM2413 Tone {index + 1}";
                    }

                    switch (index)
                    {
                        case 9: return "YM2413 Bass Drum";
                        case 10: return "YM2413 Snare Drum";
                        case 11: return "YM2413 Tom-Tom";
                        case 12: return "YM2413 Cymbal";
                        case 13: return "YM2413 Hi-Hat";
                    }
                }

                index = namePart.IndexOf(" - SEGA PSG #", StringComparison.Ordinal);
                if (index > -1)
                {
                    if (int.TryParse(namePart.Substring(index + 13), out index))
                    {
                        switch (index)
                        {
                            case 0:
                            case 1:
                            case 2:
                                return $"Sega PSG Square {index + 1}";
                            case 3:
                                return "Sega PSG Noise";
                        }
                    }
                }

                index = namePart.IndexOf(" - SN76489 #", StringComparison.Ordinal);
                if (index > -1)
                {
                    if (int.TryParse(namePart.Substring(index + 12), out index))
                    {
                        switch (index)
                        {
                            case 0:
                            case 1:
                            case 2:
                                return $"SN76489 Square {index + 1}";
                            case 3:
                                return "SN76489 Noise";
                        }
                    }
                }

                // Guess it's the bit after the last " - "
                index = namePart.LastIndexOf(" - ", StringComparison.Ordinal);
                if (index > -1)
                {
                    return namePart.Substring(index + 3);
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error guessing channel name for {filename}: {ex}");
            }

            // Default to just the filename
            return namePart;
        }

        /// <summary>
        /// This allows us to use a property grid to select a trigger algorithm
        /// </summary>
        public class TriggerAlgorithmTypeConverter: StringConverter
        {
            public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
            {
                return true;
            }

            public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
            {
                return true;
            }

            public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
            {
                return new StandardValuesCollection(
                    Assembly.GetExecutingAssembly()
                        .GetTypes()
                        .Where(t => typeof(ITriggerAlgorithm).IsAssignableFrom(t) && t != typeof(ITriggerAlgorithm))
                        .Select(t => t.Name)
                        .ToList());
            }

            public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
            {
                return sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);
            }

            public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
            {
                if (value is string)
                {
                    var type = Assembly.GetExecutingAssembly()
                        .GetTypes()
                        .FirstOrDefault(t => typeof(ITriggerAlgorithm).IsAssignableFrom(t) && t.Name.ToLowerInvariant().Equals(value.ToString().ToLowerInvariant()));
                    if (type != null)
                    {
                        return Activator.CreateInstance(type) as ITriggerAlgorithm;
                    }
                }

                return base.ConvertFrom(context, culture, value);
            }
        }


        public void Dispose()
        {
            _samples?.Dispose();
            if (_samplesForTrigger != _samples)
            {
                _samplesForTrigger.Dispose();
            }
            _labelFont?.Dispose();
        }


        
    }
}