using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Serilog;

namespace AlgoTradeWithPythonWithScottPlot
{
    public class AlgoTrader : IDisposable
    {
        private GuiManager guiManager;
        private bool disposed = false;
        private readonly ILogger logger = Log.ForContext<AlgoTrader>();

        public AlgoTrader()
        {
            logger.Information("AlgoTrader instance created");
        }

        public void SetGuiManager(GuiManager gui)
        {
            guiManager = gui;
            
            // Subscribe to GUI events
            guiManager.InitRequested += OnInitRequested;
            guiManager.StartRequested += OnStartRequested;
            guiManager.StopRequested += OnStopRequested;
            guiManager.ResetRequested += OnResetRequested;
            guiManager.TerminateRequested += OnTerminateRequested;
            
            logger.Information("GuiManager connected to AlgoTrader");
            guiManager.UpdateStatus("AlgoTrader ready");
        }

        // Event handlers for GUI requests
        private void OnInitRequested(object sender, EventArgs e)
        {
            Init();
        }

        private void OnStartRequested(object sender, EventArgs e)
        {
            Start();
        }

        private void OnStopRequested(object sender, EventArgs e)
        {
            Stop();
        }

        private void OnResetRequested(object sender, EventArgs e)
        {
            Reset();
        }

        private void OnTerminateRequested(object sender, EventArgs e)
        {
            Terminate();
        }

        public void Reset()
        {
            logger.Information("Reset method called");
            guiManager?.UpdateStatus("AlgoTrader reset");
        }

        public void Init()
        {
            logger.Information("Init method called - Initializing AlgoTrader");
            guiManager?.UpdateStatus("AlgoTrader initializing...");
            
            // Simulate some initialization work
            System.Threading.Thread.Sleep(100);
            
            guiManager?.UpdateStatus("AlgoTrader initialized");
        }

        public void Start()
        {
            logger.Information("Start method called - Starting AlgoTrader");
            guiManager?.UpdateStatus("AlgoTrader starting...");
            
            // Simulate startup work
            System.Threading.Thread.Sleep(100);
            
            guiManager?.UpdateStatus("AlgoTrader running");
        }

        public void Stop()
        {
            logger.Information("Stop method called - Stopping AlgoTrader");
            guiManager?.UpdateStatus("AlgoTrader stopping...");
            
            // Simulate stop work
            System.Threading.Thread.Sleep(100);
            
            guiManager?.UpdateStatus("AlgoTrader stopped");
        }

        public void Terminate()
        {
            logger.Information("Terminate method called - Terminating AlgoTrader");
            guiManager?.UpdateStatus("AlgoTrader terminating...");

            // Simulate termination work
            System.Threading.Thread.Sleep(100);

            guiManager?.UpdateStatus("AlgoTrader terminated");
        }

        // ========== DATA LOADING FROM CONFIG ==========

