using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using System.IO;
using System.Runtime.Intrinsics;
using FftSharp.Windows;
using NDtw;
using Complex = System.Numerics.Complex;


namespace Spritzgusslogik
{
    public class Identify
    {
        //Brauchen wir eig nicht mehr
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

        //Brauchen wir eig nicht mehr
        private static double[] GetMagnitudeSpectrum(Complex[] fft)
        {
            return fft.Select(c => c.Magnitude).ToArray();
        }

        //Brauchen wir eig nicht mehr
        private static Complex[] PadArray(Complex[] array, int targetLength)
        {
            var result = new Complex[targetLength];
            Array.Copy(array, result, array.Length);
            return result; // Rest ist automatisch (0, 0)
        }

        //Brauchen wir eig nicht mehr
        public static Complex[] LoadComplexArrayCsv(string filePath)
        {
            var result = new List<Complex>();

            foreach (var line in File.ReadLines(filePath))
            {
                var parts = line.Split(',');
                if (parts.Length != 2) continue; // Skip invalid lines

                if (double.TryParse(parts[0], out double real) &&
                    double.TryParse(parts[1], out double imag))
                {
                    result.Add(new Complex(real, imag));
                }
                else
                {
                    throw new FormatException($"Ungültige Zeile in CSV: {line}");
                }
            }

            return result.ToArray();
        }

        //Richtungs-/Winkel-Sensitiv
        public static double CosineSimilarity(double[] arrayA, double[] arrayB)
        {
            double dot = 0, normA = 0, normB = 0;
            for (int i = 0; i < arrayA.Length; i++)
            {
                dot += arrayA[i] * arrayB[i];       //Skalarprodukt
                normA += arrayA[i] * arrayA[i];     //aufsummierte Quadrate = Länge von a^2
                normB += arrayB[i] * arrayB[i];     //aufsummierte Quadrate = Länge von b^2
            }

            //Cosine-Similarity
            return dot / (Math.Sqrt(normA) * Math.Sqrt(normB));
        }

        //Absoluter-Abstand-Sensitiv
        public static double EuclideanDistance(double[] a, double[] b)
        {
            double sum = 0;
            for (int i = 0; i < a.Length; i++)
                sum += Math.Pow(a[i] - b[i], 2);
            return Math.Sqrt(sum);
        }

        //Lineare-Korrelation-Sensitiv
        public static double PearsonCorrelation(double[] a, double[] b)
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

        public static double compare(double[] a, double[] b)
        {
            double cosine = CosineSimilarity(a, b);
            double pearson = (PearsonCorrelation(a, b) + 1) / 2;
            double euclidean = 1.0 / (1.0 + EuclideanDistance(a, b)); // Abstand zu Ähnlichkeit

            Console.WriteLine($"Cosine: {cosine:F3}, Pearson: {pearson:F3}, Euclid: {euclidean:F3}");

            //Cosine & Pearson wichtig, euclidean nur ergänzend
            return (0.45 * cosine) + (0.45 * pearson) + (0.1 * euclidean); //je höher desto besser
        }
    }
}
