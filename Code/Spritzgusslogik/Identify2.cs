using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spritzgusslogik
{
    public class Identify2
    {

        Dictionary<int, double[]> referenceData = new Dictionary<int, double[]>();




        public Identify2()
        { 
           

        }

        public Dictionary<int, double[]> loadReferenceData(string path)
        {
            Dictionary<int, double[]> refData = new Dictionary<int, double[]>();
            var basePath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "SampleToFile", "WAV_samples");

            var files = Directory.GetFiles(basePath, $"*{i}.csv");


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
            double sum = 0;
            for (int i = 0; i < a.Length; i++)
                sum += Math.Pow(a[i] - b[i], 2);
            return Math.Sqrt(sum);
        }








    }
}
