/** 
* @file Identify2.cs 
* @brief File is a Program that Identifies Instructions (1-9).
* @author Donart Bazaj 
* @date 10.06.2025 
* 
* Additional Infos: none
*/
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
using Accord.Math.Distances;
using Accord.Math.Metrics;

namespace Spritzgusslogik
{
    public class Identify2
    {

        Dictionary<int, List<double[]>> referenceData = new Dictionary<int, List<double[]>>();


        public double DistanceThreshold { get; set; } = 0.5;

        /** 
        * @fn Identify2()
        * @brief Conctructor. 
        * @param none 
        * @return nothing 
        * @author Donart Bazaj
        * @date 10.06.2025 
        * 
        * 
        */
        public Identify2()
        {
            referenceData = loadReferenceData();
            Debug.WriteLine("Referenz Data größe: " + referenceData.Count);

        }

        /** 
        * @fn Dictionary<int, List<double[]>> loadReferenceData()
        * @brief Loads the Reference Files. 
        * @param void 
        * @return  Dictionary<int, List<double[]>>, keyPairValue of int(1-9) and List of double[] , Reference Data.
        * @author Donart Bazaj
        * @date 10.06.2025 
        * 
        * This method reads all Reference Data and saves in creates an Dictionary<int, List<double[]>> where it saves all
        *  Reference Data.
        */
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
        /** 
        * @fn double CosineSimilarity(double[] arrayA, double[] arrayB)
        * @brief Calculates Cosine Similarity. 
        * @param  arrayA, double Array
        * @param  arrayB, double Array
        * @return  double Value, similarity
        * @author Donart Bazaj
        * @date 10.06.2025 
        * 
        * This Method Calculates the Cosine Similarity between 2 given double Arrays.
        */
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
        /** 
        * @fn double EuclideanDistance(double[] a, double[] b)
        * @brief Calculates Euclidean Distance. 
        * @param  a, double Array
        * @param  b, double Array
        * @return  double Value, similarity
        * @author Donart Bazaj
        * @date 10.06.2025 
        * 
        * This Method Calculates the Euclidean Distance between 2 given double Arrays.
        */
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


        // Führt einen Bitmuster-Vergleich zwischen den Referenzdaten und dem aktuellen Eingabesignal durch.
        // Die Befehle werden leider trotzdem nicht richtig erkannt.
        /** 
        * @fn int Recognize(Int16[] rawInput, out double bestSimilarity)
        * @brief Regonizes the  Instruction.
        * @param  a, double Array
        * @param  bestSimilarity
        * @return  int, regonized Command.
        * @author Donart Bazaj
        * @date 10.06.2025 
        * 
        * This Method regonizes the spoken Command. It compares the given Values with Reference Data.
        */
        //public int Recognize(Int16[] rawInput, out double bestSimilarity)
        //{
        //    SignalFFTSharpTransformer st = new SignalFFTSharpTransformer();

        //    Debug.WriteLine("References: " + referenceData[1].Count);



        //    if (rawInput == null) throw new ArgumentNullException(" Input is NULL!!");

        //    //var inpT = st.Transform(rawInput);
        //     //var input = NormalizeMax(inpT);
        //    float[] floatInput = rawInput.Select(x => x / 32768f).ToArray();
        //    double[] mfcc = audioConvert.MfccFromFloat(floatInput);
        //    int bestCmd = -1;
        //    bestSimilarity = 0.0;

        //    double bestDist = double.MaxValue;

        //    // Schwellwert bestimmen (z. B. 10% vom Maximum)
        //    double threshold = 0.03;//1e-7;

        //    /*double normFactor = mfcc.Max();
        //    if (normFactor > 0)
        //        mfcc = mfcc.Select(x => x / normFactor).ToArray();*/
        //  var mfccN = NormalizeMax(mfcc);

        //    // Eingabespektrum in Bitmuster wandeln
        //    bool[] inputBits = mfccN.Select(x => x > threshold).ToArray();
        //    Debug.WriteLine("FFT Input: " + string.Join(", ", mfccN.Take(10)));
        //   // Debug.WriteLine("Max: " + mfcc.Max());
        //   // Debug.WriteLine("Bitmuster: " + string.Join("", inputBits.Select(b => b ? "1" : "0")));

        //    for (var kv = 1; kv < 10; kv++ )
        //    {
        //        double avgSim = 0.0;
        //        int c = 0;
        //        //int cmd = kv.Key;
        //        foreach (var rawRefVec in referenceData[kv])
        //        {
        //           // Debug.WriteLine("Länge: refVec: " + rawRefVec.Length + " Input: " + mfcc.Length);
        //            if (rawRefVec.Length != mfccN.Length)
        //                continue; // Optional: Mit DTW vergleichen, wenn Längen verschieden


        //            var refVec = NormalizeMax(rawRefVec);
        //            // Referenz in Bitmuster wandeln
        //               bool[] refBits = refVec.Select(x => x > threshold).ToArray();

        //             int same = 0;
        //             for (int i = 0; i < inputBits.Length; i++)
        //             {
        //                 if (inputBits[i] == refBits[i])
        //                     same++;
        //             }








        //           // double similarity = (double) same / inputBits.Length;

