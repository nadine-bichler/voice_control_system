using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using System.IO;
using System.Runtime.Intrinsics;
using NDtw;
using Complex = System.Numerics.Complex;


namespace Spritzgusslogik
{
    public class Identify
    {
        //Correlation
        public static double compare(Complex[] arrayA, Complex[] arrayB)
        {
            Console.WriteLine(arrayA.Length);
            Console.WriteLine(arrayB.Length);

            var maxLength = Math.Max(arrayA.Length, arrayB.Length);


            var x = GetMagnitudeSpectrum(PadArray(arrayA,maxLength));
            var y = GetMagnitudeSpectrum(PadArray(arrayB,maxLength));


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

        private static double[] GetMagnitudeSpectrum(Complex[] fft)
        {
            return fft.Select(c => c.Magnitude).ToArray();
        }

        private static Complex[] PadArray(Complex[] array, int targetLength)
        {
            var result = new Complex[targetLength];
            Array.Copy(array, result, array.Length);
            return result; // Rest ist automatisch (0, 0)
        }

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

        //DTW
        public static double compare2(double[] arrayA, double[] arrayB)
        {
            return new Dtw(arrayA, arrayB).GetCost() + 1.0;
        }


        //DTW with Chat(normalizing & scaling)
        public static double compare3(double[] arrayA, double[] arrayB)
        {
            if (arrayA == null || arrayB == null || arrayA.Length == 0 || arrayB.Length == 0)
                throw new ArgumentException("Input arrays must not be null or empty.");

            var normA = Normalize(arrayA);
            var normB = Normalize(arrayB);

            var dtw = new Dtw(normA, normB);
            return dtw.GetCost() + 1.0;
        }

        //Normalizing
        private static double[] Normalize(double[] input)
        {
            double max = input.Max();
            if (max == 0) return input;
            return input.Select(x => x / max).ToArray();
        }
    }
}
