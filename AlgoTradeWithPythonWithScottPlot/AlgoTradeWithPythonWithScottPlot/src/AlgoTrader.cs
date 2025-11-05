using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Serilog;

namespace AlgoTradeWithPythonWithScottPlot
{
    public class AlgoTrader : IDisposable
    {
        private DataManager dataManager;
        private GuiManager guiManager;
        private bool disposed = false;
        private readonly ILogger logger = Log.ForContext<AlgoTrader>();

        public AlgoTrader()
        {
            dataManager = new DataManager();
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

        public DataManager GetDataManager()
        {
            return dataManager;
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
                    
                    dataManager?.Dispose();
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