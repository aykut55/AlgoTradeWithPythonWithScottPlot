using Serilog;

namespace AlgoTradeWithPythonWithScottPlot
{
    public partial class Form1 : Form
    {
        private AlgoTrader algoTrader;
        private GuiManager guiManager;
        private readonly ILogger logger = Log.ForContext<Form1>();
        private System.Threading.Timer logUpdateTimer;

        public Form1()
        {
            InitializeComponent();
            logger.Information("Form1 constructor called - Creating components");
            
            // Create GuiManager and AlgoTrader
            guiManager = new GuiManager();
            guiManager.Initialize(this);
            algoTrader = new AlgoTrader();

            // Connect GuiManager and AlgoTrader (bidirectional)
            algoTrader.SetGuiManager(guiManager);
            guiManager.SetAlgoTrader(algoTrader);

            logger.Information("Form1 initialized successfully");
            
            // Initial status (button text will be set by GuiManager.UpdateLogPanelButtonText)
            guiManager.UpdateStatus("Application started");
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            logger.Information("Form1_Load event triggered - Form is now visible");

            // Timer'ı başlat - her 500ms'de logları güncelle
            logUpdateTimer = new System.Threading.Timer(UpdateLogDisplay, null, 0, 500);

            // Set zoom axis combobox
            guiManager.SetZoomAxisComboBox(cmbZoomAxis);
        }

        private void UpdateLogDisplay(object state)
        {
            if (LoggerConfig.MemoryLogSink.HasLogs)
            {
                var newLogs = LoggerConfig.MemoryLogSink.GetAndClearLogs();
                
                if (newLogs.Count > 0)
                {
                    // UI thread'e marshal et
                    if (InvokeRequired)
                    {
                        Invoke(new Action(() => AppendLogsToTextBox(newLogs)));
                    }
                    else
                    {
                        AppendLogsToTextBox(newLogs);
                    }
                }
            }
        }

        private void AppendLogsToTextBox(List<string> logs)
        {
            foreach (var log in logs)
            {
                txtLogs.AppendText(log);
            }
            
            // En alta scroll et
            txtLogs.SelectionStart = txtLogs.Text.Length;
            txtLogs.ScrollToCaret();
        }

        private void btnToggleLogs_Click(object sender, EventArgs e)
        {
            guiManager.ToggleLogPanel();
        }

        private void btnClearLogs_Click(object sender, EventArgs e)
        {
            guiManager.ClearLogs();
        }

        // Menu event handlers
        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            logger.Information("Exit menu item clicked");
            this.Close();
        }

