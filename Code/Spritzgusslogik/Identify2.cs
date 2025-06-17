using Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Printing;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Shapes;
using Path = System.IO.Path;
using Debug = System.Diagnostics.Debug;
using MathNet.Numerics.LinearAlgebra;

namespace Spritzgusslogik
{
    public class Identify2
    {

        Dictionary<int, List<double[]>> referenceData = new Dictionary<int, List<double[]>>();


        public double DistanceThreshold { get; set; } = 0.5;

        public Identify2()
        {
            referenceData = loadReferenceData();
            Debug.WriteLine("Referenz Data größe: " + referenceData.Count);

        }

        public Dictionary<int, List<double[]>> loadReferenceData()
        {
            Dictionary<int, List<double[]>> refData = new Dictionary<int, List<double[]>>();

            for (int i = 1; i < 10; i++)
            {
                List<double[]> cmds = new List<double[]>();

                var basePath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "SampleToFile", "WAV_samples");
                var files = Directory.GetFiles(basePath, $"*{i}.csv");

                foreach (var file in files)
                {
                    var cmd = audioConvert.LoadDoubleArrayCSV(file);
                    cmds.Add(cmd);

                }

                refData.Add(i, cmds);

            }
            return refData;
        }

        //Richtungs-/Winkel-Sensitiv
        //Robust gegen Lautstärkeunterschiede = Ideal für Richtung & Klangmuster
        //Ergebnis von 0-1 (je höher desto besser)
        public static double CosineSimilarity(double[] arrayA, double[] arrayB)
        {
            if (arrayA.Length != arrayB.Length)
            {
                throw new ArgumentException("Arrays must be the same length");
            }

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
        //Robust gegen Verschiebungen = Idel für Strukturähnlichkeiten
        //Ergebnis mit Zahlen (je näher an 0 desto besser)
        public static double EuclideanDistance(double[] a, double[] b)
        {
            double sum = 0.0;
            for (int i = 0; i < a.Length; i++)
            {
                double diff = a[i] - b[i];
                sum += diff * diff;
            }
            return Math.Sqrt(sum);
        }


        public int Recognize(double[] input, out double bestSimilarity)
        {
            if (input == null) throw new ArgumentNullException(" Input is NULL!!");

            double[] mfcc = ConvertFftToMfcc(input);
            int bestCmd = -1;
            bestSimilarity = 0.0;

            double bestDist = double.MaxValue;

            // Schwellwert bestimmen (z. B. 10% vom Maximum)
            double threshold = 1e-7;

            double normFactor = mfcc.Max();
            if (normFactor > 0)
                mfcc = mfcc.Select(x => x / normFactor).ToArray();

            // Eingabespektrum in Bitmuster wandeln
            bool[] inputBits = mfcc.Select(x => x > threshold).ToArray();
            Debug.WriteLine("FFT Input: " + string.Join(", ", mfcc.Take(10)));
            Debug.WriteLine("Max: " + mfcc.Max());
            Debug.WriteLine("Bitmuster: " + string.Join("", inputBits.Select(b => b ? "1" : "0")));
            foreach (var kv in referenceData)
            {
                //int cmd = kv.Key;
                foreach (var refVec in kv.Value)
                {
                    Debug.WriteLine("Länge: refVec: " + refVec.Length + " Input: " + mfcc.Length);
                    if (refVec.Length != mfcc.Length)
                        continue; // Optional: Mit DTW vergleichen, wenn Längen verschieden

                    // double dist = EuclideanDistance(mfcc, refVec);

                      // Referenz in Bitmuster wandeln
                      bool[] refBits = refVec.Select(x => x > threshold).ToArray();

                      int same = 0;
                      for (int i = 0; i < inputBits.Length; i++)
                      {
                          if (inputBits[i] == refBits[i])
                              same++;
                      }


                    double similarity = (double)same / inputBits.Length;
                    if (similarity > bestSimilarity)
                    {
                        bestSimilarity = similarity;
                        bestCmd = kv.Key;
                    }
                }
            }

            // Ähnlichkeit aus Abstand berechnen (normalisiert auf 0.0 bis 1.0)
            // Annahme: dist = 0 bedeutet perfekt ähnlich, große dist = unähnlich
           // bestSimilarity = 1.0 / (1.0 + bestDist); // z.B. dist=0 => sim=1.0, dist=9 => sim~0.1

            return bestCmd;
        }





        public static double[] ConvertFftToMfcc(double[] fftMag, int numFilters = 26, int numCoeffs = 13)
        {
            int sampleRate = 44100;
            int fftSize = fftMag.Length * 2;

            // Mel-Filterbank anwenden
            double[] melEnergies = ApplyMelFilterbank(fftMag, sampleRate, fftSize, numFilters);

            // Logarithmieren
            double[] logMel = melEnergies.Select(e => Math.Log(e + 1e-10)).ToArray();

            // DCT-II anwenden
            double[] mfcc = DctII(logMel);

            return mfcc.Take(numCoeffs).ToArray();
        }

        // Einfache eigene DCT-II Implementation
        private static double[] DctII(double[] input)
        {
            int N = input.Length;
            double[] result = new double[N];

            for (int k = 0; k < N; k++)
            {
                double sum = 0.0;
                for (int n = 0; n < N; n++)
                {
                    sum += input[n] * Math.Cos(Math.PI * k * (2 * n + 1) / (2.0 * N));
                }

                result[k] = sum * Math.Sqrt(2.0 / N);
            }

            result[0] *= 1.0 / Math.Sqrt(2); // Normierung für k = 0
            return result;
        }

        public static double[] ApplyMelFilterbank(double[] fftMag, int sampleRate, int fftSize, int numFilters = 13)
        {
            double melMin = 0;
            double melMax = 2595 * Math.Log10(1 + (sampleRate / 2.0) / 700);
            double[] melPoints = Enumerable.Range(0, numFilters + 2)
                .Select(i => melMin + i * (melMax - melMin) / (numFilters + 1))
                .Select(m => 700 * (Math.Pow(10, m / 2595.0) - 1))
                .ToArray();

            int[] bin = melPoints.Select(f => (int)Math.Floor((fftSize + 1) * f / sampleRate)).ToArray();
            double[] filterEnergies = new double[numFilters];

            for (int i = 1; i <= numFilters; i++)
            {
                double energy = 0.0;
                for (int j = bin[i - 1]; j < bin[i]; j++)
                {
                    double weight = (j - bin[i - 1]) / (double)(bin[i] - bin[i - 1]);
                    energy += fftMag.ElementAtOrDefault(j) * weight;
                }
                for (int j = bin[i]; j < bin[i + 1]; j++)
                {
                    double weight = (bin[i + 1] - j) / (double)(bin[i + 1] - bin[i]);
                    energy += fftMag.ElementAtOrDefault(j) * weight;
                }
                filterEnergies[i - 1] = energy;
            }

            return filterEnergies;


        }

        }
}

