using Microsoft.VisualBasic;
using System.Globalization;
using System.Numerics;
using System.Globalization;
using System.IO;
using Complex = System.Numerics.Complex;
using NAudio.Wave;
using NWaves.FeatureExtractors.Options;
using NWaves.FeatureExtractors;
using NWaves.Signals;

namespace Common
{
    public class audioConvert
    {
        public static Int16[] ConvertMp3ToInt16Array(string mp3FilePath)
        {
            using (var mp3Reader = new Mp3FileReader(mp3FilePath))
            {
                // WaveFormat für PCM 16-bit
                var pcmFormat = new WaveFormat(44100, 16, 1);

                // Resampler für Konvertierung zum PCM-Format
                using (var resampler = new WaveFormatConversionStream(pcmFormat, mp3Reader))
                {
                    using (var ms = new MemoryStream())
                    {
                        resampler.CopyTo(ms);
                        byte[] pcmBytes = ms.ToArray();

                        // Bytes in short-Array umwandeln (Int16)
                        Int16[] pcmShorts = new Int16[pcmBytes.Length / 2];
                        Buffer.BlockCopy(pcmBytes, 0, pcmShorts, 0, pcmBytes.Length);

                        return pcmShorts;
                    }
                }
            }
        }

        //brauchen wir überhaupt?
        public static void SaveInt16ArrayAsWav(short[] samples, string outputFilePath, int sampleRate = 44100, int channels = 1)
        {
            // Erstelle ein WaveFormat (z.B. 44100 Hz, 16-bit, mono)
            var waveFormat = new WaveFormat(sampleRate, 16, channels);

            // Öffne einen WaveFileWriter, um die WAV-Datei zu schreiben
            using (var writer = new WaveFileWriter(outputFilePath, waveFormat))
            {
                // Konvertiere das short[] in byte[]
                byte[] byteBuffer = new byte[samples.Length * 2];
                Buffer.BlockCopy(samples, 0, byteBuffer, 0, byteBuffer.Length);

                // Schreibe die Bytes in die WAV-Datei
                writer.Write(byteBuffer, 0, byteBuffer.Length);
            }
        }

        //brauchen wir überhaupt?
        public static void SaveComplexArrayCsv(string filePath, Complex[] data)
        {
            using (var writer = new StreamWriter(filePath))
            {
                foreach (var c in data)
                {
                    writer.WriteLine($"{c.Real.ToString(CultureInfo.InvariantCulture)},{c.Imaginary.ToString(CultureInfo.InvariantCulture)}");
                }
            }
        }

        public static void SaveDoubleArrayCsv(string filePath, Double[] data)
        {
            using (var writer = new StreamWriter(filePath))
            {
                foreach (var c in data)
                {
                    writer.WriteLine($"{c}");
                }
            }
        }

        //?brauchen wir überhaupt?
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

        //? brauchen wir überhaupt?
        public static Double[] LoadDoubleArrayCsv(string filePath)
        {
            var result = new List<Double>();

            foreach (var line in File.ReadLines(filePath))
            {
                result.Add(Convert.ToDouble(line));
            }

            return result.ToArray();
        }

        public static double[] LoadDoubleArrayCSV(string filePath)
        {
            var line = File.ReadLines(filePath).First(); // erste Zeile mit allen Werten
            return line
                .Split(',')                              // Trenne an Kommas
                .Select(v => Convert.ToDouble(v.Trim(), CultureInfo.InvariantCulture))  // Umwandeln in double
                .ToArray();
        }

        public static double[] MfccFromDouble(float[] input, int inputRate = 16000)
        {
            var signal = new DiscreteSignal(inputRate, input);

            var mfccOptions = new MfccOptions
            {
                SamplingRate = inputRate,
                FeatureCount = 13,               // wie in deinem Python-Code
                FrameDuration = 0.025,           // 25ms Frames
                HopDuration = 0.01,              // 10ms Schrittweite
                PreEmphasis = 0.97,
                FilterBankSize = 26,
                //NWaves nutzt automatisch einen Bereich zwischen 300-8000(Besser für Sprachverarbeitung)
            };

            var extractor = new MfccExtractor(mfccOptions);

            //Merkmalmatrix berechnen
            var mfccVectors = extractor.ComputeFrom(signal);

            //Mittelwert-Vektor = 13Werte
            var meanVector = new double[13];
            for (int i = 0; i < mfccVectors.Count; i++)
            {
                for (int j = 0; j < 13; j++)
                {
                    meanVector[j] += mfccVectors[i][j];
                }
            }

            for (int i = 0; i < 13; i++)
            {
                meanVector[i] /= mfccVectors.Count;
            }

            return meanVector;
        }
    }
}

