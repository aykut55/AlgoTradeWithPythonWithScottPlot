using System;
using System.Drawing;
using System.Windows.Forms;
using ScottPlot.WinForms;

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