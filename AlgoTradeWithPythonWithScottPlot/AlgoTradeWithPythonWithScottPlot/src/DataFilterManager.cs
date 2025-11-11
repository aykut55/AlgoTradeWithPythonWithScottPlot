using System;
using Serilog;

namespace AlgoTradeWithPythonWithScottPlot
{
    /// <summary>
    /// Veri filtreleme ve görselleştirme yönetimi
    /// Farklı filtreleme stratejileri ile veri alt kümeleri oluşturur
    /// </summary>
    public static class DataFilterManager
    {
        private static readonly ILogger logger = Log.ForContext(typeof(DataFilterManager));

        /// <summary>
        /// Filtreleme modu
        /// </summary>
        public enum FilterMode
        {
            All,                // Tüm veriyi çizdir
            FitToScreen,        // Ekran çözünürlüğüne göre max sayıda data (scrollbar çıkmasın)
            LastN,              // Son N sayıda data
            FirstN,             // İlk N sayıda data
            Range,              // Belirli aralıktaki data (start index - end index)
            DateRange,          // Belirli tarihler arası (gelecekte - DateTime destekli)
            DateBefore,         // Belirli tarih öncesi (gelecekte - DateTime destekli)
            DateAfter           // Belirli tarih sonrası (gelecekte - DateTime destekli)
        }

        /// <summary>
        /// Filtrelenmiş veri sonucu
        /// </summary>
        public class FilterResult
        {
            public double[] X { get; set; } = Array.Empty<double>();
            public double[] Y { get; set; } = Array.Empty<double>();
            public int OriginalCount { get; set; }
            public int FilteredCount { get; set; }
            public FilterMode Mode { get; set; }
            public string Description { get; set; } = string.Empty;

            /// <summary>
            /// Görünüm aralığı (X ekseni değerleri bazında)
            /// null ise tüm veriyi göster
            /// Plota TÜM veri yüklenir ama ekranda sadece bu aralık görünür
            /// </summary>
            public (double XMin, double XMax)? ViewRange { get; set; }
        }

        /// <summary>
        /// Tüm datayı döndür (filtreleme yok)
        /// </summary>
        public static FilterResult GetAllData(double[] x, double[] y)
        {
            logger.Information($"GetAllData: Returning all {x.Length:N0} data points");

            return new FilterResult
            {
                X = x,
                Y = y,
                OriginalCount = x.Length,
                FilteredCount = x.Length,
                Mode = FilterMode.All,
                Description = $"All data ({x.Length:N0} points)"
            };
        }

        /// <summary>
        /// Ekran çözünürlüğüne/plot genişliğine göre uygun ViewRange döndür
        /// TÜM veriyi yükler ama ekranda scroll olmayacak kadar kısım görünür
        /// Adaptive Plot System veri miktarına göre otomatik optimizasyon yapar
        /// </summary>
        /// <param name="x">X verisi</param>
        /// <param name="y">Y verisi</param>
        /// <param name="plotWidthPixels">Plot genişliği (pixel). Default 1000.</param>
        /// <param name="pointsPerPixel">Her pixel için kaç nokta (1-2 arası önerilir). Default 2.</param>
        public static FilterResult GetFitToScreenData(double[] x, double[] y, int plotWidthPixels = 1000, int pointsPerPixel = 2)
        {
            if (x.Length == 0)
            {
                return new FilterResult
                {
                    X = Array.Empty<double>(),
                    Y = Array.Empty<double>(),
                    OriginalCount = 0,
                    FilteredCount = 0,
                    Mode = FilterMode.FitToScreen,
                    Description = "No data",
                    ViewRange = null
                };
            }

            int maxPoints = plotWidthPixels * pointsPerPixel;

            // Eğer veri zaten yeterince küçükse, tümünü göster
            if (x.Length <= maxPoints)
            {
                logger.Information($"GetFitToScreenData: Data size ({x.Length:N0}) already fits screen (max {maxPoints:N0})");
                return GetAllData(x, y);
            }

            // TÜM veriyi yükle ama ekranda sadece son maxPoints kadar görünsün
            // Son kısmı gösteriyoruz çünkü genellikle en güncel veri ilgi çekicidir
            int viewStartIndex = x.Length - maxPoints;
            double xMin = x[viewStartIndex];
            double xMax = x[x.Length - 1];

            logger.Information($"GetFitToScreenData: Returning ALL {x.Length:N0} points, but viewing last {maxPoints:N0} (X range: {xMin} - {xMax})");

            return new FilterResult
            {
                X = x,  // TÜM veriyi döndür
                Y = y,  // TÜM veriyi döndür
                OriginalCount = x.Length,
                FilteredCount = x.Length,  // Tüm veri yüklendiği için
                Mode = FilterMode.FitToScreen,
                Description = $"Fit to screen (viewing last {maxPoints:N0} of {x.Length:N0} points)",
                ViewRange = (xMin, xMax)  // Ekrana sığacak kadar göster
            };
        }

