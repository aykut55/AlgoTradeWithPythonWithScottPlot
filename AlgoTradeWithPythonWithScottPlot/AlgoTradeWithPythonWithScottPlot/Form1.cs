using Serilog;

namespace AlgoTradeWithPythonWithScottPlot
{
    public partial class Form1 : Form
    {
        private AlgoTrader algoTrader;
        private readonly ILogger logger = Log.ForContext<Form1>();
        private System.Threading.Timer logUpdateTimer;

        public Form1()
        {
            InitializeComponent();
            logger.Information("Form1 constructor called - Creating AlgoTrader instance");
            algoTrader = new AlgoTrader();
            logger.Information("Form1 initialized successfully");
            
            // Initial status
            btnToggleLogs.Text = "Hide Logs";
            showLogsToolStripMenuItem.Text = "Hide Logs";
            statusLabel.Text = "Application started - Log panel visible";
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
            if (pnlLogs.Visible)
            {
                pnlLogs.Visible = false;
                pnlBottom.Visible = false;
                btnToggleLogs.Text = "Show Logs";
                showLogsToolStripMenuItem.Text = "Show Logs";
                statusLabel.Text = "Log panel hidden";
                logger.Information("Log panel hidden");
            }
            else
            {
                pnlLogs.Visible = true;
                pnlBottom.Visible = true;
                btnToggleLogs.Text = "Hide Logs";
                showLogsToolStripMenuItem.Text = "Hide Logs";
                statusLabel.Text = "Log panel shown";
                logger.Information("Log panel shown");
            }
        }

        private void btnClearLogs_Click(object sender, EventArgs e)
        {
            txtLogs.Clear();
            logger.Information("Log display cleared");
        }

        // Menu event handlers
        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            logger.Information("Exit menu item clicked");
            this.Close();
        }

        private void showLogsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            btnToggleLogs_Click(sender, e);
        }

        private void clearLogsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            btnClearLogs_Click(sender, e);
        }

        private void initToolStripMenuItem_Click(object sender, EventArgs e)
        {
            algoTrader.Init();
        }

        private void startToolStripMenuItem_Click(object sender, EventArgs e)
        {
            algoTrader.Start();
        }

        private void stopToolStripMenuItem_Click(object sender, EventArgs e)
        {
            algoTrader.Stop();
        }

        private void resetToolStripMenuItem_Click(object sender, EventArgs e)
        {
            algoTrader.Reset();
        }

        private void terminateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            algoTrader.Terminate();
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            logger.Information("Form1_FormClosed event triggered - Disposing AlgoTrader");
            
            // Timer'ı durdur
            logUpdateTimer?.Dispose();
            
            algoTrader?.Dispose();
            logger.Information("Form1 cleanup completed");
        }
    }
}
