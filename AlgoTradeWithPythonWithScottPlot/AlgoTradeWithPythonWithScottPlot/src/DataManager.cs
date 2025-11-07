using System;

namespace AlgoTradeWithPythonWithScottPlot
{
    /// <summary>
    /// Merkezi data generation manager
    /// Farklı tipte test verileri üretir
    /// </summary>
    public static class DataManager
    {
        /// <summary>
        /// Index'e göre farklı tipte veriler üretir
        /// </summary>
        /// <param name="idx">Data tipi index:
        /// 0 = Complex wave with noise (Form1'deki default)
        /// 1 = Simple sine wave (LoadSineWaveData gibi)
        /// 2 = Cosine wave
        /// 3 = Square wave
        /// 4 = Sawtooth wave
        /// 5 = Random walk
        /// 6 = Multiple harmonics
        /// 7 = Exponential growth
        /// 8 = Logarithmic curve
        /// 9 = Noisy step function
        /// </param>
        /// <param name="points">Kaç adet point üretilecek</param>
        /// <param name="amplitude">Genlik (amplitude)</param>
        /// <param name="frequency">Frekans</param>
        /// <returns>Tuple of (x array, y array)</returns>
        public static (double[] x, double[] y) GenerateData(int idx, int points = 1000, double amplitude = 1.0, double frequency = 1.0)
        {
            double[] x = new double[points];
            double[] y = new double[points];

            switch (idx)
            {
                case 0: // Complex wave with noise (Form1'deki default)
                    GenerateComplexWaveWithNoise(x, y, points);
                    break;

                case 1: // Simple sine wave (LoadSineWaveData gibi)
                    GenerateSimpleSineWave(x, y, points, amplitude, frequency);
                    break;

                case 2: // Cosine wave
                    GenerateCosineWave(x, y, points, amplitude, frequency);
                    break;

                case 3: // Square wave
                    GenerateSquareWave(x, y, points, amplitude, frequency);
                    break;

                case 4: // Sawtooth wave
                    GenerateSawtoothWave(x, y, points, amplitude, frequency);
                    break;

                case 5: // Random walk
                    GenerateRandomWalk(x, y, points, amplitude);
                    break;

                case 6: // Multiple harmonics
                    GenerateMultipleHarmonics(x, y, points, amplitude, frequency);
                    break;

                case 7: // Exponential growth
                    GenerateExponentialGrowth(x, y, points, amplitude);
                    break;

                case 8: // Logarithmic curve
                    GenerateLogarithmicCurve(x, y, points, amplitude);
                    break;

                case 9: // Noisy step function
                    GenerateNoisyStepFunction(x, y, points, amplitude);
                    break;

                default:
                    // Default: Complex wave with noise
                    GenerateComplexWaveWithNoise(x, y, points);
                    break;
            }

            return (x, y);
        }

        #region Data Generation Methods

        /// <summary>
        /// idx = 0: Complex wave with noise (Form1'deki mevcut kod)
        /// </summary>
        private static void GenerateComplexWaveWithNoise(double[] x, double[] y, int points)
        {
            var random = new Random();
            for (int i = 0; i < points; i++)
            {
                x[i] = i * 0.05; // Wider X range (0 to 50)
                y[i] = Math.Sin(x[i] * 0.5) + 0.3 * Math.Sin(x[i] * 3) + 0.1 * random.NextDouble(); // Complex wave with noise
            }
        }

        /// <summary>
        /// idx = 1: Simple sine wave (GuiManager.LoadSineWaveData gibi)
        /// </summary>
        private static void GenerateSimpleSineWave(double[] x, double[] y, int points, double amplitude, double frequency)
        {
            for (int i = 0; i < points; i++)
            {
                x[i] = i * 0.01; // X spacing
                y[i] = amplitude * Math.Sin(2 * Math.PI * frequency * x[i]);
            }
        }

        /// <summary>
        /// idx = 2: Cosine wave
        /// </summary>
        private static void GenerateCosineWave(double[] x, double[] y, int points, double amplitude, double frequency)
        {
            for (int i = 0; i < points; i++)
            {
                x[i] = i * 0.01;
                y[i] = amplitude * Math.Cos(2 * Math.PI * frequency * x[i]);
            }
        }

        /// <summary>
        /// idx = 3: Square wave
        /// </summary>
        private static void GenerateSquareWave(double[] x, double[] y, int points, double amplitude, double frequency)
        {
            for (int i = 0; i < points; i++)
            {
                x[i] = i * 0.01;
                double sinValue = Math.Sin(2 * Math.PI * frequency * x[i]);
                y[i] = amplitude * Math.Sign(sinValue);
            }
        }