        /// <summary>
        /// Son N sayıda data döndür
        /// TÜM veriyi döndürür ama ekranda sadece son N tanesi görünür (ViewRange ile)
        /// Pan yaparak geri kalan verilere de erişilebilir
        /// </summary>
        public static FilterResult GetLastNData(double[] x, double[] y, int n)
        {
            if (x.Length == 0 || n <= 0)
            {
                return new FilterResult
                {
                    X = Array.Empty<double>(),
                    Y = Array.Empty<double>(),
                    OriginalCount = x.Length,
                    FilteredCount = 0,
                    Mode = FilterMode.LastN,
                    Description = "No data",
                    ViewRange = null
                };
            }

            int count = Math.Min(n, x.Length);
            int startIndex = x.Length - count;

            // ViewRange'i son N veri için ayarla
            double xMin = x[startIndex];
            double xMax = x[x.Length - 1];

            logger.Information($"GetLastNData: Returning ALL {x.Length:N0} points, but viewing last {count:N0} (X range: {xMin} - {xMax})");

            return new FilterResult
            {
                X = x,  // TÜM veriyi döndür
                Y = y,  // TÜM veriyi döndür
                OriginalCount = x.Length,
                FilteredCount = x.Length,  // Tüm veri yüklendiği için
                Mode = FilterMode.LastN,
                Description = $"Last {count:N0} points (of {x.Length:N0} total)",
                ViewRange = (xMin, xMax)  // Sadece son N'i göster
            };
        }

        /// <summary>
        /// İlk N sayıda data döndür
        /// TÜM veriyi döndürür ama ekranda sadece ilk N tanesi görünür (ViewRange ile)
        /// Pan yaparak geri kalan verilere de erişilebilir
        /// </summary>
        public static FilterResult GetFirstNData(double[] x, double[] y, int n)
        {
            if (x.Length == 0 || n <= 0)
            {
                return new FilterResult
                {
                    X = Array.Empty<double>(),
                    Y = Array.Empty<double>(),
                    OriginalCount = x.Length,
                    FilteredCount = 0,
                    Mode = FilterMode.FirstN,
                    Description = "No data",
                    ViewRange = null
                };
            }

            int count = Math.Min(n, x.Length);

            // ViewRange'i ilk N veri için ayarla
            double xMin = x[0];
            double xMax = x[count - 1];

            logger.Information($"GetFirstNData: Returning ALL {x.Length:N0} points, but viewing first {count:N0} (X range: {xMin} - {xMax})");

            return new FilterResult
            {
                X = x,  // TÜM veriyi döndür
                Y = y,  // TÜM veriyi döndür
                OriginalCount = x.Length,
                FilteredCount = x.Length,  // Tüm veri yüklendiği için
                Mode = FilterMode.FirstN,
                Description = $"First {count:N0} points (of {x.Length:N0} total)",
                ViewRange = (xMin, xMax)  // Sadece ilk N'i göster
            };
        }

