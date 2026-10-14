///
/// @file    Identify.cs
/// Identifies digits from audio input using dynamic time warping on MFCC sequences.
/// @author  Bichler Nadine, Anton Armin
/// @date    24.06.2025
///
///

using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Tests")]
namespace Spritzgusslogik
{
    /// <summary>
    /// Provides functionality to identify spoken digits based on extracted audio features.
    /// </summary>
    public class Identify
    {
        /// <summary>
        /// Calculates the Euclidean distance between two feature frames.
        /// </summary>
        private static double FrameDistance(double[] x, double[] y)
        {
            double sum = 0;
            for (int i = 0; i < x.Length; i++)
            {
                double d = x[i] - y[i];
                sum += d * d;
            }
            return Math.Sqrt(sum);
        }

        /// <summary>
        /// Calculates the normalized dynamic time warping (DTW) distance between two
        /// MFCC feature sequences. Lower values mean more similar sequences.
        /// </summary>
        /// <param name="a">First feature sequence (frames x coefficients).</param>
        /// <param name="b">Second feature sequence (frames x coefficients).</param>
        /// <returns>The accumulated DTW cost normalized by the total number of frames.</returns>
        internal static double DtwDistance(double[][] a, double[][] b)
        {
            int n = a.Length;
            int m = b.Length;
            if (n == 0 || m == 0)
                return double.PositiveInfinity;

            var prev = new double[m + 1];
            var cur = new double[m + 1];
            for (int j = 0; j <= m; j++)
                prev[j] = double.PositiveInfinity;
            prev[0] = 0.0;

            for (int i = 1; i <= n; i++)
            {
                cur[0] = double.PositiveInfinity;
                var ai = a[i - 1];
                for (int j = 1; j <= m; j++)
                {
                    double cost = FrameDistance(ai, b[j - 1]);
                    double best = Math.Min(prev[j], Math.Min(cur[j - 1], prev[j - 1]));
                    cur[j] = cost + best;
                }

                var tmp = prev;
                prev = cur;
                cur = tmp;
            }

            return prev[m] / (n + m);
        }

        /// <summary>
        /// Recognizes a spoken digit from a sequence of MFCC feature frames by comparing it
        /// to stored reference sequences using dynamic time warping. For each digit the
        /// closest reference (nearest neighbour) is used, so that a small set of references
        /// that match the speaker well is not diluted by many dissimilar (other-speaker)
        /// references.
        /// </summary>
        /// <param name="input">Feature sequence of the recorded digit (frames x coefficients).</param>
        /// <param name="bestSimilarity">Outputs a confidence in [0,1] derived from the margin between the best and second best digit.</param>
        /// <returns>The recognized digit (1-9), or -1 if no match is found.</returns>
        public static int RecognizeSequence(double[][] input, out double bestSimilarity)
        {
            if (input == null || input.Length == 0)
            {
                throw new ArgumentNullException(nameof(input));
            }

            var bestMatch = -1;
            double bestDistance = double.PositiveInfinity;
            double secondDistance = double.PositiveInfinity;

            var basePath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "SampleToFile",
                "WAV_sequences");

            for (int i = 1; i <= 9; i++)
            {
                var files = Directory.GetFiles(basePath, $"*{i}.csv");

                // As soon as the user has recorded his own references via the
                // "Referenzen aufnehmen" button (files named own*.csv), only those are
                // used. References from other speakers (spk*, Test*, single digit files)
                // are only a fallback: mixing voices in the nearest-neighbour comparison
                // causes systematic confusions (e.g. 2 <-> 3 or 9 -> 3/5).
                var ownFiles = files
                    .Where(f => Path.GetFileName(f).StartsWith("own", StringComparison.OrdinalIgnoreCase))
                    .ToArray();
                if (ownFiles.Length > 0)
                    files = ownFiles;

                var distances = new List<double>();
                foreach (var file in files)
                {
                    var reference = audioConvert.LoadDoubleArray2DCsv(file);

                    if (reference == null || reference.Length == 0)
                        continue;
                    if (reference[0].Length != input[0].Length)
                        continue;

                    distances.Add(DtwDistance(input, reference));
                }

                if (distances.Count == 0)
                    continue;

                // Nearest neighbour: take the closest reference for this digit. Averaging
                // would let dissimilar (other-speaker) references outweigh a good match.
                var digitDistance = distances.Min();

                if (digitDistance < bestDistance)
                {
                    secondDistance = bestDistance;
                    bestDistance = digitDistance;
                    bestMatch = i;
                }
                else if (digitDistance < secondDistance)
                {
                    secondDistance = digitDistance;
                }
            }

            if (bestMatch < 0 || double.IsInfinity(secondDistance))
            {
                bestSimilarity = 0.0;
                return bestMatch;
            }

            // Relative margin between best and second best digit, scaled to [0,1] for display.
            double margin = (secondDistance - bestDistance) / secondDistance;
            bestSimilarity = Math.Clamp(margin * 5.0, 0.0, 1.0);
            return bestMatch;
        }
    }
}
