using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using ScottPlot.WinForms;
using ScottPlot.Plottables;

namespace AlgoTradeWithPythonWithScottPlot
{
    public class PlotInfo : IDisposable
    {
        public string Id { get; set; }
        public FormsPlot Plot { get; set; }
        public Panel Container { get; set; }
        public bool IsVisible { get; set; }
        public DockStyle DockStyle { get; set; }
        public Size? FixedSize { get; set; }
        public AnchorStyles Anchor { get; set; }
        public Point Location { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Title { get; set; }
        public Color BackgroundColor { get; set; }

        // Crosshair reference
        public Crosshair Crosshair { get; set; }

        // ========== VERİ YÖNETİMİ ==========

        // OHLC verisi (tek bir tane olabilir)
        private CandlestickPlot? _ohlcPlottable;

        // Volume verisi (tek bir tane olabilir)
        private BarPlot? _volumePlottable;

        // Histogram verisi (tek bir tane olabilir)
        private BarPlot? _histogramPlottable;

        // Çizgisel veriler (birden fazla olabilir: MA5, MA8, MA200, vb.)
        private readonly Dictionary<int, Scatter> _lineData = new();
        private readonly Dictionary<string, int> _lineNameToIndex = new(); // İsimle erişim için
        private int _nextLineIndex = 0;

        // Zoom control references
        public Button YZoomInButton { get; set; }
        public Button YZoomOutButton { get; set; }
        public Button XZoomInButton { get; set; }
        public Button XZoomOutButton { get; set; }
        
        // Pan control references
        public Button YPanUpButton { get; set; }
        public Button YPanDownButton { get; set; }
        public Button XPanLeftButton { get; set; }
        public Button XPanRightButton { get; set; }
        
        // Reset control references
        public Button ResetButton { get; set; }
        public Button ResetXButton { get; set; }
        public Button ResetYButton { get; set; }

        // Copy to all control reference
        public Button CopyToAllButton { get; set; }
        
        // Plot management buttons
        public Button CloseButton { get; set; }
        public Button MaximizeButton { get; set; }

        // Horizontal ScrollBar for X-axis data navigation
        public HScrollBar XAxisScrollBar { get; set; }

        // Vertical ScrollBar for Y-axis data navigation
        public VScrollBar YAxisScrollBar { get; set; }

        // ViewRange bilgisi - scrollbar için gerekli
        private (double XMin, double XMax)? _currentXViewRange;
        private (double YMin, double YMax)? _currentYViewRange;
        internal double[] _currentXData;
        internal double[] _currentYData;

        private bool disposed = false;

        public PlotInfo(string id)
        {
            Id = id;
            CreatedAt = DateTime.Now;
            IsVisible = true;
            DockStyle = DockStyle.Fill;
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Bottom | AnchorStyles.Right;
            Location = Point.Empty;
            Title = id;
            BackgroundColor = Color.White;
        }

        // ========== OHLC VERİ YÖNETİMİ ==========

        public void SetOHLCData(List<ScottPlot.OHLC> ohlcList)
        {
            if (Plot == null) return;

            // Eski OHLC plot varsa kaldır
            if (_ohlcPlottable != null)
            {
                Plot.Plot.Remove(_ohlcPlottable);
            }

            // Yeni OHLC plot ekle
            _ohlcPlottable = Plot.Plot.Add.Candlestick(ohlcList);
            Plot.Refresh();
        }

        // ========== VOLUME VERİ YÖNETİMİ ==========

        public void SetVolumeData(double[] volumes, double[] positions)
        {
            if (Plot == null) return;

            // Eski volume plot varsa kaldır
            if (_volumePlottable != null)
            {
                Plot.Plot.Remove(_volumePlottable);
            }

            // Yeni volume plot ekle
            var bars = new List<ScottPlot.Bar>();
            for (int i = 0; i < volumes.Length; i++)
            {
                bars.Add(new ScottPlot.Bar
                {
                    Position = positions[i],
                    Value = volumes[i],
                    FillColor = ScottPlot.Colors.Blue.WithAlpha(0.5)
                });
            }

            _volumePlottable = Plot.Plot.Add.Bars(bars);
            Plot.Refresh();
        }

        // ========== HISTOGRAM VERİ YÖNETİMİ ==========

        public void SetHistogramData(double[] values, double[] positions, Color? color = null)
        {
            if (Plot == null) return;

            // Eski histogram plot varsa kaldır
            if (_histogramPlottable != null)
            {
                Plot.Plot.Remove(_histogramPlottable);
            }

            // Yeni histogram plot ekle
            var bars = new List<ScottPlot.Bar>();
            var fillColor = color ?? Color.Gray;

            for (int i = 0; i < values.Length; i++)
            {
                bars.Add(new ScottPlot.Bar
                {
                    Position = positions[i],
                    Value = values[i],
                    FillColor = ScottPlot.Color.FromColor(fillColor).WithAlpha(0.5)
                });
            }

            _histogramPlottable = Plot.Plot.Add.Bars(bars);
            Plot.Refresh();
        }

        // ========== ÇİZGİSEL VERİ YÖNETİMİ (İNDEKSLİ) ==========

        /// <summary>
        /// İndeks ile çizgisel veri ekler/günceller (MA5, MA8, MA200 gibi)
        /// </summary>
        public void SetYData(int index, double[] xData, double[] yData, string name = null, Color? color = null)
        {
            if (Plot == null) return;

            // Eski çizgi varsa kaldır
            if (_lineData.TryGetValue(index, out var oldLine))
            {
                Plot.Plot.Remove(oldLine);
            }

            // Yeni çizgi ekle
            var newLine = Plot.Plot.Add.Scatter(xData, yData);

            if (color.HasValue)
                newLine.Color = ScottPlot.Color.FromColor(color.Value);

            if (!string.IsNullOrEmpty(name))
            {
                newLine.LegendText = name;
                _lineNameToIndex[name] = index;
            }

            _lineData[index] = newLine;
            Plot.Refresh();
        }

        /// <summary>
        /// İsimle çizgisel veri ekler (otomatik indeks ataması yapar)
        /// </summary>
        public int AddLineData(string name, double[] xData, double[] yData, Color? color = null)
        {
            // Aynı isimde çizgi varsa, onun indeksini kullan
            if (_lineNameToIndex.TryGetValue(name, out int existingIndex))
            {
                SetYData(existingIndex, xData, yData, name, color);
                return existingIndex;
            }

            // Yeni çizgi için otomatik indeks ata
            int newIndex = _nextLineIndex++;
            SetYData(newIndex, xData, yData, name, color);
            return newIndex;
        }

        /// <summary>
        /// İsimle çizgisel veriyi siler
        /// </summary>
        public bool RemoveLineData(string name)
        {
            if (_lineNameToIndex.TryGetValue(name, out int index))
            {
                return RemoveLineData(index);
            }
            return false;
        }

        /// <summary>
        /// İndeks ile çizgisel veriyi siler
        /// </summary>
        public bool RemoveLineData(int index)
        {
            if (_lineData.TryGetValue(index, out var line))
            {
                if (Plot != null)
                {
                    Plot.Plot.Remove(line);
                    Plot.Refresh();
                }

                _lineData.Remove(index);

                // İsim mapping'i temizle
                var nameToRemove = _lineNameToIndex.FirstOrDefault(x => x.Value == index).Key;
                if (nameToRemove != null)
                    _lineNameToIndex.Remove(nameToRemove);

                return true;
            }
            return false;
        }

        /// <summary>
        /// Tüm çizgisel verileri temizler
        /// </summary>
        public void ClearAllLineData()
        {
            if (Plot != null)
            {
                foreach (var line in _lineData.Values)
                {
                    Plot.Plot.Remove(line);
                }
                Plot.Refresh();
            }

            _lineData.Clear();
            _lineNameToIndex.Clear();
        }

        /// <summary>
        /// Tüm verileri temizler (OHLC, Volume, Histogram, Lines)
        /// </summary>
        public void ClearAllData()
        {
            if (Plot != null)
            {
                if (_ohlcPlottable != null)
                {
                    Plot.Plot.Remove(_ohlcPlottable);
                    _ohlcPlottable = null;
                }

                if (_volumePlottable != null)
                {
                    Plot.Plot.Remove(_volumePlottable);
                    _volumePlottable = null;
                }

                if (_histogramPlottable != null)
                {
                    Plot.Plot.Remove(_histogramPlottable);
                    _histogramPlottable = null;
                }

                ClearAllLineData();
            }
        }

        // ========== AXIS LIMIT YÖNETİMİ ==========

        /// <summary>
        /// X ekseni limitlerini ayarlar
        /// </summary>
        public void SetXAxisLimits(double xMin, double xMax)
        {
            if (Plot == null) return;
            Plot.Plot.Axes.SetLimitsX(xMin, xMax);
            Plot.Refresh();
        }

        /// <summary>
        /// X ViewRange ayarlar ve X scrollbar'ı yapılandırır
        /// </summary>
        public void SetXViewRangeWithScrollBar((double XMin, double XMax)? viewRange, double[] xData, double[] yData)
        {
            if (Plot == null || XAxisScrollBar == null) return;

            _currentXViewRange = viewRange;
            _currentXData = xData;
            _currentYData = yData;

            if (viewRange.HasValue && xData != null && xData.Length > 0)
            {
                // ViewRange var - ScrollBar'ı göster ve ayarla
                var (xMin, xMax) = viewRange.Value;

                // Görünen aralıktaki data sayısını hesapla
                int visibleDataCount = 0;
                for (int i = 0; i < xData.Length; i++)
                {
                    if (xData[i] >= xMin && xData[i] <= xMax)
                        visibleDataCount++;
                }

                // ScrollBar properties
                XAxisScrollBar.Minimum = 0;
                XAxisScrollBar.Maximum = Math.Max(0, xData.Length - 1);
                XAxisScrollBar.LargeChange = Math.Max(1, visibleDataCount);
                XAxisScrollBar.SmallChange = Math.Max(1, visibleDataCount / 10);

                // Şu anki pozisyonu bul (viewRange'in başlangıç noktası)
                int currentPos = 0;
                for (int i = 0; i < xData.Length; i++)
                {
                    if (xData[i] >= xMin)
                    {
                        currentPos = i;
                        break;
                    }
                }
                XAxisScrollBar.Value = Math.Min(currentPos, XAxisScrollBar.Maximum - XAxisScrollBar.LargeChange + 1);

                XAxisScrollBar.Visible = true;
            }
            else
            {
                // ViewRange yok - ScrollBar'ı gizle
                XAxisScrollBar.Visible = false;
            }
        }

        /// <summary>
        /// Y ViewRange ayarlar ve Y scrollbar'ı yapılandırır
        /// </summary>
        public void SetYViewRangeWithScrollBar((double YMin, double YMax)? viewRange, double[] xData, double[] yData)
        {
            if (Plot == null || YAxisScrollBar == null) return;

            _currentYViewRange = viewRange;
            _currentXData = xData;
            _currentYData = yData;

            if (viewRange.HasValue && yData != null && yData.Length > 0)
            {
                // ViewRange var - ScrollBar'ı göster ve ayarla
                var (yMin, yMax) = viewRange.Value;

                // Görünen aralıktaki data sayısını hesapla
                int visibleDataCount = 0;
                for (int i = 0; i < yData.Length; i++)
                {
                    if (yData[i] >= yMin && yData[i] <= yMax)
                        visibleDataCount++;
                }

                // ScrollBar properties
                YAxisScrollBar.Minimum = 0;
                YAxisScrollBar.Maximum = Math.Max(0, yData.Length - 1);
                YAxisScrollBar.LargeChange = Math.Max(1, visibleDataCount);
                YAxisScrollBar.SmallChange = Math.Max(1, visibleDataCount / 10);

                // Şu anki pozisyonu bul (viewRange'in başlangıç noktası)
                int currentPos = 0;
                for (int i = 0; i < yData.Length; i++)
                {
                    if (yData[i] >= yMin)
                    {
                        currentPos = i;
                        break;
                    }
                }
                YAxisScrollBar.Value = Math.Min(currentPos, YAxisScrollBar.Maximum - YAxisScrollBar.LargeChange + 1);

                YAxisScrollBar.Visible = true;
            }
            else
            {
                // ViewRange yok - ScrollBar'ı gizle
                YAxisScrollBar.Visible = false;
            }
        }

        /// <summary>
        /// X ScrollBar'ın pozisyonuna göre X axis limitlerini günceller
        /// </summary>
        public void UpdateXViewFromScrollBar(int scrollPosition)
        {
            if (Plot == null || _currentXData == null || _currentXData.Length == 0) return;

            // Scroll position'dan başlayarak LargeChange kadar veri göster
            int startIndex = Math.Max(0, scrollPosition);
            int endIndex = Math.Min(_currentXData.Length - 1, startIndex + XAxisScrollBar.LargeChange - 1);

            if (startIndex < _currentXData.Length && endIndex < _currentXData.Length)
            {
                double xMin = _currentXData[startIndex];
                double xMax = _currentXData[endIndex];

                Plot.Plot.Axes.SetLimitsX(xMin, xMax);
                Plot.Plot.Axes.AutoScaleY();
                Plot.Refresh();
            }
        }

        /// <summary>
        /// Y ScrollBar'ın pozisyonuna göre Y axis limitlerini günceller
        /// </summary>
        public void UpdateYViewFromScrollBar(int scrollPosition)
        {
            if (Plot == null || _currentYData == null || _currentYData.Length == 0) return;

            // Scroll position'dan başlayarak LargeChange kadar veri göster
            int startIndex = Math.Max(0, scrollPosition);
            int endIndex = Math.Min(_currentYData.Length - 1, startIndex + YAxisScrollBar.LargeChange - 1);

            if (startIndex < _currentYData.Length && endIndex < _currentYData.Length)
            {
                double yMin = _currentYData[startIndex];
                double yMax = _currentYData[endIndex];

                Plot.Plot.Axes.SetLimitsY(yMin, yMax);
                Plot.Plot.Axes.AutoScaleX();
                Plot.Refresh();
            }
        }

        /// <summary>
        /// Y ekseni limitlerini ayarlar
        /// </summary>
        public void SetYAxisLimits(double yMin, double yMax)
        {
            if (Plot == null) return;
            Plot.Plot.Axes.SetLimitsY(yMin, yMax);
            Plot.Refresh();
        }

        /// <summary>
        /// Her iki ekseni de ayarlar
        /// </summary>
        public void SetAxisLimits(double xMin, double xMax, double yMin, double yMax)
        {
            if (Plot == null) return;
            Plot.Plot.Axes.SetLimits(xMin, xMax, yMin, yMax);
            Plot.Refresh();
        }

        /// <summary>
        /// Axis limitlerini otomatik ayarlar (tüm veriyi göster)
        /// </summary>
        public void AutoScale()
        {
            if (Plot == null) return;
            Plot.Plot.Axes.AutoScale();
            Plot.Refresh();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed && disposing)
            {
                Plot?.Dispose();
                
                // Dispose zoom buttons
                YZoomInButton?.Dispose();
                YZoomOutButton?.Dispose();
                XZoomInButton?.Dispose();
                XZoomOutButton?.Dispose();
                
                // Dispose pan buttons
                YPanUpButton?.Dispose();
                YPanDownButton?.Dispose();
                XPanLeftButton?.Dispose();
                XPanRightButton?.Dispose();
                
                // Dispose reset buttons
                ResetButton?.Dispose();
                ResetXButton?.Dispose();
                ResetYButton?.Dispose();

                // Dispose copy to all button
                CopyToAllButton?.Dispose();
                
                // Dispose plot management buttons
                CloseButton?.Dispose();
                MaximizeButton?.Dispose();

                // Dispose scrollbars
                XAxisScrollBar?.Dispose();
                YAxisScrollBar?.Dispose();

                Container?.Dispose();
                disposed = true;
            }
        }

        ~PlotInfo()
        {
            Dispose(false);
        }
    }
}