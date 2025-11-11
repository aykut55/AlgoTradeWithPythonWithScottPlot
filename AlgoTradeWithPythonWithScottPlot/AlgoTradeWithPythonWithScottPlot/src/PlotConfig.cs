using System.Collections.Generic;

namespace AlgoTradeWithPythonWithScottPlot
{
    /// <summary>
    /// JSON configuration dosyası için root model
    /// </summary>
    public class PlotConfiguration
    {
        public List<PlotDefinition> Plots { get; set; } = new List<PlotDefinition>();
    }

    /// <summary>
    /// Tek bir plot tanımı
    /// </summary>
    public class PlotDefinition
    {
        public string PlotId { get; set; } = string.Empty;
        public string PlotName { get; set; } = string.Empty;
        public int Height { get; set; } = 300;
        public List<DataDefinition> Data { get; set; } = new List<DataDefinition>();
    }

    /// <summary>
    /// Plot içindeki bir data serisi tanımı
    /// </summary>
    public class DataDefinition
    {
        public int DataId { get; set; }
        public string Type { get; set; } = string.Empty; // "OHLC", "Volume", "Line", "Histogram"
        public string Name { get; set; } = string.Empty;
        public string Source { get; set; } = string.Empty; // Dosya yolu
        public string? Color { get; set; } // Hex renk (#FF0000) veya null
    }

    /// <summary>
    /// Data türleri için enum
    /// </summary>
    public enum DataType
    {
        OHLC,
        Volume,
        Line,
        Histogram
    }
}