        /// <summary>
        /// Belirli bir aralıktaki datayı döndür (start index - end index)
        /// TÜM veriyi döndürür ama ekranda sadece belirtilen aralık görünür (ViewRange ile)
        /// Pan yaparak diğer aralıklara da erişilebilir
        /// Örnek: GetRangeData(x, y, 100, 200) -> 100. ile 200. datalar arası görünür
        /// </summary>
        /// <param name="startIndex">Başlangıç index (dahil)</param>
        /// <param name="endIndex">Bitiş index (dahil)</param>
        public static FilterResult GetRangeData(double[] x, double[] y, int startIndex, int endIndex)
        {
            if (x.Length == 0 || startIndex < 0 || endIndex < startIndex)
            {
                return new FilterResult
                {
                    X = Array.Empty<double>(),
                    Y = Array.Empty<double>(),
                    OriginalCount = x.Length,
                    FilteredCount = 0,
                    Mode = FilterMode.Range,
                    Description = "Invalid range",
                    ViewRange = null
                };
            }

            // Ensure indices are within bounds
            startIndex = Math.Max(0, startIndex);
            endIndex = Math.Min(x.Length - 1, endIndex);

            int count = endIndex - startIndex + 1;

            // ViewRange'i belirtilen aralık için ayarla
            double xMin = x[startIndex];
            double xMax = x[endIndex];

            logger.Information($"GetRangeData: Returning ALL {x.Length:N0} points, but viewing range [{startIndex}..{endIndex}] = {count:N0} points (X range: {xMin} - {xMax})");

            return new FilterResult
            {
                X = x,  // TÜM veriyi döndür
                Y = y,  // TÜM veriyi döndür
                OriginalCount = x.Length,
                FilteredCount = x.Length,  // Tüm veri yüklendiği için
                Mode = FilterMode.Range,
                Description = $"Range [{startIndex}..{endIndex}] ({count:N0} points of {x.Length:N0} total)",
                ViewRange = (xMin, xMax)  // Sadece belirtilen aralığı göster
            };
        }

        #region DateTime Filtering (Future Implementation)

        /// <summary>
        /// Belirli tarihler arası datayı döndür (gelecekte implement edilecek)
        /// X ekseninde DateTime değerleri olduğunda kullanılacak
        /// </summary>
        public static FilterResult GetDateRangeData(double[] x, double[] y, DateTime startDate, DateTime endDate)
        {
            // TODO: X eksenini DateTime'a çevir ve filtrele
            logger.Warning("GetDateRangeData: Not implemented yet - requires DateTime X-axis");
            return GetAllData(x, y);
        }

        /// <summary>
        /// Belirli tarih öncesi datayı döndür (gelecekte implement edilecek)
        /// X ekseninde DateTime değerleri olduğunda kullanılacak
        /// </summary>
        public static FilterResult GetDateBeforeData(double[] x, double[] y, DateTime beforeDate)
        {
            // TODO: X eksenini DateTime'a çevir ve filtrele
            logger.Warning("GetDateBeforeData: Not implemented yet - requires DateTime X-axis");
            return GetAllData(x, y);
        }

        /// <summary>
        /// Belirli tarih sonrası datayı döndür (gelecekte implement edilecek)
        /// X ekseninde DateTime değerleri olduğunda kullanılacak
        /// </summary>
        public static FilterResult GetDateAfterData(double[] x, double[] y, DateTime afterDate)
        {
            // TODO: X eksenini DateTime'a çevir ve filtrele
            logger.Warning("GetDateAfterData: Not implemented yet - requires DateTime X-axis");
            return GetAllData(x, y);
        }

        #endregion

        /// <summary>
        /// Ekran çözünürlüğünü tahmin et (plot control'ün genişliği bilinmiyorsa)
        /// </summary>
        public static int EstimatePlotWidth()
        {
            // Ortalama bir plot genişliği tahmin et
            // Tam ekran 1920x1080'de plot ~1400-1600 pixel olabilir
            // Güvenli bir default 1000 pixel
            return 1000;
        }
    }
}
