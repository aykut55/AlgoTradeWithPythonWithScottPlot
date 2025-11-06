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
            
            // Connect GuiManager and AlgoTrader
            algoTrader.SetGuiManager(guiManager);
            
            logger.Information("Form1 initialized successfully");
            
            // Initial status (button text will be set by GuiManager.UpdateLogPanelButtonText)
            guiManager.UpdateStatus("Application started");
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            logger.Information("Form1_Load event triggered - Form is now visible");
            
            // Timer'ı başlat - her 500ms'de logları güncelle
            logUpdateTimer = new System.Threading.Timer(UpdateLogDisplay, null, 0, 500);
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
                    // Sample sine wave with more data points for better pan visibility
                    double[] x = new double[1000];
                    double[] y = new double[1000];
                    var random = new Random();
                    for (int i = 0; i < 1000; i++)
                    {
                        x[i] = i * 0.05; // Wider X range (0 to 50)
                        y[i] = Math.Sin(x[i] * 0.5) + 0.3 * Math.Sin(x[i] * 3) + 0.1 * random.NextDouble(); // Complex wave with noise
                    }
                    plot.Plot.Add.Scatter(x, y);
                    // Clean up the ID for display: "0" -> "Plot 0", "Plot_1" -> "Plot 1"
                    string displayName = createdId.StartsWith("Plot_") ? 
                        $"Plot {createdId.Substring(5)}" : 
                        $"Plot {createdId}";
                    plot.Plot.Title(displayName);
                    plot.Refresh();
                    
                    logger.Information($"Plot {createdId} added and displayed");
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
