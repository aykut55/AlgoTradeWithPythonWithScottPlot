using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Serilog;

namespace AlgoTradeWithPythonWithScottPlot
{
    /// <summary>
    /// Demo finansal veri üreteci
    /// OHLC, Volume, MA, RSI ve constant level verileri üretir
    /// </summary>
    public class DemoDataGenerator
    {
        private readonly ILogger logger = Log.ForContext<DemoDataGenerator>();
        private readonly Random random = new Random(42); // Fixed seed for reproducibility

        public int DataPointCount { get; set; } = 1000;
        public double InitialPrice { get; set; } = 100.0;
        public string OutputDirectory { get; set; } = "D:/DemoData";

        public DemoDataGenerator()
        {
        }

        /// <summary>
        /// Tüm demo dataları üretir ve dosyalara yazar
        /// </summary>
        public void GenerateAllData()
        {
            logger.Information($"Generating {DataPointCount} data points to {OutputDirectory}");

            // Output directory oluştur
            Directory.CreateDirectory(OutputDirectory);

            // 1. OHLC data üret
            var ohlcData = GenerateOHLCData();
            WriteToCsv(Path.Combine(OutputDirectory, "ohlc.csv"), ohlcData,
                       new[] { "Timestamp", "Open", "High", "Low", "Close" });

            // Close fiyatlarını extract et (MA ve RSI için)
            var closePrices = ohlcData.Select(row => double.Parse(row[4])).ToArray();

            // 2. Volume data üret
            var volumeData = GenerateVolumeData();
            WriteToCsv(Path.Combine(OutputDirectory, "volume.csv"), volumeData,
                       new[] { "Timestamp", "Volume" });

            // 3. MA verileri üret
            var ma5 = CalculateMA(closePrices, 5);
            WriteToCsv(Path.Combine(OutputDirectory, "ma5.csv"),
                       ConvertToTimestampValue(ma5), new[] { "Timestamp", "Value" });

            var ma8 = CalculateMA(closePrices, 8);
            WriteToCsv(Path.Combine(OutputDirectory, "ma8.csv"),
                       ConvertToTimestampValue(ma8), new[] { "Timestamp", "Value" });

            var ma13 = CalculateMA(closePrices, 13);
            WriteToCsv(Path.Combine(OutputDirectory, "ma13.csv"),
                       ConvertToTimestampValue(ma13), new[] { "Timestamp", "Value" });

            var ma50 = CalculateMA(closePrices, 50);
            WriteToCsv(Path.Combine(OutputDirectory, "ma50.csv"),
                       ConvertToTimestampValue(ma50), new[] { "Timestamp", "Value" });

            var ma100 = CalculateMA(closePrices, 100);
            WriteToCsv(Path.Combine(OutputDirectory, "ma100.csv"),
                       ConvertToTimestampValue(ma100), new[] { "Timestamp", "Value" });

            var ma200 = CalculateMA(closePrices, 200);
            WriteToCsv(Path.Combine(OutputDirectory, "ma200.csv"),
                       ConvertToTimestampValue(ma200), new[] { "Timestamp", "Value" });

            // 4. RSI data üret
            var rsi = CalculateRSI(closePrices, 14);
            WriteToCsv(Path.Combine(OutputDirectory, "rsi.csv"),
                       ConvertToTimestampValue(rsi), new[] { "Timestamp", "Value" });

            // 5. Constant level verileri (RSI için 30 ve 70 seviyeleri)
            var level30 = GenerateConstantLevel(30.0);
            WriteToCsv(Path.Combine(OutputDirectory, "rsi_level_30.csv"),
                       ConvertToTimestampValue(level30), new[] { "Timestamp", "Value" });

            var level70 = GenerateConstantLevel(70.0);
            WriteToCsv(Path.Combine(OutputDirectory, "rsi_level_70.csv"),
                       ConvertToTimestampValue(level70), new[] { "Timestamp", "Value" });

            logger.Information("All demo data generated successfully");
        }

