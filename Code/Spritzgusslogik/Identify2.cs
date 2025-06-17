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

namespace Spritzgusslogik
{
    public class Identify2
    {

        Dictionary<int, List<double[]>> referenceData = new Dictionary<int, List<double[]>>();


        public double DistanceThreshold { get; set; } = 0.5;

        public Identify2()
        {
            referenceData = loadReferenceData();

        }

        public Dictionary<int, List<double[]>> loadReferenceData()
        {
            Dictionary<int, List<double[]>> refData = new Dictionary<int, List<double[]>>();
            
            for (int i = 1; i < 10; i++)
            {
                List<double[]> cmds = new List<double[]>();

                var basePath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "SampleToFile", "WAV_samples");
                var files = Directory.GetFiles(basePath, $"*{i}.csv");

                foreach(var file in files)
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
            int bestCmd = -1;
            bestSimilarity = 0.0;

            double bestDist = double.MaxValue;

            // Schwellwert bestimmen (z. B. 10% vom Maximum)
            double threshold = input.Max() * 0.1;

            // Eingabespektrum in Bitmuster wandeln
            bool[] inputBits = input.Select(x => x > threshold).ToArray();

            foreach (var kv in referenceData)
            {
                int cmd = kv.Key;
                foreach (var refVec in kv.Value)
                {
                    if (refVec.Length != input.Length)
                        continue; // Optional: Mit DTW vergleichen, wenn Längen verschieden

                    // double dist = EuclideanDistance(input, refVec);

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
                        bestCmd = cmd;
                    }
                }
            }

            // Ähnlichkeit aus Abstand berechnen (normalisiert auf 0.0 bis 1.0)
            // Annahme: dist = 0 bedeutet perfekt ähnlich, große dist = unähnlich
            bestSimilarity = 1.0 / (1.0 + bestDist); // z.B. dist=0 => sim=1.0, dist=9 => sim~0.1

            return bestCmd;
        }
            
               




       

        }






    }

