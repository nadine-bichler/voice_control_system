///
/// @file    Identifier
/// @brief   Identifies Digit from audio
/// @author  Bichler Nadine, Anton Armin
/// @date    24.06.2025
///
///

using System.IO;
using Complex = System.Numerics.Complex;
using System.Diagnostics;



namespace Spritzgusslogik
{
    /// <summary>
    /// Identifies voice command
    /// </summary>
    public class Identify
    {
        //const int TargetLength = 2048;

        /// <deprecated>
        /// Brauchen wir eig nicht mehr
        /// </deprecated>
        /*
        public static double PearsonCorrelation(Complex[] arrayA, Complex[] arrayB)
        {
            Console.WriteLine(arrayA.Length);
            Console.WriteLine(arrayB.Length);

            var maxLength = Math.Max(arrayA.Length, arrayB.Length);

            var x = GetMagnitudeSpectrum(PadArray(arrayA, maxLength));
            var y = GetMagnitudeSpectrum(PadArray(arrayB, maxLength));

            if (x.Length != y.Length)
                throw new ArgumentException("Arrays must be the same length");

            double meanX = x.Average();
            double meanY = y.Average();

            double numerator = 0, denomX = 0, denomY = 0;
            for (int i = 0; i < x.Length; i++)
            {
                double dx = x[i] - meanX;
                double dy = y[i] - meanY;
                numerator += dx * dy;
                denomX += dx * dx;
                denomY += dy * dy;
            }

            return numerator / Math.Sqrt(denomX * denomY);
        }
        */

        /// <deprecated>
        /// Brauchen wir eig nicht mehr
        /// </deprecated>
        /*
        private static double[] GetMagnitudeSpectrum(Complex[] fft)
        {
            return fft.Select(c => c.Magnitude).ToArray();
        }
        */

        /// <deprecated>
        /// Brauchen wir eig nicht mehr
        /// </deprecated>
        /*
        private static Complex[] PadArray(Complex[] array, int targetLength)
        {
            var result = new Complex[targetLength];
            Array.Copy(array, result, array.Length);
            return result; // Rest ist automatisch (0, 0)
        }
        */

        // Richtungs-/Winkel-Sensitiv
        private static double CosineSimilarity(double[] arrayA, double[] arrayB)
        {
            if (arrayA.Length != arrayB.Length)
            {
                throw new ArgumentException("Arrays must be the same length");
            }

            double dot = 0, normA = 0, normB = 0;
            for (int i = 0; i < arrayA.Length; i++)
            {
                dot += arrayA[i] * arrayB[i];
                normA += arrayA[i] * arrayA[i];
                normB += arrayB[i] * arrayB[i];
            }

            return dot / (Math.Sqrt(normA) * Math.Sqrt(normB));
        }

        // Absoluter-Abstand-Sensitiv
        private static double EuclideanDistance(double[] a, double[] b)
        {
            double sum = 0;
            for (int i = 0; i < a.Length; i++)
                sum += Math.Pow(a[i] - b[i], 2);
            return Math.Sqrt(sum);
        }

        // Lineare-Korrelation-Sensitiv
        private static double PearsonCorrelation(double[] a, double[] b)
        {
            if (a.Length != b.Length)
                throw new ArgumentException("Arrays müssen gleich lang sein.");

            double meanA = a.Average();
            double meanB = b.Average();

            double numerator = 0, denomA = 0, denomB = 0;
            for (int i = 0; i < a.Length; i++)
            {
                double da = a[i] - meanA;
                double db = b[i] - meanB;
                numerator += da * db;
                denomA += da * da;
                denomB += db * db;
            }

            return numerator / Math.Sqrt(denomA * denomB);
        }

        // Vergleich aller Metriken
        private static double Compare(double[] a, double[] b)
        {
            double cosine = CosineSimilarity(a, b);
            double pearson = (PearsonCorrelation(a, b) + 1) / 2;
            double euclidean = 1.0 / (1.0 + EuclideanDistance(a, b));

            Console.WriteLine($"Cosine: {cosine:F3}, Pearson: {pearson:F3}, Euclid: {euclidean:F3}");

            return (0.5 * cosine) + (0.25 * pearson) + (0.25 * euclidean);
        }

        public static int Recognize(double[] input, out double bestSimilarity)
        {
            var bestMatch = -1;
            bestSimilarity = 0.0;

            for (int i = 1; i <= 9; i++)
            {
                var scores = new List<double>();

                var basePath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "SampleToFile",
                    "WAV_samples");

                var files = Directory.GetFiles(basePath, $"*{i}.csv");

                foreach (var file in files)
                {
                    var test = audioConvert.LoadDoubleArrayCsv(file);
                    var sim = Compare(input, test);
                    scores.Add(sim);
                }

                if (scores.Count == 0) continue;

                var avgScore = scores.Average();

                if (avgScore > bestSimilarity)
                {
                    bestSimilarity = avgScore;
                    bestMatch = i;
                    var d = audioConvert.LoadDoubleArrayCsv(files[0]);
                    Debug.WriteLine("Reference Daten: " + string.Join(", ", d.Take(10)) + "\n Input Daten: " +
                                    string.Join(", ", input.Take(10)));
                }
            }

            return bestMatch;
        }
    }
}