        /// <summary>
        /// JSON config dosyası oluşturur
        /// </summary>
        public void GenerateJsonConfig()
        {
            string jsonContent = @"{
  ""plots"": [
    {
      ""plotId"": ""Plot_OHLC"",
      ""plotName"": ""Main Chart - OHLC + MAs"",
      ""height"": 400,
      ""data"": [
        {
          ""dataId"": 0,
          ""type"": ""OHLC"",
          ""name"": ""BTC/USD"",
          ""source"": """ + OutputDirectory.Replace("\\", "/") + @"/ohlc.csv"",
          ""color"": null
        },
        {
          ""dataId"": 1,
          ""type"": ""Line"",
          ""name"": ""MA5"",
          ""source"": """ + OutputDirectory.Replace("\\", "/") + @"/ma5.csv"",
          ""color"": ""#FF6347""
        },
        {
          ""dataId"": 2,
          ""type"": ""Line"",
          ""name"": ""MA8"",
          ""source"": """ + OutputDirectory.Replace("\\", "/") + @"/ma8.csv"",
          ""color"": ""#FFD700""
        },
        {
          ""dataId"": 3,
          ""type"": ""Line"",
          ""name"": ""MA13"",
          ""source"": """ + OutputDirectory.Replace("\\", "/") + @"/ma13.csv"",
          ""color"": ""#32CD32""
        },
        {
          ""dataId"": 4,
          ""type"": ""Line"",
          ""name"": ""MA50"",
          ""source"": """ + OutputDirectory.Replace("\\", "/") + @"/ma50.csv"",
          ""color"": ""#FF0000""
        },
        {
          ""dataId"": 5,
          ""type"": ""Line"",
          ""name"": ""MA100"",
          ""source"": """ + OutputDirectory.Replace("\\", "/") + @"/ma100.csv"",
          ""color"": ""#00FF00""
        },
        {
          ""dataId"": 6,
          ""type"": ""Line"",
          ""name"": ""MA200"",
          ""source"": """ + OutputDirectory.Replace("\\", "/") + @"/ma200.csv"",
          ""color"": ""#0000FF""
        }
      ]
    },
    {
      ""plotId"": ""Plot_Volume"",
      ""plotName"": ""Volume"",
      ""height"": 150,
      ""data"": [
        {
          ""dataId"": 0,
          ""type"": ""Volume"",
          ""name"": ""Volume"",
          ""source"": """ + OutputDirectory.Replace("\\", "/") + @"/volume.csv"",
          ""color"": ""#4169E1""
        }
      ]
    },
    {
      ""plotId"": ""Plot_RSI"",
      ""plotName"": ""RSI (14)"",
      ""height"": 150,
      ""data"": [
        {
          ""dataId"": 0,
          ""type"": ""Line"",
          ""name"": ""RSI"",
          ""source"": """ + OutputDirectory.Replace("\\", "/") + @"/rsi.csv"",
          ""color"": ""#9370DB""
        },
        {
          ""dataId"": 1,
          ""type"": ""Line"",
          ""name"": ""Level 30"",
          ""source"": """ + OutputDirectory.Replace("\\", "/") + @"/rsi_level_30.csv"",
          ""color"": ""#808080""
        },
        {
          ""dataId"": 2,
          ""type"": ""Line"",
          ""name"": ""Level 70"",
          ""source"": """ + OutputDirectory.Replace("\\", "/") + @"/rsi_level_70.csv"",
          ""color"": ""#808080""
        }
      ]
    }
  ]
}";

            string configPath = Path.Combine(OutputDirectory, "demo_config.json");
            File.WriteAllText(configPath, jsonContent);
            logger.Information($"JSON config written to: {configPath}");
        }

        private List<string[]> GenerateOHLCData()
        {
            var data = new List<string[]>();
            double price = InitialPrice;
            DateTime startDate = DateTime.Now.AddDays(-DataPointCount);

            for (int i = 0; i < DataPointCount; i++)
            {
                DateTime timestamp = startDate.AddDays(i);

                // Random walk with trend
                double change = (random.NextDouble() - 0.48) * 2.0; // Slight upward bias
                double open = price;
                double close = price + change;

                // High and Low based on volatility
                double volatility = Math.Abs(change) * 1.5;
                double high = Math.Max(open, close) + random.NextDouble() * volatility;
                double low = Math.Min(open, close) - random.NextDouble() * volatility;

                // Ensure low < open,close < high
                low = Math.Min(low, Math.Min(open, close));
                high = Math.Max(high, Math.Max(open, close));

                data.Add(new[]
                {
                    timestamp.ToString("yyyy-MM-dd HH:mm:ss"),
                    open.ToString("F2"),
                    high.ToString("F2"),
                    low.ToString("F2"),
                    close.ToString("F2")
                });

                price = close;
            }

            return data;
        }

        private List<string[]> GenerateVolumeData()
        {
            var data = new List<string[]>();
            DateTime startDate = DateTime.Now.AddDays(-DataPointCount);

            for (int i = 0; i < DataPointCount; i++)
            {
                DateTime timestamp = startDate.AddDays(i);
                double volume = 1000000 + random.NextDouble() * 2000000; // 1M to 3M

                data.Add(new[]
                {
                    timestamp.ToString("yyyy-MM-dd HH:mm:ss"),
                    volume.ToString("F0")
                });
            }

            return data;
        }

        private double[] CalculateMA(double[] prices, int period)
        {
            double[] ma = new double[prices.Length];

            for (int i = 0; i < prices.Length; i++)
            {
                if (i < period - 1)
                {
                    // İlk period kadar veri için NaN veya mevcut değeri kullan
                    ma[i] = prices[i];
                }
                else
                {
                    double sum = 0;
                    for (int j = 0; j < period; j++)
                    {
                        sum += prices[i - j];
                    }
                    ma[i] = sum / period;
                }
            }

            return ma;
        }

        private double[] CalculateRSI(double[] prices, int period)
        {
            double[] rsi = new double[prices.Length];

            if (prices.Length < period + 1)
                return rsi;

            // İlk period için average gain ve loss hesapla
            double avgGain = 0;
            double avgLoss = 0;

            for (int i = 1; i <= period; i++)
            {
                double change = prices[i] - prices[i - 1];
                if (change > 0)
                    avgGain += change;
                else
                    avgLoss += Math.Abs(change);
            }

            avgGain /= period;
            avgLoss /= period;

            // İlk RSI değeri
            if (avgLoss == 0)
                rsi[period] = 100;
            else
            {
                double rs = avgGain / avgLoss;
                rsi[period] = 100 - (100 / (1 + rs));
            }

            // Sonraki RSI değerleri (smoothed)
            for (int i = period + 1; i < prices.Length; i++)
            {
                double change = prices[i] - prices[i - 1];
                double gain = change > 0 ? change : 0;
                double loss = change < 0 ? Math.Abs(change) : 0;

                avgGain = (avgGain * (period - 1) + gain) / period;
                avgLoss = (avgLoss * (period - 1) + loss) / period;

                if (avgLoss == 0)
                    rsi[i] = 100;
                else
                {
                    double rs = avgGain / avgLoss;
                    rsi[i] = 100 - (100 / (1 + rs));
                }
            }

            // İlk period için değerleri 50 olarak set et (neutral)
            for (int i = 0; i < period; i++)
            {
                rsi[i] = 50;
            }

            return rsi;
        }

        private double[] GenerateConstantLevel(double level)
        {
            double[] data = new double[DataPointCount];
            for (int i = 0; i < DataPointCount; i++)
            {
                data[i] = level;
            }
            return data;
        }

        private List<string[]> ConvertToTimestampValue(double[] values)
        {
            var data = new List<string[]>();
            DateTime startDate = DateTime.Now.AddDays(-DataPointCount);

            for (int i = 0; i < values.Length; i++)
            {
                DateTime timestamp = startDate.AddDays(i);
                data.Add(new[]
                {
                    timestamp.ToString("yyyy-MM-dd HH:mm:ss"),
                    values[i].ToString("F2")
                });
            }

            return data;
        }

        private void WriteToCsv(string filePath, List<string[]> data, string[] headers)
        {
            using (var writer = new StreamWriter(filePath))
            {
                // Write header
                writer.WriteLine(string.Join(",", headers));

                // Write data
                foreach (var row in data)
                {
                    writer.WriteLine(string.Join(",", row));
                }
            }

            logger.Debug($"Written {data.Count} rows to {filePath}");
        }
    }
}
