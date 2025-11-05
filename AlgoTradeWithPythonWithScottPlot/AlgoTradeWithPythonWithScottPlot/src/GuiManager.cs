using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Serilog;
using ScottPlot.WinForms;

namespace AlgoTradeWithPythonWithScottPlot
{
    public class GuiManager : IDisposable
    {
        private readonly ILogger logger = Log.ForContext<GuiManager>();
        private bool disposed = false;

        // Form references
        private Form1 mainForm;
        private Panel pnlLeft;
        private Panel pnlRight;
        private Panel pnlCenter;
        private Panel pnlBottom;
        private Panel pnlLogs;
        private Panel pnlTop;
        private TextBox txtLogs;
        private FormsPlot formsPlot;
        private ToolStripStatusLabel statusLabel;
        private Button btnToggleLogs;
        private ToolStripMenuItem showLogsMenuItem;

        // Plot management
        private readonly ConcurrentDictionary<string, PlotInfo> plots;
        private int plotCounter = 0;
        private const int MAIN_PLOT_HEIGHT = 600;
        private const int SECONDARY_PLOT_HEIGHT = 500;
        private const string MAIN_PLOT_ID = "0";

        // Events for AlgoTrader interaction
        public event EventHandler? InitRequested;
        public event EventHandler? StartRequested;
        public event EventHandler? StopRequested;
        public event EventHandler? ResetRequested;
        public event EventHandler? TerminateRequested;

        // Global control buttons (affecting all plots)
        private Button? globalYPanUpButton;
        private Button? globalYZoomInButton;
        private Button? globalYZoomOutButton;
        private Button? globalYPanDownButton;
        private Button? globalXPanLeftButton;
        private Button? globalXZoomInButton;
        private Button? globalXZoomOutButton;
        private Button? globalXPanRightButton;
        private Button? globalResetYButton;
        private Button? globalResetButton;
        private Button? globalResetXButton;

        // Global sync checkboxes
        private CheckBox? syncZoomCheckBox;
        private CheckBox? syncPanCheckBox;
        private CheckBox? syncMouseWheelCheckBox;
        private CheckBox? syncResetCheckBox;
        private CheckBox? syncMouseDragCheckBox;
        private CheckBox? syncAxisLimitsCheckBox;
        private CheckBox? enableScrollbarCheckBox;

        // Throttling for real-time sync
        private DateTime lastMouseMoveSync = DateTime.MinValue;
        private const int MOUSE_MOVE_SYNC_THROTTLE_MS = 50; // 50ms throttle

        public GuiManager()
        {
            plots = new ConcurrentDictionary<string, PlotInfo>();
            logger.Information("GuiManager instance created");
        }

        public void Initialize(Form1 form)
        {
            mainForm = form;
            logger.Information("GuiManager initialized with Form1 reference");
            
            // Cache form control references for performance
            CacheControlReferences();
            
            // Create global control buttons
            CreateGlobalControls();
            
            // Prevent mouse wheel scrolling on center panel when over plots
            if (pnlCenter != null)
            {
                pnlCenter.MouseWheel += PnlCenter_MouseWheel;
            }
        }

        private void CacheControlReferences()
        {
            if (mainForm == null) return;

            // Get references to panels
            pnlLeft = GetControl<Panel>("pnlLeft");
            pnlRight = GetControl<Panel>("pnlRight");
            pnlCenter = GetControl<Panel>("pnlCenter");
            pnlBottom = GetControl<Panel>("pnlBottom");
            pnlLogs = GetControl<Panel>("pnlLogs");
            
            // Get reference to top panel for global controls
            pnlTop = GetControl<Panel>("pnlTop");
            
            // Get references to other controls
            txtLogs = GetControl<TextBox>("txtLogs");
            formsPlot = GetControl<FormsPlot>("formsPlot1");
            statusLabel = GetControl<ToolStripStatusLabel>("statusLabel");
            btnToggleLogs = GetControl<Button>("btnToggleLogs");
            showLogsMenuItem = GetControl<ToolStripMenuItem>("showLogsToolStripMenuItem");

            logger.Information("Control references cached successfully");
        }

        private T GetControl<T>(string name) where T : class
        {
            var controls = mainForm.Controls.Find(name, true);
            if (controls.Length > 0 && controls[0] is T control)
                return control;

            // Check menu items separately
            if (typeof(T) == typeof(ToolStripMenuItem))
            {
                return FindMenuItemByName(name) as T;
            }

            // Check status strip items
            if (typeof(T) == typeof(ToolStripStatusLabel))
            {
                return FindStatusLabelByName(name) as T;
            }

            logger.Warning($"Control '{name}' of type '{typeof(T).Name}' not found");
            return null;
        }

        private ToolStripMenuItem FindMenuItemByName(string name)
        {
            var menuStrip = mainForm.Controls.OfType<MenuStrip>().FirstOrDefault();
            if (menuStrip != null)
            {
                foreach (ToolStripMenuItem item in menuStrip.Items)
                {
                    var found = FindMenuItemRecursive(item, name);
                    if (found != null) return found;
                }
            }
            return null;
        }

        private ToolStripMenuItem FindMenuItemRecursive(ToolStripMenuItem parent, string name)
        {
            if (parent.Name == name) return parent;
            
            foreach (ToolStripItem item in parent.DropDownItems)
            {
                if (item is ToolStripMenuItem menuItem)
                {
                    if (menuItem.Name == name) return menuItem;
                    var found = FindMenuItemRecursive(menuItem, name);
                    if (found != null) return found;
                }
            }
            return null;
        }

        private ToolStripStatusLabel FindStatusLabelByName(string name)
        {
            var statusStrip = mainForm.Controls.OfType<StatusStrip>().FirstOrDefault();
            return statusStrip?.Items.OfType<ToolStripStatusLabel>().FirstOrDefault(x => x.Name == name);
        }

        // GUI Control Methods
        public void UpdateStatus(string message)
        {
            if (statusLabel != null && mainForm != null)
            {
                if (mainForm.InvokeRequired)
                {
                    mainForm.Invoke(() => statusLabel.Text = message);
                }
                else
                {
                    statusLabel.Text = message;
                }
                logger.Debug($"Status updated: {message}");
            }
        }

        public void ShowPanel(string panelName)
        {
            var panel = GetPanelByName(panelName);
            if (panel != null)
            {
                SetPanelVisibility(panel, true);
                logger.Information($"Panel '{panelName}' shown");
            }
        }

        public void HidePanel(string panelName)
        {
            var panel = GetPanelByName(panelName);
            if (panel != null)
            {
                SetPanelVisibility(panel, false);
                logger.Information($"Panel '{panelName}' hidden");
            }
        }

        public void ToggleLogPanel()
        {
            if (pnlLogs != null && pnlBottom != null)
            {
                bool isVisible = pnlLogs.Visible;
                SetPanelVisibility(pnlLogs, !isVisible);
                SetPanelVisibility(pnlBottom, !isVisible);
                
                // Update button and menu text
                string newText = isVisible ? "Show Logs" : "Hide Logs";
                if (btnToggleLogs != null) btnToggleLogs.Text = newText;
                if (showLogsMenuItem != null) showLogsMenuItem.Text = newText;
                
                UpdateStatus(isVisible ? "Log panel hidden" : "Log panel shown");
                logger.Information($"Log panel toggled - now {(isVisible ? "hidden" : "shown")}");
            }
        }

        public void ClearLogs()
        {
            if (txtLogs != null)
            {
                if (mainForm.InvokeRequired)
                {
                    mainForm.Invoke(() => txtLogs.Clear());
                }
                else
                {
                    txtLogs.Clear();
                }
                UpdateStatus("Logs cleared");
                logger.Information("Logs cleared via GuiManager");
            }
        }

        public void UpdateChart(Action<FormsPlot> updateAction)
        {
            if (formsPlot != null && mainForm != null)
            {
                if (mainForm.InvokeRequired)
                {
                    mainForm.Invoke(() => updateAction(formsPlot));
                }
                else
                {
                    updateAction(formsPlot);
                }
                logger.Debug("Chart updated via GuiManager");
            }
        }