        private void showLogsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            guiManager.ToggleLogPanel();
        }

        private void clearLogsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            guiManager.ClearLogs();
        }

        private void initToolStripMenuItem_Click(object sender, EventArgs e)
        {
            guiManager.TriggerInit();
        }

        private void startToolStripMenuItem_Click(object sender, EventArgs e)
        {
            guiManager.TriggerStart();
        }

        private void stopToolStripMenuItem_Click(object sender, EventArgs e)
        {
            guiManager.TriggerStop();
        }

        private void resetToolStripMenuItem_Click(object sender, EventArgs e)
        {
            guiManager.TriggerReset();
        }

        private void terminateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            guiManager.TriggerTerminate();
        }

        // Plot menu event handlers
        private void addPlotToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string createdId = guiManager.AddPlot();
            if (!string.IsNullOrEmpty(createdId))
            {
                var plot = guiManager.GetPlot(createdId);
                if (plot != null)
                {
                    // max DataCount
                    int dataCount = 10 * 1000 * 1000;
                        dataCount = 1000;

                    // Generate data using DataManager (idx = 0: Complex wave with noise)
                    var (x, y) = DataManager.GenerateData(idx: 0, points: dataCount);

                    // Use GuiManager's adaptive plotting method for automatic optimization
                    guiManager.AddAdaptivePlotPublic(plot, x, y, createdId);

                    // Clean up the ID for display: "0" -> "Plot 0", "Plot_1" -> "Plot 1"
                    string displayName = createdId.StartsWith("Plot_") ?
                        $"Plot {createdId.Substring(5)}" :
                        $"Plot {createdId}";
                    plot.Plot.Title(displayName);
                    plot.Refresh();

                    logger.Information($"Plot {createdId} added and displayed with {DataManager.GetDataTypeName(0)}");
                }
            }
        }

        private void deletePlotToolStripMenuItem_Click(object sender, EventArgs e)
        {
            guiManager.DeleteAllSecondaryPlots();
        }

        private void clearAllPlotsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            guiManager.ClearAllPlots();
        }

        private void hideAllPlotsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            guiManager.HideAllSecondaryPlots();
        }

        private void showAllPlotsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            guiManager.ShowAllSecondaryPlots();
        }

        private void btnClearData_Click(object sender, EventArgs e)
        {
            guiManager.ClearAllPlots();
        }

        private void btnLoadData_Click(object sender, EventArgs e)
        {
            // Parse input values
            if (!double.TryParse(txtAmplitude.Text, out double amplitude))
            {
                amplitude = 1.0;
                txtAmplitude.Text = "1.0";
            }

            if (!double.TryParse(txtFrequency.Text, out double frequency))
            {
                frequency = 1.0;
                txtFrequency.Text = "1.0";
            }

            if (!int.TryParse(txtPoints.Text, out int points))
            {
                points = 1000;
                txtPoints.Text = "1000";
            }

            // Generate and load sine wave data to all plots
            guiManager.LoadSineWaveData(amplitude, frequency, points);
        }

        private void btnPlotAllData_Click(object sender, EventArgs e)
        {
            // Parse input values
            if (!double.TryParse(txtAmplitude.Text, out double amplitude))
            {
                amplitude = 1.0;
                txtAmplitude.Text = "1.0";
            }

            if (!double.TryParse(txtFrequency.Text, out double frequency))
            {
                frequency = 1.0;
                txtFrequency.Text = "1.0";
            }

            if (!int.TryParse(txtPoints.Text, out int points))
            {
                points = 1000;
                txtPoints.Text = "1000";
            }

            // Generate data using DataManager
            var (x, y) = DataManager.GenerateData(idx: 1, points: points, amplitude: amplitude, frequency: frequency);

            // Use DataFilterManager to get all data (no filtering)
            var filterResult = DataFilterManager.GetAllData(x, y);

            // Plot to all plots
            guiManager.LoadFilteredData(filterResult);

            logger.Information($"Plot All Data: {filterResult.Description}");
        }

        private void btnPlotFitScreen_Click(object sender, EventArgs e)
        {
            // Parse input values
            if (!double.TryParse(txtAmplitude.Text, out double amplitude))
            {
                amplitude = 1.0;
                txtAmplitude.Text = "1.0";
            }

            if (!double.TryParse(txtFrequency.Text, out double frequency))
            {
                frequency = 1.0;
                txtFrequency.Text = "1.0";
            }

            if (!int.TryParse(txtPoints.Text, out int points))
            {
                points = 1000;
                txtPoints.Text = "1000";
            }

            // Generate data using DataManager
            var (x, y) = DataManager.GenerateData(idx: 1, points: points, amplitude: amplitude, frequency: frequency);

            // Use DataFilterManager to fit to screen (downsampling)
            var filterResult = DataFilterManager.GetFitToScreenData(x, y, plotWidthPixels: 1200, pointsPerPixel: 2);

            // Plot to all plots
            guiManager.LoadFilteredData(filterResult);

            logger.Information($"Plot Fit Screen: {filterResult.Description}");
        }

        private void btnPlotLastN_Click(object sender, EventArgs e)
        {
            // Parse input values
            if (!double.TryParse(txtAmplitude.Text, out double amplitude))
            {
                amplitude = 1.0;
                txtAmplitude.Text = "1.0";
            }

            if (!double.TryParse(txtFrequency.Text, out double frequency))
            {
                frequency = 1.0;
                txtFrequency.Text = "1.0";
            }

            if (!int.TryParse(txtPoints.Text, out int points))
            {
                points = 1000;
                txtPoints.Text = "1000";
            }

            // Parse Last N value
            if (!int.TryParse(txtLastN.Text, out int lastN))
            {
                lastN = 100;
                txtLastN.Text = "100";
            }

            // Generate data using DataManager
            var (x, y) = DataManager.GenerateData(idx: 1, points: points, amplitude: amplitude, frequency: frequency);

            // Use DataFilterManager to get last N data
            var filterResult = DataFilterManager.GetLastNData(x, y, lastN);

            // Plot to all plots
            guiManager.LoadFilteredData(filterResult);

            logger.Information($"Plot Last N: {filterResult.Description}");
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            logger.Information("Form1_FormClosed event triggered - Disposing components");
            
            // Timer'ı durdur
            logUpdateTimer?.Dispose();
            
            // Dispose components
            algoTrader?.Dispose();
            guiManager?.Dispose();
            
            logger.Information("Form1 cleanup completed");
        }
    }
}