        //            // double simCosine = CosineSimilarity(refVec, mfccN);
        //           //  double dist = EuclideanDistance(mfcc, refVec);
        //          //    double simHybrid = 0.5  * similarity + 0.5 * simCosine;

        //          var simHybrid = (double)same/ refBits.Length;
        //            avgSim += simHybrid;
        //            c++;


        //        }

        //        avgSim /= c;
        //        Debug.WriteLine("Sim: " + avgSim + " bei Key: " + kv);
        //        if (avgSim >= bestSimilarity)
        //        {
        //            bestSimilarity = avgSim;
        //            bestCmd = kv;
        //            Debug.WriteLine("Best Command: " + bestCmd + " Best Similarity: " + bestSimilarity);
        //            // Debug.WriteLine("Reference Daten: " + string.Join(", ", refVec.Take(10)) + "\n Input Daten: " + string.Join(", ", mfccN.Take(10)));

        //        }
        //    }

        //    // Ähnlichkeit aus Abstand berechnen (normalisiert auf 0.0 bis 1.0)
        //    // Annahme: dist = 0 bedeutet perfekt ähnlich, große dist = unähnlich
        //    // bestSimilarity = 1.0 / (1.0 + bestDist); // z.B. dist=0 => sim=1.0, dist=9 => sim~0.1
        //    //var d = referenceData[bestCmd];

        //    return bestCmd;
        //}



        /** 
         * @fn double[] ConvertFftToMfcc(double[] fftMag, int numFilters = 26, int numCoeffs = 13)
         * @brief It converts from Fft to Mfcc-Vectors of length 13.
         * @param  fftMag, double Array
         * @param  numFilters, 26
         * @param  numCoeffs, 13
         * @return  int, regonized Command.
         * @author Donart Bazaj
         * @date 10.06.2025 
         * 
         * It converts from Fft Values to Mfcc-Vectors of length 13. 
         */
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
        /** 
        * @fn    double[] DctII(double[] input)
        * @brief Easy DCT-II Implementiation
        * @param  input, double Array
        * @return  result
        * @author Donart Bazaj
        * @date 10.06.2025 
        * 
        * Easy DCT-II Implementiation
        */
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



        /** 
        * @fn    double[] ApplyMelFilterbank(double[] fftMag, int sampleRate, int fftSize, int numFilters = 13)
        * @brief It is needed to Convert From Fft to Mfcc
        * @param  fftMag, double Array
        * @param sampleRate
        * @param  fftSize
        * @param numFilters, 13
        * @return  filterEnergies
        * @author Donart Bazaj
        * @date 10.06.2025 
        * 
        * It is needed to Convert From Fft to Mfcc
        */
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

        /** 
        * @fn    double[] NormalizeMax(double[] v)
        * @brief Normalizes an double Array.
        * @param  v, double Array
        * @return  normalised double []
        * @author Donart Bazaj
        * @date 10.06.2025 
        * 
        * Normalizes a vector of doubles by dividing each element by the maximum absolute value.
        * This scales the values into the range [-1, 1].
        * It Returns  a new double[] array where each value is divided by the maximum absolute value of v .
        *   If all values are zero, the original array is returned.
        */
        private double[] NormalizeMax(double[] v)
        {
            double m = v.Select(x => Math.Abs(x)).DefaultIfEmpty(0).Max();
            if (m <= 0) return v;
            return v.Select(x => x / m).ToArray();
        }

        /*
        double ComputeItakuraSaito(double[] X, double[] Y)
        {
            double sum = 0;
            for (int i = 0; i < X.Length; i++)
            {
                if (Y[i] == 0) return double.PositiveInfinity;
                double r = X[i] / Y[i];
                if (r <= 0 || double.IsNaN(r))  return double.PositiveInfinity;
                sum += r - Math.Log(r) - 1;
            }
            return sum;
        }

        // Levinson-Durbin Algorithmus zur Berechnung der LPC-Koeffizienten
        public static double[] ComputeLPC(double[] signal, int order)
        {
            int n = signal.Length;
            double[] autocorr = new double[order + 1];

            // Autokorrelation berechnen
            for (int i = 0; i <= order; i++)
            {
                for (int j = 0; j < n - i; j++)
                {
                    autocorr[i] += signal[j] * signal[j + i];
                }
            }

            double[] a = new double[order + 1];
            double[] e = new double[order + 1];
            double[] k = new double[order + 1];

            a[0] = 1.0;
            e[0] = autocorr[0];

            for (int i = 1; i <= order; i++)
            {
                double sum = 0.0;
                for (int j = 1; j < i; j++)
                {
                    sum += a[j] * autocorr[i - j];
                }

                k[i] = (autocorr[i] - sum) / e[i - 1];
                a[i] = k[i];

                for (int j = 1; j < i; j++)
                {
                    a[j] = a[j] - k[i] * a[i - j];
                }

                e[i] = e[i - 1] * (1.0 - k[i] * k[i]);
            }

            // Nur die Koeffizienten zurückgeben (ohne a[0])
            double[] result = new double[order];
            Array.Copy(a, 1, result, 0, order);
            return result;
        }
        */
    }
}