        private Panel GetPanelByName(string name)
        {
            return name.ToLower() switch
            {
                "left" => pnlLeft,
                "right" => pnlRight,
                "center" => pnlCenter,
                "bottom" => pnlBottom,
                "logs" => pnlLogs,
                _ => null
            };
        }

        private void SetPanelVisibility(Panel panel, bool visible)
        {
            if (panel != null && mainForm != null)
            {
                if (mainForm.InvokeRequired)
                {
                    mainForm.Invoke(() => panel.Visible = visible);
                }
                else
                {
                    panel.Visible = visible;
                }
            }
        }

        // Plot Management Methods
        public string AddPlot(string id = null, DockStyle dock = DockStyle.Top, Panel parentPanel = null, int height = 0)
        {
            // Determine if this is the main plot or secondary plot
            bool isMainPlot = plots.Count == 0;
            
            if (string.IsNullOrEmpty(id))
            {
                if (isMainPlot)
                {
                    id = MAIN_PLOT_ID;
                }
                else
                {
                    id = $"Plot_{++plotCounter}";
                }
            }

            if (plots.ContainsKey(id))
            {
                logger.Warning($"Plot with ID '{id}' already exists");
                return null;
            }

            // Set height based on plot type
            if (height == 0)
            {
                height = isMainPlot ? MAIN_PLOT_HEIGHT : SECONDARY_PLOT_HEIGHT;
            }

            try
            {
                var plotInfo = new PlotInfo(id)
                {
                    DockStyle = dock
                };

                // Position will be automatically calculated by DockStyle.Top
                
                // Create container panel with fixed size for stacking
                plotInfo.Container = new Panel
                {
                    Name = $"pnl_{id}",
                    BackColor = plotInfo.BackgroundColor,
                    BorderStyle = BorderStyle.FixedSingle
                };

                // Set size and position based on plot count
                if (isMainPlot)
                {
                    // Main plot - can fill the space initially
                    plotInfo.Container.Dock = DockStyle.Fill;
                }
                else
                {
                    // Secondary plots - fixed height, stacked vertically with Top dock
                    plotInfo.Container.Dock = DockStyle.Top;
                    plotInfo.Container.Height = height;
                }

                // Create FormsPlot (with margins for zoom controls)
                plotInfo.Plot = new FormsPlot
                {
                    Name = $"plot_{id}",
                    Dock = DockStyle.None,
                    Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom,
                    DisplayScale = 1F
                };

                // Create zoom controls
                CreateZoomControls(plotInfo);

                // Add all controls to container
                plotInfo.Container.Controls.Add(plotInfo.Plot);
                plotInfo.Container.Controls.Add(plotInfo.YZoomInButton);
                plotInfo.Container.Controls.Add(plotInfo.YZoomOutButton);
                plotInfo.Container.Controls.Add(plotInfo.XZoomInButton);
                plotInfo.Container.Controls.Add(plotInfo.XZoomOutButton);
                plotInfo.Container.Controls.Add(plotInfo.YPanUpButton);
                plotInfo.Container.Controls.Add(plotInfo.YPanDownButton);
                plotInfo.Container.Controls.Add(plotInfo.XPanLeftButton);
                plotInfo.Container.Controls.Add(plotInfo.XPanRightButton);
                plotInfo.Container.Controls.Add(plotInfo.ResetButton);
                plotInfo.Container.Controls.Add(plotInfo.ResetXButton);
                plotInfo.Container.Controls.Add(plotInfo.ResetYButton);

                // Add container to parent panel (default: pnlCenter)
                Panel parent = parentPanel ?? pnlCenter;
                if (parent != null)
                {
                    if (mainForm.InvokeRequired)
                    {
                        mainForm.Invoke(() => {
                            parent.Controls.Add(plotInfo.Container);
                            if (plots.Count > 0) // Not the first plot
                            {
                                plotInfo.Container.BringToFront();
                            }
                        });
                    }
                    else
                    {
                        parent.Controls.Add(plotInfo.Container);
                        if (plots.Count > 0) // Not the first plot
                        {
                            plotInfo.Container.BringToFront();
                        }
                    }
                }

                // Store plot info
                plots[id] = plotInfo;

                // Convert main plot to fixed height when adding the second plot (Plot_1)
                if (plots.Count == 2 && plots.ContainsKey(MAIN_PLOT_ID) && !isMainPlot)
                {
                    var mainPlot = plots[MAIN_PLOT_ID];
                    if (mainForm.InvokeRequired)
                    {
                        mainForm.Invoke(() => ConvertMainPlotToFixed(mainPlot));
                    }
                    else
                    {
                        ConvertMainPlotToFixed(mainPlot);
                    }
                    logger.Debug("Converted main plot to fixed height");
                }

                // Update AutoScroll after adding the plot to get correct count
                Panel scrollParent = parentPanel ?? pnlCenter;
                if (scrollParent != null)
                {
                    if (mainForm.InvokeRequired)
                    {
                        mainForm.Invoke(() => UpdateParentAutoScrollMinSize(scrollParent));
                    }
                    else
                    {
                        UpdateParentAutoScrollMinSize(scrollParent);
                    }
                }

                logger.Information($"Plot '{id}' added successfully with Height={height}");
                UpdateStatus($"Plot '{id}' created");
                return id;
            }
            catch (Exception ex)
            {
                logger.Error($"Failed to add plot '{id}': {ex.Message}");
                return null;
            }
        }

        private void ConvertMainPlotToFixed(PlotInfo mainPlot)
        {
            mainPlot.Container.Dock = DockStyle.Top;
            mainPlot.Container.Height = MAIN_PLOT_HEIGHT;
        }

