namespace AlgoTradeWithPythonWithScottPlot
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            menuStrip1 = new MenuStrip();
            fileToolStripMenuItem = new ToolStripMenuItem();
            exitToolStripMenuItem = new ToolStripMenuItem();
            viewToolStripMenuItem = new ToolStripMenuItem();
            showLogsToolStripMenuItem = new ToolStripMenuItem();
            clearLogsToolStripMenuItem = new ToolStripMenuItem();
            plotsToolStripMenuItem = new ToolStripMenuItem();
            addPlotToolStripMenuItem = new ToolStripMenuItem();
            deletePlotToolStripMenuItem = new ToolStripMenuItem();
            clearAllPlotsToolStripMenuItem = new ToolStripMenuItem();
            hideAllPlotsToolStripMenuItem = new ToolStripMenuItem();
            showAllPlotsToolStripMenuItem = new ToolStripMenuItem();
            algoTraderToolStripMenuItem = new ToolStripMenuItem();
            initToolStripMenuItem = new ToolStripMenuItem();
            startToolStripMenuItem = new ToolStripMenuItem();
            stopToolStripMenuItem = new ToolStripMenuItem();
            resetToolStripMenuItem = new ToolStripMenuItem();
            terminateToolStripMenuItem = new ToolStripMenuItem();
            pnlTop = new Panel();
            btnClearLogs = new Button();
            btnToggleLogs = new Button();
            pnlLeft = new Panel();
            pnlRight = new Panel();
            pnlCenter = new Panel();
            statusStrip1 = new StatusStrip();
            statusLabel = new ToolStripStatusLabel();
            pnlBottom = new Panel();
            pnlLogs = new Panel();
            txtLogs = new TextBox();
            menuStrip1.SuspendLayout();
            pnlTop.SuspendLayout();
            statusStrip1.SuspendLayout();
            pnlBottom.SuspendLayout();
            pnlLogs.SuspendLayout();
            SuspendLayout();
            // 
            // menuStrip1
            // 
            menuStrip1.Items.AddRange(new ToolStripItem[] { fileToolStripMenuItem, viewToolStripMenuItem, plotsToolStripMenuItem, algoTraderToolStripMenuItem });
            menuStrip1.Location = new Point(0, 0);
            menuStrip1.Name = "menuStrip1";
            menuStrip1.Size = new Size(1415, 24);
            menuStrip1.TabIndex = 0;
            menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            fileToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { exitToolStripMenuItem });
            fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            fileToolStripMenuItem.Size = new Size(37, 20);
            fileToolStripMenuItem.Text = "File";
            // 
            // exitToolStripMenuItem
            // 
            exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            exitToolStripMenuItem.Size = new Size(92, 22);
            exitToolStripMenuItem.Text = "Exit";
            exitToolStripMenuItem.Click += exitToolStripMenuItem_Click;
            // 
            // viewToolStripMenuItem
            // 
            viewToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { showLogsToolStripMenuItem, clearLogsToolStripMenuItem });
            viewToolStripMenuItem.Name = "viewToolStripMenuItem";
            viewToolStripMenuItem.Size = new Size(44, 20);
            viewToolStripMenuItem.Text = "View";
            // 
            // showLogsToolStripMenuItem
            // 
            showLogsToolStripMenuItem.Name = "showLogsToolStripMenuItem";
            showLogsToolStripMenuItem.Size = new Size(131, 22);
            showLogsToolStripMenuItem.Text = "Show Logs";
            showLogsToolStripMenuItem.Click += showLogsToolStripMenuItem_Click;
            // 
            // clearLogsToolStripMenuItem
            // 
            clearLogsToolStripMenuItem.Name = "clearLogsToolStripMenuItem";
            clearLogsToolStripMenuItem.Size = new Size(131, 22);
            clearLogsToolStripMenuItem.Text = "Clear Logs";
            clearLogsToolStripMenuItem.Click += clearLogsToolStripMenuItem_Click;
            // 
            // plotsToolStripMenuItem
            // 
            plotsToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { addPlotToolStripMenuItem, deletePlotToolStripMenuItem, clearAllPlotsToolStripMenuItem, hideAllPlotsToolStripMenuItem, showAllPlotsToolStripMenuItem });
            plotsToolStripMenuItem.Name = "plotsToolStripMenuItem";
            plotsToolStripMenuItem.Size = new Size(45, 20);
            plotsToolStripMenuItem.Text = "Plots";
            // 
            // addPlotToolStripMenuItem
            // 
            addPlotToolStripMenuItem.Name = "addPlotToolStripMenuItem";
            addPlotToolStripMenuItem.Size = new Size(153, 22);
            addPlotToolStripMenuItem.Text = "Add Plot";
            addPlotToolStripMenuItem.Click += addPlotToolStripMenuItem_Click;
            // 
            // deletePlotToolStripMenuItem
            // 
            deletePlotToolStripMenuItem.Name = "deletePlotToolStripMenuItem";
            deletePlotToolStripMenuItem.Size = new Size(153, 22);
            deletePlotToolStripMenuItem.Text = "Delete All Plots";
            deletePlotToolStripMenuItem.Click += deletePlotToolStripMenuItem_Click;
            // 
            // clearAllPlotsToolStripMenuItem
            // 
            clearAllPlotsToolStripMenuItem.Name = "clearAllPlotsToolStripMenuItem";
            clearAllPlotsToolStripMenuItem.Size = new Size(153, 22);
            clearAllPlotsToolStripMenuItem.Text = "Clear Data";
            clearAllPlotsToolStripMenuItem.Click += clearAllPlotsToolStripMenuItem_Click;
            // 
            // hideAllPlotsToolStripMenuItem
            // 
            hideAllPlotsToolStripMenuItem.Name = "hideAllPlotsToolStripMenuItem";
            hideAllPlotsToolStripMenuItem.Size = new Size(153, 22);
            hideAllPlotsToolStripMenuItem.Text = "Hide All Plots";
            hideAllPlotsToolStripMenuItem.Click += hideAllPlotsToolStripMenuItem_Click;
            // 
            // showAllPlotsToolStripMenuItem
            // 
            showAllPlotsToolStripMenuItem.Name = "showAllPlotsToolStripMenuItem";
            showAllPlotsToolStripMenuItem.Size = new Size(153, 22);
            showAllPlotsToolStripMenuItem.Text = "Show All Plots";
            showAllPlotsToolStripMenuItem.Click += showAllPlotsToolStripMenuItem_Click;
            // 
            // algoTraderToolStripMenuItem
            // 
            algoTraderToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { initToolStripMenuItem, startToolStripMenuItem, stopToolStripMenuItem, resetToolStripMenuItem, terminateToolStripMenuItem });
            algoTraderToolStripMenuItem.Name = "algoTraderToolStripMenuItem";
            algoTraderToolStripMenuItem.Size = new Size(77, 20);
            algoTraderToolStripMenuItem.Text = "AlgoTrader";
            // 
            // initToolStripMenuItem
            // 
            initToolStripMenuItem.Name = "initToolStripMenuItem";
            initToolStripMenuItem.Size = new Size(127, 22);
            initToolStripMenuItem.Text = "Init";
            initToolStripMenuItem.Click += initToolStripMenuItem_Click;
            // 
            // startToolStripMenuItem
            // 
            startToolStripMenuItem.Name = "startToolStripMenuItem";
            startToolStripMenuItem.Size = new Size(127, 22);
            startToolStripMenuItem.Text = "Start";
            startToolStripMenuItem.Click += startToolStripMenuItem_Click;
            // 
            // stopToolStripMenuItem
            // 
            stopToolStripMenuItem.Name = "stopToolStripMenuItem";
            stopToolStripMenuItem.Size = new Size(127, 22);
            stopToolStripMenuItem.Text = "Stop";
            stopToolStripMenuItem.Click += stopToolStripMenuItem_Click;
            // 
            // resetToolStripMenuItem
            // 
            resetToolStripMenuItem.Name = "resetToolStripMenuItem";
            resetToolStripMenuItem.Size = new Size(127, 22);
            resetToolStripMenuItem.Text = "Reset";
            resetToolStripMenuItem.Click += resetToolStripMenuItem_Click;
            // 
            // terminateToolStripMenuItem
            // 
            terminateToolStripMenuItem.Name = "terminateToolStripMenuItem";
            terminateToolStripMenuItem.Size = new Size(127, 22);
            terminateToolStripMenuItem.Text = "Terminate";
            terminateToolStripMenuItem.Click += terminateToolStripMenuItem_Click;
            // 
            // pnlTop
            // 
            pnlTop.BackColor = Color.LightGray;
            pnlTop.Controls.Add(btnClearLogs);
            pnlTop.Controls.Add(btnToggleLogs);
            pnlTop.Dock = DockStyle.Top;
            pnlTop.Location = new Point(0, 24);
            pnlTop.Name = "pnlTop";
            pnlTop.Size = new Size(1415, 60);
            pnlTop.TabIndex = 0;
            // 
            // btnClearLogs
            // 
            btnClearLogs.Location = new Point(125, 15);
            btnClearLogs.Name = "btnClearLogs";
            btnClearLogs.Size = new Size(100, 30);
            btnClearLogs.TabIndex = 1;
            btnClearLogs.Text = "Clear Logs";
            btnClearLogs.UseVisualStyleBackColor = true;
            btnClearLogs.Click += btnClearLogs_Click;
            // 
            // btnToggleLogs
            // 
            btnToggleLogs.Location = new Point(12, 15);
            btnToggleLogs.Name = "btnToggleLogs";
            btnToggleLogs.Size = new Size(100, 30);
            btnToggleLogs.TabIndex = 0;
            btnToggleLogs.Text = "Show Logs";
            btnToggleLogs.UseVisualStyleBackColor = true;
            btnToggleLogs.Click += btnToggleLogs_Click;
            // 
            // pnlLeft
            // 
            pnlLeft.BackColor = Color.LightBlue;
            pnlLeft.Dock = DockStyle.Left;
            pnlLeft.Location = new Point(0, 84);
            pnlLeft.Name = "pnlLeft";
            pnlLeft.Size = new Size(42, 489);
            pnlLeft.TabIndex = 1;
            pnlLeft.Visible = false;
            // 
            // pnlRight
            // 
            pnlRight.BackColor = Color.LightCoral;
            pnlRight.Dock = DockStyle.Right;
            pnlRight.Location = new Point(1375, 84);
            pnlRight.Name = "pnlRight";
            pnlRight.Size = new Size(40, 489);
            pnlRight.TabIndex = 5;
            pnlRight.Visible = false;
            // 
            // pnlCenter
            // 
            pnlCenter.AutoScroll = true;
            pnlCenter.BackColor = Color.White;
            pnlCenter.Dock = DockStyle.Fill;
            pnlCenter.Location = new Point(42, 84);
            pnlCenter.Name = "pnlCenter";
            pnlCenter.Size = new Size(1333, 335);
            pnlCenter.TabIndex = 3;
            // 
            // statusStrip1
            // 
            statusStrip1.Items.AddRange(new ToolStripItem[] { statusLabel });
            statusStrip1.Location = new Point(0, 573);
            statusStrip1.Name = "statusStrip1";
            statusStrip1.Size = new Size(1415, 22);
            statusStrip1.TabIndex = 6;
            statusStrip1.Text = "statusStrip1";
            // 
            // statusLabel
            // 
            statusLabel.Name = "statusLabel";
            statusLabel.Size = new Size(39, 17);
            statusLabel.Text = "Ready";
            // 
            // pnlBottom
            // 
            pnlBottom.BackColor = Color.LightYellow;
            pnlBottom.Controls.Add(pnlLogs);
            pnlBottom.Dock = DockStyle.Bottom;
            pnlBottom.Location = new Point(42, 419);
            pnlBottom.Name = "pnlBottom";
            pnlBottom.Size = new Size(1333, 154);
            pnlBottom.TabIndex = 7;
            // 
            // pnlLogs
            // 
            pnlLogs.Controls.Add(txtLogs);
            pnlLogs.Dock = DockStyle.Fill;
            pnlLogs.Location = new Point(0, 0);
            pnlLogs.Name = "pnlLogs";
            pnlLogs.Size = new Size(1333, 154);
            pnlLogs.TabIndex = 4;
            // 
            // txtLogs
            // 
            txtLogs.Dock = DockStyle.Fill;
            txtLogs.Font = new Font("Consolas", 9F);
            txtLogs.Location = new Point(0, 0);
            txtLogs.Multiline = true;
            txtLogs.Name = "txtLogs";
            txtLogs.ReadOnly = true;
            txtLogs.ScrollBars = ScrollBars.Both;
            txtLogs.Size = new Size(1333, 154);
            txtLogs.TabIndex = 0;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1415, 595);
            Controls.Add(pnlCenter);
            Controls.Add(pnlBottom);
            Controls.Add(pnlRight);
            Controls.Add(pnlLeft);
            Controls.Add(pnlTop);
            Controls.Add(statusStrip1);
            Controls.Add(menuStrip1);
            MainMenuStrip = menuStrip1;
            Name = "Form1";
            Text = "AlgoTrader";
            WindowState = FormWindowState.Maximized;
            FormClosed += Form1_FormClosed;
            Load += Form1_Load;
            menuStrip1.ResumeLayout(false);
            menuStrip1.PerformLayout();
            pnlTop.ResumeLayout(false);
            statusStrip1.ResumeLayout(false);
            statusStrip1.PerformLayout();
            pnlBottom.ResumeLayout(false);
            pnlLogs.ResumeLayout(false);
            pnlLogs.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private MenuStrip menuStrip1;
        private ToolStripMenuItem fileToolStripMenuItem;
        private ToolStripMenuItem exitToolStripMenuItem;
        private ToolStripMenuItem viewToolStripMenuItem;
        private ToolStripMenuItem showLogsToolStripMenuItem;
        private ToolStripMenuItem clearLogsToolStripMenuItem;
        private ToolStripMenuItem plotsToolStripMenuItem;
        private ToolStripMenuItem addPlotToolStripMenuItem;
        private ToolStripMenuItem deletePlotToolStripMenuItem;
        private ToolStripMenuItem clearAllPlotsToolStripMenuItem;
        private ToolStripMenuItem hideAllPlotsToolStripMenuItem;
        private ToolStripMenuItem showAllPlotsToolStripMenuItem;
        private ToolStripMenuItem algoTraderToolStripMenuItem;
        private ToolStripMenuItem initToolStripMenuItem;
        private ToolStripMenuItem startToolStripMenuItem;
        private ToolStripMenuItem stopToolStripMenuItem;
        private ToolStripMenuItem resetToolStripMenuItem;
        private ToolStripMenuItem terminateToolStripMenuItem;
        private Panel pnlTop;
        private Button btnToggleLogs;
        private Button btnClearLogs;
        private Panel pnlLeft;
        private Panel pnlRight;
        private Panel pnlCenter;
        private StatusStrip statusStrip1;
        private ToolStripStatusLabel statusLabel;
        private Panel pnlBottom;
        private Panel pnlLogs;
        private TextBox txtLogs;
    }
}