        /// <summary>
        /// JSON config dosyasından plotları ve dataları yükler
        /// </summary>
        public void LoadDataFromConfig(string configFilePath)
        {
            try
            {
                logger.Information($"Loading data from config: {configFilePath}");
                guiManager?.UpdateStatus($"Loading config: {configFilePath}");

                // JSON dosyasını oku
                string jsonContent = File.ReadAllText(configFilePath);
                var config = JsonSerializer.Deserialize<PlotConfiguration>(jsonContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (config == null || config.Plots == null || config.Plots.Count == 0)
                {
                    logger.Warning("Config file is empty or invalid");
                    guiManager?.UpdateStatus("Config file is empty");
                    return;
                }

                logger.Information($"Config loaded: {config.Plots.Count} plots defined");

                // Her plot için - Form1'deki addPlotToolStripMenuItem_Click yaklaşımı
                foreach (var plotDef in config.Plots)
                {
                    logger.Information($"Processing plot: {plotDef.PlotId} - {plotDef.PlotName}");

                    // Plot'u oluştur (AddPlot ile)
                    string createdId = guiManager?.AddPlot(plotDef.PlotId, System.Windows.Forms.DockStyle.Top, height: plotDef.Height);

                    if (string.IsNullOrEmpty(createdId))
                    {
                        logger.Error($"Failed to create plot: {plotDef.PlotId}");
                        continue;
                    }

                    // Plot'u al (GetPlot ile FormsPlot)
                    var plot = guiManager?.GetPlot(createdId);
                    if (plot == null)
                    {
                        logger.Error($"Failed to get FormsPlot for: {createdId}");
                        continue;
                    }

                    // Her data serisi için
                    foreach (var dataDef in plotDef.Data.OrderBy(d => d.DataId))
                    {
                        logger.Information($"  Loading data: {dataDef.Name} (Type: {dataDef.Type}, Source: {dataDef.Source})");

                        try
                        {
                            // Data tipine göre işle ve plot'a ekle
                            switch (dataDef.Type.ToUpper())
                            {
                                case "OHLC":
                                    LoadOHLCDataToPlot(plot, dataDef);
                                    break;

                                case "VOLUME":
                                    LoadVolumeDataToPlot(plot, dataDef);
                                    break;

                                case "LINE":
                                    // AddAdaptivePlotPublic kullan (Form1 yaklaşımı)
                                    LoadLineDataToPlot(plot, dataDef, createdId);
                                    break;

                                case "HISTOGRAM":
                                    LoadHistogramDataToPlot(plot, dataDef);
                                    break;

                                default:
                                    logger.Warning($"Unknown data type: {dataDef.Type}");
                                    break;
                            }
                        }
                        catch (Exception ex)
                        {
                            logger.Error($"Error loading data {dataDef.Name}: {ex.Message}");
                            guiManager?.UpdateStatus($"Error loading {dataDef.Name}");
                        }
                    }

                    // Plot başlığını set et ve refresh yap
                    plot.Plot.Title(plotDef.PlotName);
                    plot.Refresh();

                    logger.Information($"Plot {createdId} completed with title: {plotDef.PlotName}");
                }

                logger.Information("Config loading completed successfully");
                guiManager?.UpdateStatus($"Loaded {config.Plots.Count} plots successfully");
            }
            catch (Exception ex)
            {
                logger.Error($"Error loading config: {ex.Message}");
                guiManager?.UpdateStatus($"Error loading config: {ex.Message}");
            }
        }

        /// <summary>
        /// Bir data definition'ı plot'a yükler
        /// </summary>
        private void LoadDataToPlot(PlotInfo plotInfo, DataDefinition dataDef)
        {
            // Dosyayı oku
            if (!File.Exists(dataDef.Source))
            {
                logger.Warning($"Data file not found: {dataDef.Source}");
                return;
            }

            // Data tipine göre yükle
            switch (dataDef.Type.ToUpper())
            {
                case "OHLC":
                    LoadOHLCData(plotInfo, dataDef);
                    break;

                case "VOLUME":
                    LoadVolumeData(plotInfo, dataDef);
                    break;

                case "LINE":
                    LoadLineData(plotInfo, dataDef);
                    break;

                case "HISTOGRAM":
                    LoadHistogramData(plotInfo, dataDef);
                    break;

                default:
                    logger.Warning($"Unknown data type: {dataDef.Type}");
                    break;
            }
        }

        /// <summary>
        /// OHLC datasını yükler
        /// CSV Format: Timestamp,Open,High,Low,Close
        /// </summary>
        private void LoadOHLCData(PlotInfo plotInfo, DataDefinition dataDef)
        {
            var ohlcList = new List<ScottPlot.OHLC>();
            var lines = File.ReadAllLines(dataDef.Source).Skip(1); // Skip header

            foreach (var line in lines)
            {
                var parts = line.Split(',');
                if (parts.Length >= 5)
                {
                    double open = double.Parse(parts[1]);
                    double high = double.Parse(parts[2]);
                    double low = double.Parse(parts[3]);
                    double close = double.Parse(parts[4]);
                    DateTime timestamp = DateTime.Parse(parts[0]);
                    TimeSpan span = TimeSpan.FromDays(1); // Default 1 gün bar süresi

                    ohlcList.Add(new ScottPlot.OHLC(open, high, low, close, timestamp, span));
                }
            }

            plotInfo.SetOHLCData(ohlcList);
            logger.Information($"Loaded {ohlcList.Count} OHLC data points");
        }

        /// <summary>
        /// Volume datasını yükler
        /// CSV Format: Timestamp,Volume
        /// </summary>
        private void LoadVolumeData(PlotInfo plotInfo, DataDefinition dataDef)
        {
            var volumes = new List<double>();
            var positions = new List<double>();
            var lines = File.ReadAllLines(dataDef.Source).Skip(1); // Skip header

            int index = 0;
            foreach (var line in lines)
            {
                var parts = line.Split(',');
                if (parts.Length >= 2)
                {
                    double volume = double.Parse(parts[1]);
                    volumes.Add(volume);
                    positions.Add(index++);
                }
            }

            plotInfo.SetVolumeData(volumes.ToArray(), positions.ToArray());
            logger.Information($"Loaded {volumes.Count} volume data points");
        }

        /// <summary>
        /// Line (çizgi) datasını yükler - AddAdaptivePlotPublic kullanarak
        /// CSV Format: X,Y  veya  Timestamp,Value
        /// </summary>
        private void LoadLineData(PlotInfo plotInfo, DataDefinition dataDef)
        {
            var xData = new List<double>();
            var yData = new List<double>();
            var lines = File.ReadAllLines(dataDef.Source).Skip(1); // Skip header

            int index = 0;
            foreach (var line in lines)
            {
                var parts = line.Split(',');
                if (parts.Length >= 2)
                {
                    // X değeri timestamp ise index kullan, değilse parse et
                    xData.Add(index++);
                    yData.Add(double.Parse(parts[1]));
                }
            }

            var xArray = xData.ToArray();
            var yArray = yData.ToArray();

            if (plotInfo.Plot != null && xArray.Length > 0)
            {
                // GuiManager'ın AddAdaptivePlotPublic metodunu kullan
                // Bu metod otomatik olarak veri boyutuna göre Scatter veya SignalXY seçer
                guiManager?.AddAdaptivePlotPublic(plotInfo.Plot, xArray, yArray, plotInfo.Id);

                // Son eklenen plottable'ı bul ve özelliklerini ayarla (renk, legend)
                var plottables = plotInfo.Plot.Plot.GetPlottables().ToList();
                if (plottables.Count > 0)
                {
                    var lastPlottable = plottables[plottables.Count - 1];

                    // Renk ayarla
                    if (!string.IsNullOrEmpty(dataDef.Color))
                    {
                        var color = ColorTranslator.FromHtml(dataDef.Color);
                        if (lastPlottable is ScottPlot.Plottables.Scatter scatter)
                        {
                            scatter.Color = ScottPlot.Color.FromColor(color);
                        }
                        else if (lastPlottable is ScottPlot.Plottables.SignalXY signal)
                        {
                            signal.Color = ScottPlot.Color.FromColor(color);
                        }
                    }

                    // Legend ayarla
                    if (!string.IsNullOrEmpty(dataDef.Name))
                    {
                        if (lastPlottable is ScottPlot.Plottables.Scatter scatter)
                        {
                            scatter.LegendText = dataDef.Name;
                        }
                        else if (lastPlottable is ScottPlot.Plottables.SignalXY signal)
                        {
                            signal.LegendText = dataDef.Name;
                        }
                    }
                }

                logger.Information($"Loaded {xArray.Length} line data points for {dataDef.Name} using AddAdaptivePlotPublic");
            }
        }

        /// <summary>
        /// Histogram datasını yükler
        /// CSV Format: X,Y
        /// </summary>
        private void LoadHistogramData(PlotInfo plotInfo, DataDefinition dataDef)
        {
            var values = new List<double>();
            var positions = new List<double>();
            var lines = File.ReadAllLines(dataDef.Source).Skip(1); // Skip header

            int index = 0;
            foreach (var line in lines)
            {
                var parts = line.Split(',');
                if (parts.Length >= 2)
                {
                    values.Add(double.Parse(parts[1]));
                    positions.Add(index++);
                }
            }

            // Renk parse et
            Color? color = null;
            if (!string.IsNullOrEmpty(dataDef.Color))
            {
                color = ColorTranslator.FromHtml(dataDef.Color);
            }

            plotInfo.SetHistogramData(values.ToArray(), positions.ToArray(), color);
            logger.Information($"Loaded {values.Count} histogram data points");
        }

        // ========== FormsPlot için direkt yükleme metodları ==========

        /// <summary>
        /// OHLC datasını FormsPlot'a direkt yükler
        /// CSV Format: Timestamp,Open,High,Low,Close
        /// </summary>
        private void LoadOHLCDataToPlot(ScottPlot.WinForms.FormsPlot plot, DataDefinition dataDef)
        {
            var ohlcList = new List<ScottPlot.OHLC>();
            var lines = File.ReadAllLines(dataDef.Source).Skip(1); // Skip header

            foreach (var line in lines)
            {
                var parts = line.Split(',');
                if (parts.Length >= 5)
                {
                    double open = double.Parse(parts[1]);
                    double high = double.Parse(parts[2]);
                    double low = double.Parse(parts[3]);
                    double close = double.Parse(parts[4]);
                    DateTime timestamp = DateTime.Parse(parts[0]);
                    TimeSpan span = TimeSpan.FromDays(1); // Default 1 gün bar süresi

                    ohlcList.Add(new ScottPlot.OHLC(open, high, low, close, timestamp, span));
                }
            }

            // OHLC chart ekle
            var candlestick = plot.Plot.Add.Candlestick(ohlcList);

            // Renk ve legend için not: CandlestickPlot ScottPlot 5.x'te renk ve legend ayarı desteklemeyebilir
            // veya farklı property isimleri kullanıyor olabilir. Şimdilik basit şekilde bırakıyoruz.

            logger.Information($"Loaded {ohlcList.Count} OHLC data points to plot");
        }

        /// <summary>
        /// Volume datasını FormsPlot'a direkt yükler
        /// CSV Format: Timestamp,Volume
        /// </summary>
        private void LoadVolumeDataToPlot(ScottPlot.WinForms.FormsPlot plot, DataDefinition dataDef)
        {
            var bars = new List<ScottPlot.Bar>();
            var lines = File.ReadAllLines(dataDef.Source).Skip(1); // Skip header

            // Renk parse et
            ScottPlot.Color fillColor = ScottPlot.Colors.Blue.WithAlpha(0.5);
            if (!string.IsNullOrEmpty(dataDef.Color))
            {
                var color = ColorTranslator.FromHtml(dataDef.Color);
                fillColor = ScottPlot.Color.FromColor(color).WithAlpha(0.5);
            }

            int index = 0;
            foreach (var line in lines)
            {
                var parts = line.Split(',');
                if (parts.Length >= 2)
                {
                    double volume = double.Parse(parts[1]);
                    bars.Add(new ScottPlot.Bar
                    {
                        Position = index++,
                        Value = volume,
                        FillColor = fillColor
                    });
                }
            }

            // Bar chart ekle
            var barPlot = plot.Plot.Add.Bars(bars);

            // Legend ayarla
            if (!string.IsNullOrEmpty(dataDef.Name))
            {
                barPlot.LegendText = dataDef.Name;
            }

            logger.Information($"Loaded {bars.Count} volume data points to plot");
        }

        /// <summary>
        /// Line (çizgi) datasını FormsPlot'a direkt yükler
        /// CSV Format: X,Y  veya  Timestamp,Value
        /// </summary>
        private void LoadLineDataToPlot(ScottPlot.WinForms.FormsPlot plot, DataDefinition dataDef, string plotId)
        {
            var xData = new List<double>();
            var yData = new List<double>();
            var lines = File.ReadAllLines(dataDef.Source).Skip(1); // Skip header

            int index = 0;
            foreach (var line in lines)
            {
                var parts = line.Split(',');
                if (parts.Length >= 2)
                {
                    // X değeri timestamp ise index kullan, değilse parse et
                    xData.Add(index++);
                    yData.Add(double.Parse(parts[1]));
                }
            }

            var xArray = xData.ToArray();
            var yArray = yData.ToArray();

            if (xArray.Length > 0)
            {
                // GuiManager'ın AddAdaptivePlotPublic metodunu kullan
                guiManager?.AddAdaptivePlotPublic(plot, xArray, yArray, plotId);

                // Son eklenen plottable'ı bul ve özelliklerini ayarla
                var plottables = plot.Plot.GetPlottables().ToList();
                if (plottables.Count > 0)
                {
                    var lastPlottable = plottables[plottables.Count - 1];

                    // Renk ayarla
                    if (!string.IsNullOrEmpty(dataDef.Color))
                    {
                        var color = ColorTranslator.FromHtml(dataDef.Color);
                        if (lastPlottable is ScottPlot.Plottables.Scatter scatter)
                        {
                            scatter.Color = ScottPlot.Color.FromColor(color);
                        }
                        else if (lastPlottable is ScottPlot.Plottables.SignalXY signal)
                        {
                            signal.Color = ScottPlot.Color.FromColor(color);
                        }
                    }

                    // Legend ayarla
                    if (!string.IsNullOrEmpty(dataDef.Name))
                    {
                        if (lastPlottable is ScottPlot.Plottables.Scatter scatter)
                        {
                            scatter.LegendText = dataDef.Name;
                        }
                        else if (lastPlottable is ScottPlot.Plottables.SignalXY signal)
                        {
                            signal.LegendText = dataDef.Name;
                        }
                    }
                }

                logger.Information($"Loaded {xArray.Length} line data points for {dataDef.Name} to plot");
            }
        }

        /// <summary>
        /// Histogram datasını FormsPlot'a direkt yükler
        /// CSV Format: X,Y
        /// </summary>
        private void LoadHistogramDataToPlot(ScottPlot.WinForms.FormsPlot plot, DataDefinition dataDef)
        {
            var bars = new List<ScottPlot.Bar>();
            var lines = File.ReadAllLines(dataDef.Source).Skip(1); // Skip header

            // Renk parse et
            ScottPlot.Color fillColor = ScottPlot.Colors.Gray.WithAlpha(0.5);
            if (!string.IsNullOrEmpty(dataDef.Color))
            {
                var color = ColorTranslator.FromHtml(dataDef.Color);
                fillColor = ScottPlot.Color.FromColor(color).WithAlpha(0.5);
            }

            int index = 0;
            foreach (var line in lines)
            {
                var parts = line.Split(',');
                if (parts.Length >= 2)
                {
                    bars.Add(new ScottPlot.Bar
                    {
                        Position = index++,
                        Value = double.Parse(parts[1]),
                        FillColor = fillColor
                    });
                }
            }

            // Bar chart ekle
            var barPlot = plot.Plot.Add.Bars(bars);

            // Legend ayarla
            if (!string.IsNullOrEmpty(dataDef.Name))
            {
                barPlot.LegendText = dataDef.Name;
            }

            logger.Information($"Loaded {bars.Count} histogram data points to plot");
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    logger.Information("Disposing AlgoTrader and its components");

                    // Unsubscribe from GUI events
                    if (guiManager != null)
                    {
                        guiManager.InitRequested -= OnInitRequested;
                        guiManager.StartRequested -= OnStartRequested;
                        guiManager.StopRequested -= OnStopRequested;
                        guiManager.ResetRequested -= OnResetRequested;
                        guiManager.TerminateRequested -= OnTerminateRequested;
                    }
                }
                disposed = true;
                logger.Information("AlgoTrader disposed successfully");
            }
        }

        ~AlgoTrader()
        {
            Dispose(false);
        }
    }
}