        private void CreateZoomControls(PlotInfo plotInfo)
        {
            const int buttonSize = 25;
            const int margin = 3;
            const int yControlsWidth = buttonSize + margin;
            
            // Store original limits for reset functionality
            if (plotInfo.Plot.Plot.Axes.GetLimits() != null)
            {
                // Will be set after data is added to plot
            }

            // Y-axis controls (left side, vertical stack)
            int yStartY = (plotInfo.Container.Height / 2) - (2 * buttonSize + margin);
            
            plotInfo.YPanUpButton = new Button
            {
                Name = $"btnYPanUp_{plotInfo.Id}",
                Text = "↑",
                Size = new Size(buttonSize, buttonSize),
                Location = new Point(margin, yStartY),
                Anchor = AnchorStyles.Left,
                BackColor = Color.LightSkyBlue,
                ForeColor = Color.Black,
                Font = new Font("Arial", 10, FontStyle.Bold)
            };

            plotInfo.YZoomInButton = new Button
            {
                Name = $"btnYZoomIn_{plotInfo.Id}",
                Text = "Y+",
                Size = new Size(buttonSize, buttonSize),
                Location = new Point(margin, yStartY + buttonSize + margin),
                Anchor = AnchorStyles.Left,
                BackColor = Color.LightBlue,
                ForeColor = Color.Black,
                Font = new Font("Arial", 7, FontStyle.Bold)
            };

            plotInfo.YZoomOutButton = new Button
            {
                Name = $"btnYZoomOut_{plotInfo.Id}",
                Text = "Y-",
                Size = new Size(buttonSize, buttonSize),
                Location = new Point(margin, yStartY + 2 * (buttonSize + margin)),
                Anchor = AnchorStyles.Left,
                BackColor = Color.LightCoral,
                ForeColor = Color.Black,
                Font = new Font("Arial", 7, FontStyle.Bold)
            };

            plotInfo.YPanDownButton = new Button
            {
                Name = $"btnYPanDown_{plotInfo.Id}",
                Text = "↓",
                Size = new Size(buttonSize, buttonSize),
                Location = new Point(margin, yStartY + 3 * (buttonSize + margin)),
                Anchor = AnchorStyles.Left,
                BackColor = Color.LightSkyBlue,
                ForeColor = Color.Black,
                Font = new Font("Arial", 10, FontStyle.Bold)
            };

            // X-axis controls (top right, horizontal stack)
            int xStartX = plotInfo.Container.Width - (4 * buttonSize + 4 * margin);
            
            plotInfo.XPanLeftButton = new Button
            {
                Name = $"btnXPanLeft_{plotInfo.Id}",
                Text = "←",
                Size = new Size(buttonSize, buttonSize),
                Location = new Point(xStartX, margin),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                BackColor = Color.LightGreen,
                ForeColor = Color.Black,
                Font = new Font("Arial", 10, FontStyle.Bold)
            };

            plotInfo.XZoomInButton = new Button
            {
                Name = $"btnXZoomIn_{plotInfo.Id}",
                Text = "X+",
                Size = new Size(buttonSize, buttonSize),
                Location = new Point(xStartX + buttonSize + margin, margin),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                BackColor = Color.LightGreen,
                ForeColor = Color.Black,
                Font = new Font("Arial", 7, FontStyle.Bold)
            };

            plotInfo.XZoomOutButton = new Button
            {
                Name = $"btnXZoomOut_{plotInfo.Id}",
                Text = "X-",
                Size = new Size(buttonSize, buttonSize),
                Location = new Point(xStartX + 2 * (buttonSize + margin), margin),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                BackColor = Color.LightYellow,
                ForeColor = Color.Black,
                Font = new Font("Arial", 7, FontStyle.Bold)
            };

            plotInfo.XPanRightButton = new Button
            {
                Name = $"btnXPanRight_{plotInfo.Id}",
                Text = "→",
                Size = new Size(buttonSize, buttonSize),
                Location = new Point(xStartX + 3 * (buttonSize + margin), margin),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                BackColor = Color.LightGreen,
                ForeColor = Color.Black,
                Font = new Font("Arial", 10, FontStyle.Bold)
            };

            // Reset buttons (top left area): ResetY - Reset - ResetX
            int resetStartX = yControlsWidth + margin * 2;
            
            plotInfo.ResetYButton = new Button
            {
                Name = $"btnResetY_{plotInfo.Id}",
                Text = "RY",
                Size = new Size(buttonSize, buttonSize),
                Location = new Point(resetStartX, margin),
                Anchor = AnchorStyles.Top | AnchorStyles.Left,
                BackColor = Color.LightSalmon,
                ForeColor = Color.Black,
                Font = new Font("Arial", 7, FontStyle.Bold)
            };

            plotInfo.ResetButton = new Button
            {
                Name = $"btnReset_{plotInfo.Id}",
                Text = "⟲",
                Size = new Size(buttonSize + 5, buttonSize),
                Location = new Point(resetStartX + buttonSize + margin, margin),
                Anchor = AnchorStyles.Top | AnchorStyles.Left,
                BackColor = Color.Orange,
                ForeColor = Color.Black,
                Font = new Font("Arial", 10, FontStyle.Bold)
            };

            plotInfo.ResetXButton = new Button
            {
                Name = $"btnResetX_{plotInfo.Id}",
                Text = "RX",
                Size = new Size(buttonSize, buttonSize),
                Location = new Point(resetStartX + 2 * buttonSize + margin + 8, margin),
                Anchor = AnchorStyles.Top | AnchorStyles.Left,
                BackColor = Color.LightSalmon,
                ForeColor = Color.Black,
                Font = new Font("Arial", 7, FontStyle.Bold)
            };

            // Set plot margins to avoid overlap with controls
            int leftMargin = yControlsWidth + 2 * margin;
            int topMargin = buttonSize + 2 * margin;
            int rightMargin = 4 * buttonSize + 5 * margin;
            int bottomMargin = margin;

            plotInfo.Plot.Location = new Point(leftMargin, topMargin);
            plotInfo.Plot.Size = new Size(
                plotInfo.Container.Width - leftMargin - rightMargin,
                plotInfo.Container.Height - topMargin - bottomMargin
            );

            // Add event handlers for zoom functionality
            plotInfo.YZoomInButton.Click += (sender, e) => ZoomY(plotInfo, 0.8);
            plotInfo.YZoomOutButton.Click += (sender, e) => ZoomY(plotInfo, 1.25);
            plotInfo.XZoomInButton.Click += (sender, e) => ZoomX(plotInfo, 0.8);
            plotInfo.XZoomOutButton.Click += (sender, e) => ZoomX(plotInfo, 1.25);
            
            // Add event handlers for pan functionality
            plotInfo.YPanUpButton.Click += (sender, e) => PanY(plotInfo, 0.2); // Pan up
            plotInfo.YPanDownButton.Click += (sender, e) => PanY(plotInfo, -0.2); // Pan down
            plotInfo.XPanLeftButton.Click += (sender, e) => PanX(plotInfo, -0.2); // Pan left
            plotInfo.XPanRightButton.Click += (sender, e) => PanX(plotInfo, 0.2); // Pan right
            
            // Add event handlers for reset functionality
            plotInfo.ResetButton.Click += (sender, e) => ResetPlotView(plotInfo);
            plotInfo.ResetXButton.Click += (sender, e) => ResetPlotViewX(plotInfo);
            plotInfo.ResetYButton.Click += (sender, e) => ResetPlotViewY(plotInfo);
            
            // Add mouse synchronization event handlers
            plotInfo.Plot.MouseUp += (sender, e) => OnPlotMouseUp(plotInfo);
            plotInfo.Plot.MouseWheel += (sender, e) => OnPlotMouseWheel(plotInfo, e);
            plotInfo.Plot.MouseMove += (sender, e) => OnPlotMouseMove(plotInfo, e);
            plotInfo.Plot.Refresh();
        }

        private void ZoomX(PlotInfo plotInfo, double factor)
        {
            try
            {
                if (plotInfo?.Plot?.Plot == null) return;

                if (mainForm.InvokeRequired)
                {
                    mainForm.Invoke(() => PerformXZoom(plotInfo, factor));
                }
                else
                {
                    PerformXZoom(plotInfo, factor);
                }
            }
            catch (Exception ex)
            {
                logger.Error($"Error during X zoom for plot {plotInfo.Id}: {ex.Message}");
            }
        }

        private void ZoomY(PlotInfo plotInfo, double factor)
        {
            try
            {
                if (plotInfo?.Plot?.Plot == null) return;

                if (mainForm.InvokeRequired)
                {
                    mainForm.Invoke(() => PerformYZoom(plotInfo, factor));
                }
                else
                {
                    PerformYZoom(plotInfo, factor);
                }
            }
            catch (Exception ex)
            {
                logger.Error($"Error during Y zoom for plot {plotInfo.Id}: {ex.Message}");
            }
        }

        private void PerformXZoom(PlotInfo plotInfo, double factor)
        {
            var plot = plotInfo.Plot.Plot;
            var currentLimits = plot.Axes.GetLimits();
            
            double centerX = (currentLimits.Left + currentLimits.Right) / 2;
            double rangeX = (currentLimits.Right - currentLimits.Left) * factor;
            
            double newLeft = centerX - rangeX / 2;
            double newRight = centerX + rangeX / 2;
            
            plot.Axes.SetLimitsX(newLeft, newRight);
            plotInfo.Plot.Refresh();
            
            logger.Debug($"X zoom applied to plot {plotInfo.Id}: factor={factor}, range={newLeft:F2} to {newRight:F2}");
        }

        private void PerformYZoom(PlotInfo plotInfo, double factor)
        {
            var plot = plotInfo.Plot.Plot;
            var currentLimits = plot.Axes.GetLimits();
            
            double centerY = (currentLimits.Bottom + currentLimits.Top) / 2;
            double rangeY = (currentLimits.Top - currentLimits.Bottom) * factor;
            
            double newBottom = centerY - rangeY / 2;
            double newTop = centerY + rangeY / 2;
            
            plot.Axes.SetLimitsY(newBottom, newTop);
            plotInfo.Plot.Refresh();
            
            logger.Debug($"Y zoom applied to plot {plotInfo.Id}: factor={factor}, range={newBottom:F2} to {newTop:F2}");
        }