        /// <summary>
        /// idx = 4: Sawtooth wave
        /// </summary>
        private static void GenerateSawtoothWave(double[] x, double[] y, int points, double amplitude, double frequency)
        {
            for (int i = 0; i < points; i++)
            {
                x[i] = i * 0.01;
                double t = (frequency * x[i]) % 1.0;
                y[i] = amplitude * (2.0 * t - 1.0);
            }
        }

        /// <summary>
        /// idx = 5: Random walk
        /// </summary>
        private static void GenerateRandomWalk(double[] x, double[] y, int points, double amplitude)
        {
            var random = new Random();
            double currentValue = 0;

            for (int i = 0; i < points; i++)
            {
                x[i] = i * 0.01;
                currentValue += (random.NextDouble() - 0.5) * amplitude * 0.1;
                y[i] = currentValue;
            }
        }

        /// <summary>
        /// idx = 6: Multiple harmonics (birden fazla sinüs dalgasının toplamı)
        /// </summary>
        private static void GenerateMultipleHarmonics(double[] x, double[] y, int points, double amplitude, double frequency)
        {
            for (int i = 0; i < points; i++)
            {
                x[i] = i * 0.01;
                y[i] = amplitude * (
                    Math.Sin(2 * Math.PI * frequency * x[i]) +
                    0.5 * Math.Sin(2 * Math.PI * frequency * 2 * x[i]) +
                    0.25 * Math.Sin(2 * Math.PI * frequency * 3 * x[i])
                );
            }
        }

        /// <summary>
        /// idx = 7: Exponential growth
        /// </summary>
        private static void GenerateExponentialGrowth(double[] x, double[] y, int points, double amplitude)
        {
            for (int i = 0; i < points; i++)
            {
                x[i] = i * 0.01;
                y[i] = amplitude * Math.Exp(x[i] * 0.1);
            }
        }

        /// <summary>
        /// idx = 8: Logarithmic curve
        /// </summary>
        private static void GenerateLogarithmicCurve(double[] x, double[] y, int points, double amplitude)
        {
            for (int i = 0; i < points; i++)
            {
                x[i] = (i + 1) * 0.01; // +1 to avoid log(0)
                y[i] = amplitude * Math.Log(x[i] + 1);
            }
        }

        /// <summary>
        /// idx = 9: Noisy step function
        /// </summary>
        private static void GenerateNoisyStepFunction(double[] x, double[] y, int points, double amplitude)
        {
            var random = new Random();
            int stepSize = points / 10;

            for (int i = 0; i < points; i++)
            {
                x[i] = i * 0.01;
                int stepIndex = i / stepSize;
                double stepValue = (stepIndex % 2 == 0) ? amplitude : -amplitude;
                double noise = (random.NextDouble() - 0.5) * amplitude * 0.2;
                y[i] = stepValue + noise;
            }
        }

        #endregion

        /// <summary>
        /// Index'e göre data tipinin adını döndürür
        /// </summary>
        public static string GetDataTypeName(int idx)
        {
            return idx switch
            {
                0 => "Complex Wave with Noise",
                1 => "Simple Sine Wave",
                2 => "Cosine Wave",
                3 => "Square Wave",
                4 => "Sawtooth Wave",
                5 => "Random Walk",
                6 => "Multiple Harmonics",
                7 => "Exponential Growth",
                8 => "Logarithmic Curve",
                9 => "Noisy Step Function",
                _ => "Unknown Type"
            };
        }

        #region File I/O and Utility Methods

        /// <summary>
        /// Dosyadan veri okur (placeholder - gelecekte implement edilecek)
        /// </summary>
        public static void LoadData()
        {
            // TODO: Dosyadan veri okuma işlemi buraya eklenecek
        }

        /// <summary>
        /// Dosyaya veri yazar (placeholder - gelecekte implement edilecek)
        /// </summary>
        public static void SaveData()
        {
            // TODO: Dosyaya veri yazma işlemi buraya eklenecek
        }

        /// <summary>
        /// Veri işleme (placeholder - gelecekte implement edilecek)
        /// </summary>
        public static void ProcessData()
        {
            // TODO: Veri işleme mantığı buraya eklenecek
        }

        /// <summary>
        /// Veri doğrulama (placeholder - gelecekte implement edilecek)
        /// </summary>
        public static void ValidateData()
        {
            // TODO: Veri doğrulama mantığı buraya eklenecek
        }

        /// <summary>
        /// Kaynakları temizler
        /// </summary>
        public static void Dispose()
        {
            // TODO: Temizleme işlemleri buraya eklenecek
        }

        #endregion
    }
}
