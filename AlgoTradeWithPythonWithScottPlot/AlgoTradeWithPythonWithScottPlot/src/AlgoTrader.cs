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
        private bool disposed = false;
        private readonly ILogger logger = Log.ForContext<AlgoTrader>();

        public AlgoTrader()
        {
            dataManager = new DataManager();
            logger.Information("AlgoTrader instance created");
        }

        public DataManager GetDataManager()
        {
            return dataManager;
        }

        public void Reset()
        {
            logger.Information("Reset method called");
        }

        public void Init()
        {
            logger.Information("Init method called - Initializing AlgoTrader");
        }

        public void Start()
        {
            logger.Information("Start method called - Starting AlgoTrader");
        }

        public void Stop()
        {
            logger.Information("Stop method called - Stopping AlgoTrader");
        }

        public void Terminate()
        {
            logger.Information("Terminate method called - Terminating AlgoTrader");
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