        private void PnlCenter_MouseWheel(object? sender, MouseEventArgs e)
        {
            // If mouse wheel scrolling is disabled (checkbox unchecked), prevent mouse wheel scrolling
            // Note: Manual scrollbar usage is always allowed, only mouse wheel is controlled
            if (enableScrollbarCheckBox?.Checked != true)
            {
                // Block mouse wheel scrolling when checkbox is disabled (manual scrollbar still works)
                return;
            }
            
            // If mouse wheel scrolling is enabled, check if mouse is over any plot control
            foreach (var plot in plots.Values)
            {
                if (plot.Plot != null && plot.Plot.Visible)
                {
                    // Get plot's screen bounds
                    var plotBounds = plot.Plot.RectangleToScreen(plot.Plot.ClientRectangle);
                    
                    // If mouse cursor is over this plot, don't scroll the container
                    if (plotBounds.Contains(Cursor.Position))
                    {
                        // Cancel the scroll event by not calling the base implementation
                        return;
                    }
                }
            }
            
            // If mouse is not over any plot and scrollbar is enabled, allow normal scrolling
            // The event will bubble up normally
        }

        private void OnPlotMouseUp(PlotInfo sourcePlot)
        {
            try
            {
                // Check if sync is enabled for any operation
                if (syncZoomCheckBox?.Checked != true && syncPanCheckBox?.Checked != true)
                    return;

                // Get the current limits of the source plot
                var sourceLimits = sourcePlot.Plot.Plot.Axes.GetLimits();
                
                // Apply to all other plots
                foreach (var plot in plots.Values)
                {
                    if (plot.Id == sourcePlot.Id) continue; // Skip source plot
                    
                    try
                    {
                        var currentLimits = plot.Plot.Plot.Axes.GetLimits();
                        bool shouldUpdate = false;
                        
                        // Check if sync pan is enabled and limits have changed
                        if (syncPanCheckBox?.Checked == true)
                        {
                            // Check if pan occurred (center position changed)
                            double sourceCenterX = (sourceLimits.Left + sourceLimits.Right) / 2;
                            double sourceCenterY = (sourceLimits.Bottom + sourceLimits.Top) / 2;
                            double currentCenterX = (currentLimits.Left + currentLimits.Right) / 2;
                            double currentCenterY = (currentLimits.Bottom + currentLimits.Top) / 2;
                            
                            // Preserve current plot's zoom level but apply source's center position
                            double currentRangeX = currentLimits.Right - currentLimits.Left;
                            double currentRangeY = currentLimits.Top - currentLimits.Bottom;
                            
                            double newLeft = sourceCenterX - currentRangeX / 2;
                            double newRight = sourceCenterX + currentRangeX / 2;
                            double newBottom = sourceCenterY - currentRangeY / 2;
                            double newTop = sourceCenterY + currentRangeY / 2;
                            
                            plot.Plot.Plot.Axes.SetLimits(newLeft, newRight, newBottom, newTop);
                            shouldUpdate = true;
                        }
                        
                        // Check if sync zoom is enabled
                        if (syncZoomCheckBox?.Checked == true)
                        {
                            // Apply source plot's exact limits (both position and zoom)
                            plot.Plot.Plot.Axes.SetLimits(sourceLimits);
                            shouldUpdate = true;
                        }
                        
                        if (shouldUpdate)
                        {
                            if (mainForm.InvokeRequired)
                            {
                                mainForm.Invoke(() => plot.Plot.Refresh());
                            }
                            else
                            {
                                plot.Plot.Refresh();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.Warning($"Failed to sync plot {plot.Id}: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error($"Error in OnPlotMouseUp: {ex.Message}");
            }
        }

        private void OnPlotMouseWheel(PlotInfo sourcePlot, MouseEventArgs e)
        {
            try
            {
                // Check if mouse wheel sync is enabled
                if (syncMouseWheelCheckBox?.Checked != true)
                    return;

                // Get the current limits of the source plot after wheel zoom
                var sourceLimits = sourcePlot.Plot.Plot.Axes.GetLimits();
                
                // Apply to all other plots
                foreach (var plot in plots.Values)
                {
                    if (plot.Id == sourcePlot.Id) continue; // Skip source plot
                    
                    try
                    {
                        // Apply source plot's exact limits (both position and zoom)
                        plot.Plot.Plot.Axes.SetLimits(sourceLimits);
                        
                        if (mainForm.InvokeRequired)
                        {
                            mainForm.Invoke(() => plot.Plot.Refresh());
                        }
                        else
                        {
                            plot.Plot.Refresh();
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.Warning($"Failed to sync mouse wheel for plot {plot.Id}: {ex.Message}");
                    }
                }
                
                logger.Debug($"Mouse wheel sync applied from plot {sourcePlot.Id}");
            }
            catch (Exception ex)
            {
                logger.Error($"Error in OnPlotMouseWheel: {ex.Message}");
            }
        }

        private void OnPlotMouseMove(PlotInfo sourcePlot, MouseEventArgs e)
        {
            try
            {
                // Throttle the sync to avoid too frequent updates
                var now = DateTime.Now;
                if ((now - lastMouseMoveSync).TotalMilliseconds < MOUSE_MOVE_SYNC_THROTTLE_MS)
                    return;
                
                lastMouseMoveSync = now;

                // Check if real-time sync is enabled (either drag or pan)
                bool syncDrag = syncMouseDragCheckBox?.Checked == true;
                bool syncPan = syncPanCheckBox?.Checked == true;
                
                if (!syncDrag && !syncPan)
                    return;

                // Only sync if mouse button is pressed (dragging)
                if (e.Button == MouseButtons.None)
                    return;

                // Get the current limits of the source plot
                var sourceLimits = sourcePlot.Plot.Plot.Axes.GetLimits();
                
                // Apply to all other plots
                foreach (var plot in plots.Values)
                {
                    if (plot.Id == sourcePlot.Id) continue; // Skip source plot
                    
                    try
                    {
                        var currentLimits = plot.Plot.Plot.Axes.GetLimits();
                        bool shouldUpdate = false;
                        
                        // For pan sync during drag: preserve zoom level, apply center position
                        if (syncPan && e.Button == MouseButtons.Left)
                        {
                            double sourceCenterX = (sourceLimits.Left + sourceLimits.Right) / 2;
                            double sourceCenterY = (sourceLimits.Bottom + sourceLimits.Top) / 2;
                            
                            double currentRangeX = currentLimits.Right - currentLimits.Left;
                            double currentRangeY = currentLimits.Top - currentLimits.Bottom;
                            
                            double newLeft = sourceCenterX - currentRangeX / 2;
                            double newRight = sourceCenterX + currentRangeX / 2;
                            double newBottom = sourceCenterY - currentRangeY / 2;
                            double newTop = sourceCenterY + currentRangeY / 2;
                            
                            plot.Plot.Plot.Axes.SetLimits(newLeft, newRight, newBottom, newTop);
                            shouldUpdate = true;
                        }
                        
                        // For drag sync: apply exact limits (both position and zoom)
                        if (syncDrag && (e.Button == MouseButtons.Right || e.Button == MouseButtons.Middle))
                        {
                            plot.Plot.Plot.Axes.SetLimits(sourceLimits);
                            shouldUpdate = true;
                        }
                        
                        if (shouldUpdate)
                        {
                            if (mainForm.InvokeRequired)
                            {
                                mainForm.Invoke(() => plot.Plot.Refresh());
                            }
                            else
                            {
                                plot.Plot.Refresh();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.Warning($"Failed to sync mouse move for plot {plot.Id}: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error($"Error in OnPlotMouseMove: {ex.Message}");
            }
        }

        private void PanX(PlotInfo plotInfo, double factor)
        {
            try
            {
                if (plotInfo?.Plot?.Plot == null) return;

                if (mainForm.InvokeRequired)
                {
                    mainForm.Invoke(() => PerformXPan(plotInfo, factor));
                }
                else
                {
                    PerformXPan(plotInfo, factor);
                }
            }
            catch (Exception ex)
            {
                logger.Error($"Error during X pan for plot {plotInfo.Id}: {ex.Message}");
            }
        }

        private void PanY(PlotInfo plotInfo, double factor)
        {
            try
            {
                if (plotInfo?.Plot?.Plot == null) return;

                if (mainForm.InvokeRequired)
                {
                    mainForm.Invoke(() => PerformYPan(plotInfo, factor));
                }
                else
                {
                    PerformYPan(plotInfo, factor);
                }
            }
            catch (Exception ex)
            {
                logger.Error($"Error during Y pan for plot {plotInfo.Id}: {ex.Message}");
            }
        }

        private void PerformXPan(PlotInfo plotInfo, double factor)
        {
            var plot = plotInfo.Plot.Plot;
            var currentLimits = plot.Axes.GetLimits();
            
            double rangeX = currentLimits.Right - currentLimits.Left;
            double panDistance = rangeX * factor;
            
            double newLeft = currentLimits.Left + panDistance;
            double newRight = currentLimits.Right + panDistance;
            
            plot.Axes.SetLimitsX(newLeft, newRight);
            plotInfo.Plot.Refresh();
            
            logger.Debug($"X pan applied to plot {plotInfo.Id}: factor={factor}, new range={newLeft:F2} to {newRight:F2}");
        }

        private void PerformYPan(PlotInfo plotInfo, double factor)
        {
            var plot = plotInfo.Plot.Plot;
            var currentLimits = plot.Axes.GetLimits();
            
            double rangeY = currentLimits.Top - currentLimits.Bottom;
            double panDistance = rangeY * factor;
            
            double newBottom = currentLimits.Bottom + panDistance;
            double newTop = currentLimits.Top + panDistance;
            
            plot.Axes.SetLimitsY(newBottom, newTop);
            plotInfo.Plot.Refresh();
            
            logger.Debug($"Y pan applied to plot {plotInfo.Id}: factor={factor}, new range={newBottom:F2} to {newTop:F2}");
        }

        private void ResetPlotView(PlotInfo plotInfo)
        {
            try
            {
                if (plotInfo?.Plot?.Plot == null) return;

                if (mainForm.InvokeRequired)
                {
                    mainForm.Invoke(() => PerformPlotReset(plotInfo));
                }
                else
                {
                    PerformPlotReset(plotInfo);
                }
            }
            catch (Exception ex)
            {
                logger.Error($"Error during plot reset for plot {plotInfo.Id}: {ex.Message}");
            }
        }

        private void PerformPlotReset(PlotInfo plotInfo)
        {
            var plot = plotInfo.Plot.Plot;
            
            // Auto-scale to fit all data
            plot.Axes.AutoScale();
            plotInfo.Plot.Refresh();
            
            logger.Debug($"Plot {plotInfo.Id} view reset to auto-scale");
        }

        private void ResetPlotViewX(PlotInfo plotInfo)
        {
            try
            {
                if (plotInfo?.Plot?.Plot == null) return;

                if (mainForm.InvokeRequired)
                {
                    mainForm.Invoke(() => PerformPlotResetX(plotInfo));
                }
                else
                {
                    PerformPlotResetX(plotInfo);
                }
            }
            catch (Exception ex)
            {
                logger.Error($"Error during X reset for plot {plotInfo.Id}: {ex.Message}");
            }
        }

        private void ResetPlotViewY(PlotInfo plotInfo)
        {
            try
            {
                if (plotInfo?.Plot?.Plot == null) return;

                if (mainForm.InvokeRequired)
                {
                    mainForm.Invoke(() => PerformPlotResetY(plotInfo));
                }
                else
                {
                    PerformPlotResetY(plotInfo);
                }
            }
            catch (Exception ex)
            {
                logger.Error($"Error during Y reset for plot {plotInfo.Id}: {ex.Message}");
            }
        }

        private void PerformPlotResetX(PlotInfo plotInfo)
        {
            var plot = plotInfo.Plot.Plot;
            
            // Auto-scale X axis only
            plot.Axes.AutoScaleX();
            plotInfo.Plot.Refresh();
            
            logger.Debug($"Plot {plotInfo.Id} X-axis reset to auto-scale");
        }

        private void PerformPlotResetY(PlotInfo plotInfo)
        {
            var plot = plotInfo.Plot.Plot;
            
            // Auto-scale Y axis only
            plot.Axes.AutoScaleY();
            plotInfo.Plot.Refresh();
            
            logger.Debug($"Plot {plotInfo.Id} Y-axis reset to auto-scale");
        }

        private void CreateGlobalControls()
        {
            if (pnlTop == null) return;

            const int buttonSize = 18;
            const int margin = 2;
            const int startX = 30; // Distance from right edge
            
            // Calculate position for the 5x6 grid (taking into account checkbox area)
            int checkboxAreaWidth = 180; // Leave space for checkboxes on the left
            int rightEdge = pnlTop.Width - startX;
            int gridWidth = 5 * buttonSize + 4 * margin;
            int centerX = rightEdge - gridWidth; // Position grid from right edge
            int centerY = margin;

            /*
            Layout design:
                    ResetY           ResetX
                             panUpY
                            ZoomInY
            panLeftX     ZoomInX     ResetButton     ZoomOutX     panRightX
                            ZoomOutY                    
                            panDownY
            */

            // Center column (Y controls)
            int centerColX = centerX + 2 * (buttonSize + margin);
            
            // Row 0: Reset Y and Reset X (top row, symmetrical)
            globalResetYButton = new Button
            {
                Name = "globalResetY",
                Text = "RY",
                Size = new Size(buttonSize, buttonSize),
                Location = new Point(centerX + (buttonSize + margin), centerY),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                BackColor = Color.LightSalmon,
                ForeColor = Color.Black,
                Font = new Font("Arial", 6, FontStyle.Bold),
                TabStop = false
            };

            globalResetXButton = new Button
            {
                Name = "globalResetX",
                Text = "RX",
                Size = new Size(buttonSize, buttonSize),
                Location = new Point(centerX + 3 * (buttonSize + margin), centerY),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                BackColor = Color.LightSalmon,
                ForeColor = Color.Black,
                Font = new Font("Arial", 6, FontStyle.Bold),
                TabStop = false
            };
            
            // Row 1: Y Pan Up
            globalYPanUpButton = new Button
            {
                Name = "globalYPanUp",
                Text = "↑",
                Size = new Size(buttonSize, buttonSize),
                Location = new Point(centerColX, centerY + (buttonSize + margin)),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                BackColor = Color.LightSkyBlue,
                ForeColor = Color.Black,
                Font = new Font("Arial", 7, FontStyle.Bold),
                TabStop = false
            };

            // Row 2: Y Zoom In
            globalYZoomInButton = new Button
            {
                Name = "globalYZoomIn",
                Text = "Y+",
                Size = new Size(buttonSize, buttonSize),
                Location = new Point(centerColX, centerY + 2 * (buttonSize + margin)),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                BackColor = Color.LightBlue,
                ForeColor = Color.Black,
                Font = new Font("Arial", 6, FontStyle.Bold),
                TabStop = false
            };

            // Row 3: Main control line (X Pan Left, X Zoom In, Reset, X Zoom Out, X Pan Right)
            int row3Y = centerY + 3 * (buttonSize + margin);
            
            globalXPanLeftButton = new Button
            {
                Name = "globalXPanLeft",
                Text = "←",
                Size = new Size(buttonSize, buttonSize),
                Location = new Point(centerX, row3Y),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                BackColor = Color.LightGreen,
                ForeColor = Color.Black,
                Font = new Font("Arial", 7, FontStyle.Bold),
                TabStop = false
            };

            globalXZoomInButton = new Button
            {
                Name = "globalXZoomIn",
                Text = "X+",
                Size = new Size(buttonSize, buttonSize),
                Location = new Point(centerX + (buttonSize + margin), row3Y),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                BackColor = Color.LightGreen,
                ForeColor = Color.Black,
                Font = new Font("Arial", 6, FontStyle.Bold),
                TabStop = false
            };

            globalResetButton = new Button
            {
                Name = "globalReset",
                Text = "⟲",
                Size = new Size(buttonSize, buttonSize),
                Location = new Point(centerColX, row3Y),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                BackColor = Color.Orange,
                ForeColor = Color.Black,
                Font = new Font("Arial", 7, FontStyle.Bold),
                TabStop = false
            };

            globalXZoomOutButton = new Button
            {
                Name = "globalXZoomOut",
                Text = "X-",
                Size = new Size(buttonSize, buttonSize),
                Location = new Point(centerX + 3 * (buttonSize + margin), row3Y),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                BackColor = Color.LightYellow,
                ForeColor = Color.Black,
                Font = new Font("Arial", 6, FontStyle.Bold),
                TabStop = false
            };

            globalXPanRightButton = new Button
            {
                Name = "globalXPanRight",
                Text = "→",
                Size = new Size(buttonSize, buttonSize),
                Location = new Point(centerX + 4 * (buttonSize + margin), row3Y),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                BackColor = Color.LightGreen,
                ForeColor = Color.Black,
                Font = new Font("Arial", 7, FontStyle.Bold),
                TabStop = false
            };

            // Row 4: Y Zoom Out (center column)
            globalYZoomOutButton = new Button
            {
                Name = "globalYZoomOut",
                Text = "Y-",
                Size = new Size(buttonSize, buttonSize),
                Location = new Point(centerColX, centerY + 4 * (buttonSize + margin)),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                BackColor = Color.LightCoral,
                ForeColor = Color.Black,
                Font = new Font("Arial", 6, FontStyle.Bold),
                TabStop = false
            };

            // Row 5: Y Pan Down (center column)
            globalYPanDownButton = new Button
            {
                Name = "globalYPanDown",
                Text = "↓",
                Size = new Size(buttonSize, buttonSize),
                Location = new Point(centerColX, centerY + 5 * (buttonSize + margin)),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                BackColor = Color.LightSkyBlue,
                ForeColor = Color.Black,
                Font = new Font("Arial", 7, FontStyle.Bold),
                TabStop = false
            };

            // Add event handlers for global actions
            globalYZoomInButton.Click += (sender, e) => GlobalZoomY(0.8);
            globalYZoomOutButton.Click += (sender, e) => GlobalZoomY(1.25);
            globalXZoomInButton.Click += (sender, e) => GlobalZoomX(0.8);
            globalXZoomOutButton.Click += (sender, e) => GlobalZoomX(1.25);
            
            globalYPanUpButton.Click += (sender, e) => GlobalPanY(0.2);
            globalYPanDownButton.Click += (sender, e) => GlobalPanY(-0.2);
            globalXPanLeftButton.Click += (sender, e) => GlobalPanX(-0.2);
            globalXPanRightButton.Click += (sender, e) => GlobalPanX(0.2);
            
            globalResetButton.Click += (sender, e) => GlobalResetAll();
            globalResetXButton.Click += (sender, e) => GlobalResetX();
            globalResetYButton.Click += (sender, e) => GlobalResetY();

            // Add all global controls to pnlTop
            pnlTop.Controls.Add(globalYPanUpButton);
            pnlTop.Controls.Add(globalYZoomInButton);
            pnlTop.Controls.Add(globalYZoomOutButton);
            pnlTop.Controls.Add(globalYPanDownButton);
            pnlTop.Controls.Add(globalXPanLeftButton);
            pnlTop.Controls.Add(globalXZoomInButton);
            pnlTop.Controls.Add(globalXZoomOutButton);
            pnlTop.Controls.Add(globalXPanRightButton);
            pnlTop.Controls.Add(globalResetYButton);
            pnlTop.Controls.Add(globalResetButton);
            pnlTop.Controls.Add(globalResetXButton);

            // Create sync checkboxes (left side of pnlTop)
            CreateSyncCheckBoxes();

            logger.Information("Global control buttons and sync checkboxes created and added to pnlTop");
        }

        private void CreateSyncCheckBoxes()
        {
            if (pnlTop == null) return;

            const int checkBoxWidth = 75;
            const int checkBoxHeight = 18;
            const int margin = 3;
            int startX = margin;
            int startY = margin;

            // Sync Zoom
            syncZoomCheckBox = new CheckBox
            {
                Name = "syncZoom",
                Text = "Zoom",
                Size = new Size(checkBoxWidth, checkBoxHeight),
                Location = new Point(startX, startY),
                Anchor = AnchorStyles.Top | AnchorStyles.Left,
                Checked = true, // Default enabled
                Font = new Font("Arial", 7, FontStyle.Regular),
                TabStop = false
            };

            // Sync Pan
            syncPanCheckBox = new CheckBox
            {
                Name = "syncPan",
                Text = "Pan",
                Size = new Size(checkBoxWidth, checkBoxHeight),
                Location = new Point(startX + checkBoxWidth + margin, startY),
                Anchor = AnchorStyles.Top | AnchorStyles.Left,
                Checked = true, // Default enabled
                Font = new Font("Arial", 7, FontStyle.Regular),
                TabStop = false
            };

            // Sync Mouse Wheel (second row)
            syncMouseWheelCheckBox = new CheckBox
            {
                Name = "syncMouseWheel",
                Text = "Wheel",
                Size = new Size(checkBoxWidth, checkBoxHeight),
                Location = new Point(startX, startY + checkBoxHeight + margin),
                Anchor = AnchorStyles.Top | AnchorStyles.Left,
                Checked = false, // Default disabled
                Font = new Font("Arial", 7, FontStyle.Regular),
                TabStop = false
            };

            // Sync Reset
            syncResetCheckBox = new CheckBox
            {
                Name = "syncReset",
                Text = "Reset",
                Size = new Size(checkBoxWidth, checkBoxHeight),
                Location = new Point(startX + checkBoxWidth + margin, startY + checkBoxHeight + margin),
                Anchor = AnchorStyles.Top | AnchorStyles.Left,
                Checked = true, // Default enabled
                Font = new Font("Arial", 7, FontStyle.Regular),
                TabStop = false
            };

            // Sync Mouse Drag (third row)
            syncMouseDragCheckBox = new CheckBox
            {
                Name = "syncMouseDrag",
                Text = "Drag",
                Size = new Size(checkBoxWidth, checkBoxHeight),
                Location = new Point(startX, startY + 2 * (checkBoxHeight + margin)),
                Anchor = AnchorStyles.Top | AnchorStyles.Left,
                Checked = false, // Default disabled
                Font = new Font("Arial", 7, FontStyle.Regular),
                TabStop = false
            };

            // Sync Axis Limits
            syncAxisLimitsCheckBox = new CheckBox
            {
                Name = "syncAxisLimits",
                Text = "Limits",
                Size = new Size(checkBoxWidth, checkBoxHeight),
                Location = new Point(startX + checkBoxWidth + margin, startY + 2 * (checkBoxHeight + margin)),
                Anchor = AnchorStyles.Top | AnchorStyles.Left,
                Checked = false, // Default disabled
                Font = new Font("Arial", 7, FontStyle.Regular),
                TabStop = false
            };

            // Enable Mouse Wheel Scrolling (third row)
            enableScrollbarCheckBox = new CheckBox
            {
                Name = "enableMouseWheelScroll",
                Text = "Wheel",
                Size = new Size(checkBoxWidth, checkBoxHeight),
                Location = new Point(startX, startY + 3 * (checkBoxHeight + margin)),
                Anchor = AnchorStyles.Top | AnchorStyles.Left,
                Checked = false, // Default disabled - mouse wheel scrolling off
                Font = new Font("Arial", 7, FontStyle.Regular),
                TabStop = false
            };
            
            // Add event handler for scrollbar checkbox
            enableScrollbarCheckBox.CheckedChanged += (sender, e) => UpdateScrollbarState();

            // Add checkboxes to pnlTop
            pnlTop.Controls.Add(syncZoomCheckBox);
            pnlTop.Controls.Add(syncPanCheckBox);
            pnlTop.Controls.Add(syncMouseWheelCheckBox);
            pnlTop.Controls.Add(syncResetCheckBox);
            pnlTop.Controls.Add(syncMouseDragCheckBox);
            pnlTop.Controls.Add(syncAxisLimitsCheckBox);
            pnlTop.Controls.Add(enableScrollbarCheckBox);

            logger.Information("Sync checkboxes created and added to pnlTop");
            
            // Initialize scrollbar state
            UpdateScrollbarState();
        }

        private void UpdateScrollbarState()
        {
            try
            {
                if (pnlCenter != null)
                {
                    // Always keep AutoScroll enabled for manual scrollbar usage
                    // The checkbox only controls mouse wheel scrolling, not manual scrollbar usage
                    if (mainForm.InvokeRequired)
                    {
                        mainForm.Invoke(() => 
                        {
                            pnlCenter.AutoScroll = true; // Always enabled for manual scrolling
                        });
                    }
                    else
                    {
                        pnlCenter.AutoScroll = true; // Always enabled for manual scrolling
                    }
                    
                    bool enableMouseWheelScroll = enableScrollbarCheckBox?.Checked == true;
                    logger.Debug($"Mouse wheel scrolling: {(enableMouseWheelScroll ? "Enabled" : "Disabled")} (manual scrollbar always enabled)");
                }
            }
            catch (Exception ex)
            {
                logger.Error($"Error updating scrollbar state: {ex.Message}");
            }
        }

        // Global control event handlers (affect all plots)
        private void GlobalZoomX(double factor)
        {
            try
            {
                if (syncZoomCheckBox?.Checked == true)
                {
                    foreach (var plot in plots.Values)
                    {
                        ZoomX(plot, factor);
                    }
                    logger.Debug($"Global X zoom applied to {plots.Count} plots with factor={factor}");
                }
                else
                {
                    logger.Debug("Global X zoom skipped - Sync Zoom is disabled");
                }
            }
            catch (Exception ex)
            {
                logger.Error($"Error during global X zoom: {ex.Message}");
            }
        }

        private void GlobalZoomY(double factor)
        {
            try
            {
                if (syncZoomCheckBox?.Checked == true)
                {
                    foreach (var plot in plots.Values)
                    {
                        ZoomY(plot, factor);
                    }
                    logger.Debug($"Global Y zoom applied to {plots.Count} plots with factor={factor}");
                }
                else
                {
                    logger.Debug("Global Y zoom skipped - Sync Zoom is disabled");
                }
            }
            catch (Exception ex)
            {
                logger.Error($"Error during global Y zoom: {ex.Message}");
            }
        }

        private void GlobalPanX(double factor)
        {
            try
            {
                if (syncPanCheckBox?.Checked == true)
                {
                    foreach (var plot in plots.Values)
                    {
                        PanX(plot, factor);
                    }
                    logger.Debug($"Global X pan applied to {plots.Count} plots with factor={factor}");
                }
                else
                {
                    logger.Debug("Global X pan skipped - Sync Pan is disabled");
                }
            }
            catch (Exception ex)
            {
                logger.Error($"Error during global X pan: {ex.Message}");
            }
        }

        private void GlobalPanY(double factor)
        {
            try
            {
                if (syncPanCheckBox?.Checked == true)
                {
                    foreach (var plot in plots.Values)
                    {
                        PanY(plot, factor);
                    }
                    logger.Debug($"Global Y pan applied to {plots.Count} plots with factor={factor}");
                }
                else
                {
                    logger.Debug("Global Y pan skipped - Sync Pan is disabled");
                }
            }
            catch (Exception ex)
            {
                logger.Error($"Error during global Y pan: {ex.Message}");
            }
        }

        private void GlobalResetAll()
        {
            try
            {
                if (syncResetCheckBox?.Checked == true)
                {
                    foreach (var plot in plots.Values)
                    {
                        ResetPlotView(plot);
                    }
                    logger.Debug($"Global reset applied to {plots.Count} plots");
                }
                else
                {
                    logger.Debug("Global reset skipped - Sync Reset is disabled");
                }
            }
            catch (Exception ex)
            {
                logger.Error($"Error during global reset: {ex.Message}");
            }
        }

        private void GlobalResetX()
        {
            try
            {
                if (syncResetCheckBox?.Checked == true)
                {
                    foreach (var plot in plots.Values)
                    {
                        ResetPlotViewX(plot);
                    }
                    logger.Debug($"Global X reset applied to {plots.Count} plots");
                }
                else
                {
                    logger.Debug("Global X reset skipped - Sync Reset is disabled");
                }
            }
            catch (Exception ex)
            {
                logger.Error($"Error during global X reset: {ex.Message}");
            }
        }

        private void GlobalResetY()
        {
            try
            {
                if (syncResetCheckBox?.Checked == true)
                {
                    foreach (var plot in plots.Values)
                    {
                        ResetPlotViewY(plot);
                    }
                    logger.Debug($"Global Y reset applied to {plots.Count} plots");
                }
                else
                {
                    logger.Debug("Global Y reset skipped - Sync Reset is disabled");
                }
            }
            catch (Exception ex)
            {
                logger.Error($"Error during global Y reset: {ex.Message}");
            }
        }

        private int CalculateNextPlotPosition()
        {
            if (plots.Count == 0)
                return 0;

            // Calculate Y position based on current plot count
            int yPos = 0;
            
            // Always start with main plot height if it exists
            if (plots.ContainsKey(MAIN_PLOT_ID))
            {
                yPos = MAIN_PLOT_HEIGHT + 5; // Start after main plot: 500 + 5 = 505
            }
            
            // Add height for each existing secondary plot
            int secondaryPlotCount = plots.Count - (plots.ContainsKey(MAIN_PLOT_ID) ? 1 : 0);
            yPos += secondaryPlotCount * (SECONDARY_PLOT_HEIGHT + 5); // 300 + 5 for each
            
            logger.Debug($"Calculated position Y={yPos} for plot #{plots.Count + 1} (secondaryCount: {secondaryPlotCount})");
            return yPos;
        }

        private void UpdateParentAutoScrollMinSize(Panel parent)
        {
            if (parent == null) return;

            // With DockStyle.Top, calculate total height simply
            int totalHeight = 0;
            
            // Add height of main plot
            if (plots.ContainsKey(MAIN_PLOT_ID))
            {
                totalHeight += MAIN_PLOT_HEIGHT; // 600px
            }
            
            // Add height of all secondary plots
            var secondaryPlotCount = plots.Count - (plots.ContainsKey(MAIN_PLOT_ID) ? 1 : 0);
            totalHeight += secondaryPlotCount * SECONDARY_PLOT_HEIGHT; // 500px each
            
            // Add some bottom margin
            totalHeight += 20;
            
            // Set AutoScrollMinSize to ensure scrollbar appears
            parent.AutoScrollMinSize = new Size(0, totalHeight);
            
            logger.Debug($"Updated AutoScrollMinSize to height: {totalHeight} for {plots.Count} plots (main: {plots.ContainsKey(MAIN_PLOT_ID)}, secondary: {secondaryPlotCount})");
        }

        public bool DeletePlot(string id)
        {
            if (!plots.TryRemove(id, out var plotInfo))
            {
                logger.Warning($"Plot '{id}' not found for deletion");
                return false;
            }

            try
            {
                if (mainForm.InvokeRequired)
                {
                    mainForm.Invoke(() =>
                    {
                        plotInfo.Container?.Parent?.Controls.Remove(plotInfo.Container);
                        plotInfo.Dispose();
                    });
                }
                else
                {
                    plotInfo.Container?.Parent?.Controls.Remove(plotInfo.Container);
                    plotInfo.Dispose();
                }

                logger.Information($"Plot '{id}' deleted successfully");
                UpdateStatus($"Plot '{id}' deleted");
                return true;
            }
            catch (Exception ex)
            {
                logger.Error($"Failed to delete plot '{id}': {ex.Message}");
                return false;
            }
        }

        public bool ShowPlot(string id)
        {
            if (!plots.TryGetValue(id, out var plotInfo))
            {
                logger.Warning($"Plot '{id}' not found");
                return false;
            }

            SetControlVisibility(plotInfo.Container, true);
            plotInfo.IsVisible = true;
            logger.Information($"Plot '{id}' shown");
            return true;
        }

        public bool HidePlot(string id)
        {
            if (!plots.TryGetValue(id, out var plotInfo))
            {
                logger.Warning($"Plot '{id}' not found");
                return false;
            }

            SetControlVisibility(plotInfo.Container, false);
            plotInfo.IsVisible = false;
            logger.Information($"Plot '{id}' hidden");
            return true;
        }

        public FormsPlot GetPlot(string id)
        {
            return plots.TryGetValue(id, out var plotInfo) ? plotInfo.Plot : null;
        }

        public PlotInfo GetPlotInfo(string id)
        {
            return plots.TryGetValue(id, out var plotInfo) ? plotInfo : null;
        }

        public IEnumerable<PlotInfo> GetAllPlots()
        {
            return plots.Values.ToList();
        }

        public IEnumerable<string> GetPlotIds()
        {
            return plots.Keys.ToList();
        }

        public void IteratePlots(Action<PlotInfo> action)
        {
            foreach (var plot in plots.Values)
            {
                try
                {
                    action?.Invoke(plot);
                }
                catch (Exception ex)
                {
                    logger.Error($"Error in plot iteration for '{plot.Id}': {ex.Message}");
                }
            }
        }

        public void IterateVisiblePlots(Action<PlotInfo> action)
        {
            foreach (var plot in plots.Values.Where(p => p.IsVisible))
            {
                try
                {
                    action?.Invoke(plot);
                }
                catch (Exception ex)
                {
                    logger.Error($"Error in visible plot iteration for '{plot.Id}': {ex.Message}");
                }
            }
        }

        public int GetPlotCount() => plots.Count;
        public int GetVisiblePlotCount() => plots.Values.Count(p => p.IsVisible);

        public void ClearAllPlots()
        {
            foreach (var plotInfo in plots.Values)
            {
                if (plotInfo.Plot != null)
                {
                    if (mainForm.InvokeRequired)
                    {
                        mainForm.Invoke(() => plotInfo.Plot.Plot.Clear());
                    }
                    else
                    {
                        plotInfo.Plot.Plot.Clear();
                    }
                }
            }
            logger.Information("All plot data cleared");
            UpdateStatus("All plot data cleared");
        }

        public void DeleteAllSecondaryPlots()
        {
            var secondaryPlotIds = plots.Keys.Where(id => id != MAIN_PLOT_ID).ToList();
            foreach (var id in secondaryPlotIds)
            {
                DeletePlot(id);
            }
            logger.Information($"Deleted {secondaryPlotIds.Count} secondary plots, keeping main plot");
            UpdateStatus($"Deleted {secondaryPlotIds.Count} secondary plots");
        }

        public void HideAllSecondaryPlots()
        {
            var secondaryPlots = plots.Values.Where(p => p.Id != MAIN_PLOT_ID);
            foreach (var plotInfo in secondaryPlots)
            {
                SetControlVisibility(plotInfo.Container, false);
                plotInfo.IsVisible = false;
            }
            
            // Disable scrolling and make main plot fill the entire pnlCenter
            if (plots.ContainsKey(MAIN_PLOT_ID) && pnlCenter != null)
            {
                var mainPlot = plots[MAIN_PLOT_ID];
                if (mainForm.InvokeRequired)
                {
                    mainForm.Invoke(() => {
                        // Disable AutoScroll
                        pnlCenter.AutoScroll = false;
                        pnlCenter.AutoScrollMinSize = Size.Empty;
                        
                        // Make main plot fill the entire pnlCenter area
                        mainPlot.Container.Dock = DockStyle.Fill;
                    });
                }
                else
                {
                    // Disable AutoScroll
                    pnlCenter.AutoScroll = false;
                    pnlCenter.AutoScrollMinSize = Size.Empty;
                    
                    // Make main plot fill the entire pnlCenter area
                    mainPlot.Container.Dock = DockStyle.Fill;
                }
            }
            
            logger.Information("All secondary plots hidden, main plot expanded to fill, scrolling disabled");
            UpdateStatus("Secondary plots hidden - Full screen mode");
        }

        public void ShowAllSecondaryPlots()
        {
            var secondaryPlots = plots.Values.Where(p => p.Id != MAIN_PLOT_ID);
            foreach (var plotInfo in secondaryPlots)
            {
                SetControlVisibility(plotInfo.Container, true);
                plotInfo.IsVisible = true;
            }
            
            // Re-enable scrolling and convert main plot back to fixed height
            if (plots.ContainsKey(MAIN_PLOT_ID) && plots.Count > 1 && pnlCenter != null)
            {
                var mainPlot = plots[MAIN_PLOT_ID];
                if (mainForm.InvokeRequired)
                {
                    mainForm.Invoke(() => {
                        // Re-enable AutoScroll
                        pnlCenter.AutoScroll = true;
                        
                        // Convert main plot back to fixed height
                        ConvertMainPlotToFixed(mainPlot);
                        
                        // Update AutoScrollMinSize for proper scrolling
                        UpdateParentAutoScrollMinSize(pnlCenter);
                    });
                }
                else
                {
                    // Re-enable AutoScroll
                    pnlCenter.AutoScroll = true;
                    
                    // Convert main plot back to fixed height
                    ConvertMainPlotToFixed(mainPlot);
                    
                    // Update AutoScrollMinSize for proper scrolling
                    UpdateParentAutoScrollMinSize(pnlCenter);
                }
            }
            
            logger.Information("All secondary plots shown, main plot converted to fixed height, scrolling enabled");
            UpdateStatus("Secondary plots shown");
        }

        private void SetControlVisibility(Control control, bool visible)
        {
            if (control != null && mainForm != null)
            {
                if (mainForm.InvokeRequired)
                {
                    mainForm.Invoke(() => control.Visible = visible);
                }
                else
                {
                    control.Visible = visible;
                }
            }
        }

        // Event trigger methods for AlgoTrader
        public void TriggerInit() => InitRequested?.Invoke(this, EventArgs.Empty);
        public void TriggerStart() => StartRequested?.Invoke(this, EventArgs.Empty);
        public void TriggerStop() => StopRequested?.Invoke(this, EventArgs.Empty);
        public void TriggerReset() => ResetRequested?.Invoke(this, EventArgs.Empty);
        public void TriggerTerminate() => TerminateRequested?.Invoke(this, EventArgs.Empty);

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed && disposing)
            {
                logger.Information("Disposing GuiManager resources");
                
                // Clear all plots
                ClearAllPlots();
                
                // Dispose global control buttons
                globalYPanUpButton?.Dispose();
                globalYZoomInButton?.Dispose();
                globalYZoomOutButton?.Dispose();
                globalYPanDownButton?.Dispose();
                globalXPanLeftButton?.Dispose();
                globalXZoomInButton?.Dispose();
                globalXZoomOutButton?.Dispose();
                globalXPanRightButton?.Dispose();
                globalResetYButton?.Dispose();
                globalResetButton?.Dispose();
                globalResetXButton?.Dispose();
                
                // Dispose sync checkboxes
                syncZoomCheckBox?.Dispose();
                syncPanCheckBox?.Dispose();
                syncMouseWheelCheckBox?.Dispose();
                syncResetCheckBox?.Dispose();
                syncMouseDragCheckBox?.Dispose();
                syncAxisLimitsCheckBox?.Dispose();
                enableScrollbarCheckBox?.Dispose();
                
                // Clear event subscriptions
                InitRequested = null;
                StartRequested = null;
                StopRequested = null;
                ResetRequested = null;
                TerminateRequested = null;

                disposed = true;
                logger.Information("GuiManager disposed successfully");
            }
        }

        ~GuiManager()
        {
            Dispose(false);
        }
    }
}