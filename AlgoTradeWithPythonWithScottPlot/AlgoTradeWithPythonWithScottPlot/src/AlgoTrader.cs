using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
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
        private PlotConfiguration? loadedConfig = null; // Okunan JSON config'i sakla

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
        /// JSON config dosyasını okur ve sadece plotları oluşturur (data yüklemez)
        /// </summary>
        public void CreatePlotsFromConfig(string configFilePath)
        {
            try
            {
                logger.Information($"Reading config and creating plots: {configFilePath}");
                guiManager?.UpdateStatus($"Reading config: {configFilePath}");

                // JSON dosyasını oku
                string jsonContent = File.ReadAllText(configFilePath);
                loadedConfig = JsonSerializer.Deserialize<PlotConfiguration>(jsonContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (loadedConfig == null || loadedConfig.Plots == null || loadedConfig.Plots.Count == 0)
                {
                    logger.Warning("Config file is empty or invalid");
                    guiManager?.UpdateStatus("Config file is empty");
                    return;
                }

                logger.Information($"Config loaded: {loadedConfig.Plots.Count} plots defined");

                // Her plot için sadece boş plot penceresi oluştur
                foreach (var plotDef in loadedConfig.Plots)
                {
                    logger.Information($"Creating empty plot: {plotDef.PlotId} - {plotDef.PlotName}");

                    // Plot'u oluştur (AddPlot ile)
                    string createdId = guiManager?.AddPlot(plotDef.PlotId, System.Windows.Forms.DockStyle.Top, height: plotDef.Height);

                    if (string.IsNullOrEmpty(createdId))
                    {
                        logger.Error($"Failed to create plot: {plotDef.PlotId}");
                        continue;
                    }

                    // Plot'u al
                    var plot = guiManager?.GetPlot(createdId);
                    if (plot == null)
                    {
                        logger.Error($"Failed to get FormsPlot for: {createdId}");
                        continue;
                    }

                    // Sadece başlığı set et (data ekleme!)
                    plot.Plot.Title(plotDef.PlotName);
                    plot.Refresh();

                    logger.Information($"Empty plot {createdId} created with title: {plotDef.PlotName}");
                }

                logger.Information($"Plot creation completed: {loadedConfig.Plots.Count} plots created");
                guiManager?.UpdateStatus($"Created {loadedConfig.Plots.Count} empty plots");
            }
            catch (Exception ex)
            {
                logger.Error($"Error creating plots from config: {ex.Message}");
                guiManager?.UpdateStatus($"Error: {ex.Message}");
                loadedConfig = null;
            }
        }

        /// <summary>
        /// Önceden oluşturulmuş plotlara, loadedConfig'den verileri yükler
        /// </summary>
        public void LoadDataToPlots()
        {
            try
            {
                if (loadedConfig == null || loadedConfig.Plots == null || loadedConfig.Plots.Count == 0)
                {
                    logger.Warning("No config loaded. Please use Read button first.");
                    guiManager?.UpdateStatus("Error: No config loaded");
                    return;
                }

                logger.Information($"Loading data to {loadedConfig.Plots.Count} plots");
                guiManager?.UpdateStatus("Loading data to plots...");

                // Her plot için
                foreach (var plotDef in loadedConfig.Plots)
                {
                    logger.Information($"Loading data to plot: {plotDef.PlotId}");

                    // Plot'u al (GetPlot ile FormsPlot)
                    var plot = guiManager?.GetPlot(plotDef.PlotId);
                    if (plot == null)
                    {
                        logger.Warning($"Plot not found: {plotDef.PlotId}. Was it created with Read button?");
                        continue;
                    }

                    // PlotInfo'ya PlotName'i kaydet (zoom sırasında Y-axis kuralları için)
                    var plotInfo = guiManager?.GetPlotInfo(plotDef.PlotId);
                    if (plotInfo != null)
                    {
                        plotInfo.PlotName = plotDef.PlotName;
                    }

                    // Plot'u temizle ama Crosshair'ı koru!
                    // Clear() yerine sadece data plottable'larını sil
                    var plottablesToRemove = plot.Plot.GetPlottables()
                        .Where(p => !(p is ScottPlot.Plottables.Crosshair))
                        .ToList();
                    foreach (var plottable in plottablesToRemove)
                    {
                        plot.Plot.Remove(plottable);
                    }

                    // Her data serisi için
                    foreach (var dataDef in plotDef.Data.OrderBy(d => d.DataId))
                    {
                        logger.Information($"  Loading data: {dataDef.Name} (Type: {dataDef.Type}, Source: {dataDef.Source})");
                        string typeUpper = dataDef.Type.ToUpperInvariant();
                        logger.Information($"  Type after ToUpperInvariant: '{typeUpper}'");

                        try
                        {
                            // Data tipine göre işle ve plot'a ekle
                            switch (typeUpper)
                            {
                                case "OHLC":
                                    LoadOHLCDataToPlot(plot, dataDef);
                                    break;

                                case "VOLUME":
                                    LoadVolumeDataToPlot(plot, dataDef);
                                    break;

                                case "LINE":
                                    // AddAdaptivePlotPublic kullan (addPlotToolStripMenuItem_Click yaklaşımı)
                                    LoadLineDataToPlot(plot, dataDef, plotDef.PlotId);
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

                    // Plot başlığını yeniden set et
                    plot.Plot.Title(plotDef.PlotName);

                    // Crosshair zaten AddPlot'ta eklendi ve korundu, tekrar eklemeye gerek yok

                    // AutoScale
                    plot.Plot.Axes.AutoScale();

                    // Plot türüne göre Y-axis limitlerini ayarla
                    string plotNameLower = plotDef.PlotName.ToLowerInvariant();
                    if (plotNameLower.Contains("volume"))
                    {
                        // Volume plot: Y-axis 0'dan başlamalı
                        var currentLimits = plot.Plot.Axes.GetLimits();
                        plot.Plot.Axes.SetLimitsY(0, currentLimits.Top);
                        logger.Information($"Volume plot Y-axis adjusted: [0, {currentLimits.Top:F2}]");
                    }
                    else if (plotNameLower.Contains("rsi"))
                    {
                        // RSI plot: Y-axis 0-100 sabit
                        var currentLimits = plot.Plot.Axes.GetLimits();
                        plot.Plot.Axes.SetLimits(currentLimits.Left, currentLimits.Right, 0, 100);
                        logger.Information($"RSI plot Y-axis fixed: [0, 100]");
                    }

                    plot.Refresh();

                    logger.Information($"Data loading completed for plot: {plotDef.PlotId}");
                }

                logger.Information("All data loaded successfully");
                guiManager?.UpdateStatus($"Loaded data to {loadedConfig.Plots.Count} plots successfully");
            }
            catch (Exception ex)
            {
                logger.Error($"Error loading data to plots: {ex.Message}");
                guiManager?.UpdateStatus($"Error: {ex.Message}");
            }
        }

        /// <summary>
        /// JSON config dosyasından plotları ve dataları yükler (ESKİ METOD - şimdi kullanılmıyor)
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
                        string typeUpper = dataDef.Type.ToUpperInvariant();
                        logger.Information($"  Type after ToUpperInvariant: '{typeUpper}'");

                        try
                        {
                            // Data tipine göre işle ve plot'a ekle
                            switch (typeUpper)
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
                    double open = double.Parse(parts[1], CultureInfo.InvariantCulture);
                    double high = double.Parse(parts[2], CultureInfo.InvariantCulture);
                    double low = double.Parse(parts[3], CultureInfo.InvariantCulture);
                    double close = double.Parse(parts[4], CultureInfo.InvariantCulture);
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

            foreach (var line in lines)
            {
                var parts = line.Split(',');
                if (parts.Length >= 2)
                {
                    // Parse timestamp and convert to OADate for consistent x-axis
                    if (DateTime.TryParse(parts[0], out DateTime timestamp))
                    {
                        double volume = double.Parse(parts[1], CultureInfo.InvariantCulture);
                        volumes.Add(volume);
                        positions.Add(timestamp.ToOADate());
                    }
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

            foreach (var line in lines)
            {
                var parts = line.Split(',');
                if (parts.Length >= 2)
                {
                    // Parse timestamp from CSV and convert to OADate (same as OHLC)
                    if (DateTime.TryParse(parts[0], out DateTime timestamp))
                    {
                        xData.Add(timestamp.ToOADate());
                        yData.Add(double.Parse(parts[1], CultureInfo.InvariantCulture));
                    }
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

            foreach (var line in lines)
            {
                var parts = line.Split(',');
                if (parts.Length >= 2)
                {
                    // Parse timestamp and convert to OADate for consistent x-axis
                    if (DateTime.TryParse(parts[0], out DateTime timestamp))
                    {
                        values.Add(double.Parse(parts[1], CultureInfo.InvariantCulture));
                        positions.Add(timestamp.ToOADate());
                    }
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
                    double open = double.Parse(parts[1], CultureInfo.InvariantCulture);
                    double high = double.Parse(parts[2], CultureInfo.InvariantCulture);
                    double low = double.Parse(parts[3], CultureInfo.InvariantCulture);
                    double close = double.Parse(parts[4], CultureInfo.InvariantCulture);
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

            foreach (var line in lines)
            {
                var parts = line.Split(',');
                if (parts.Length >= 2)
                {
                    // Parse timestamp and convert to OADate for consistent x-axis
                    if (DateTime.TryParse(parts[0], out DateTime timestamp))
                    {
                        double volume = double.Parse(parts[1], CultureInfo.InvariantCulture);
                        bars.Add(new ScottPlot.Bar
                        {
                            Position = timestamp.ToOADate(),
                            Value = volume,
                            FillColor = fillColor
                        });
                    }
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

            foreach (var line in lines)
            {
                var parts = line.Split(',');
                if (parts.Length >= 2)
                {
                    // Parse timestamp from CSV and convert to OADate (same as OHLC)
                    if (DateTime.TryParse(parts[0], out DateTime timestamp))
                    {
                        xData.Add(timestamp.ToOADate());
                        yData.Add(double.Parse(parts[1], CultureInfo.InvariantCulture));
                    }
                }
            }

            var xArray = xData.ToArray();
            var yArray = yData.ToArray();

            if (xArray.Length > 0)
            {
                logger.Information($"LoadLineDataToPlot: Loaded {xArray.Length} points for {dataDef.Name}, X range: [{xArray.Min():F2}, {xArray.Max():F2}], Y range: [{yArray.Min():F2}, {yArray.Max():F2}]");

                // GuiManager'ın AddAdaptivePlotPublic metodunu kullan
                guiManager?.AddAdaptivePlotPublic(plot, xArray, yArray, plotId);

                // Son eklenen plottable'ı bul ve özelliklerini ayarla
                var plottables = plot.Plot.GetPlottables().ToList();
                logger.Information($"LoadLineDataToPlot: Total plottables after adding: {plottables.Count}");
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

            foreach (var line in lines)
            {
                var parts = line.Split(',');
                if (parts.Length >= 2)
                {
                    // Parse timestamp and convert to OADate for consistent x-axis
                    if (DateTime.TryParse(parts[0], out DateTime timestamp))
                    {
                        bars.Add(new ScottPlot.Bar
                        {
                            Position = timestamp.ToOADate(),
                            Value = double.Parse(parts[1], CultureInfo.InvariantCulture),
                            FillColor = fillColor
                        });
                    }
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