using System.Runtime.InteropServices;

namespace AlgoTradeWithPythonWithScottPlot
{
    internal static class Program
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool AllocConsole();
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // Console window'u göster (debug için)
            //AllocConsole();

            // Log ayarlarını yapılandır
            LoggerConfig.SetLogDirectory(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs"));
            LoggerConfig.SetLogFileName("AlgoTrader.log");
            
            LoggerConfig.Initialize();
            
            try
            {
                var logger = Serilog.Log.ForContext("SourceContext", "Program");
                logger.Information("=== AlgoTradeWithPythonWithScottPlot Application Starting ===");
                logger.Information("Application initialized, creating main form");
                
                // To customize application configuration such as set high DPI settings or default font,
                // see https://aka.ms/applicationconfiguration.
                ApplicationConfiguration.Initialize();
                
                logger.Information("Running main application form");
                Application.Run(new Form1());
                
                logger.Information("=== AlgoTradeWithPythonWithScottPlot Application Closing ===");
            }
            finally
            {
                LoggerConfig.Shutdown();
            }
        }
    }
}