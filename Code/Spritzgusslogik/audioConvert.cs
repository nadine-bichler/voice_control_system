using System.Globalization;
using System.IO;
using Complex = System.Numerics.Complex;
using NAudio.Wave;

namespace Spritzgusslogik
{
    /// <summary>
    /// Converting input to file or other way around 
    /// </summary>
    public class audioConvert
    {
        /// <summary>
        /// @deprecated Veralteter Code für FFT-Transformation und WAV/CSV-Konvertierung.
        /// Verwende stattdessen MFCC-Extraktion über Python.
        /// </summary>
        /*
        SignalTransformer transformer = new SignalTransformer();

        public static void SaveInt16ArrayAsWav(short[] samples, string outputFilePath, int sampleRate = 44100, int channels = 1)
        {
            var waveFormat = new WaveFormat(sampleRate, 16, channels);
            using (var writer = new WaveFileWriter(outputFilePath, waveFormat))
            {
                byte[] byteBuffer = new byte[samples.Length * 2];
                Buffer.BlockCopy(samples, 0, byteBuffer, 0, byteBuffer.Length);
                writer.Write(byteBuffer, 0, byteBuffer.Length);
            }
        }

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

        public static Complex[] LoadComplexArrayCsv(string filePath)
        {
            var result = new List<Complex>();
            foreach (var line in File.ReadLines(filePath))
            {
                var parts = line.Split(',');
                if (parts.Length != 2) continue;

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

        public static Double[] LoadDoubleArrayCsv(string filePath)
        {
            var result = new List<Double>();
            foreach (var line in File.ReadLines(filePath))
            {
                result.Add(Convert.ToDouble(line));
            }
            return result.ToArray();
        }

        public static Complex[] LoadComplexArrayCsv(string filePath)
        {
            if (!File.Exists(filePath))
                return null;

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

        public static void SaveDoubleArrayCsv(string filePath, Double[] data)
        {
            using var writer = new StreamWriter(filePath);
            foreach (var c in data)
            {
                writer.WriteLine($"{c}");
            }
        }

        public static Int16[] ConvertMp3ToInt16Array(string mp3FilePath)
        {
            using var mp3Reader = new Mp3FileReader(mp3FilePath);
            var pcmFormat = new WaveFormat(44100, 16, 1);
            using var resampler = new WaveFormatConversionStream(pcmFormat, mp3Reader);
            using var ms = new MemoryStream();
            resampler.CopyTo(ms);
            byte[] pcmBytes = ms.ToArray();
            Int16[] pcmShorts = new Int16[pcmBytes.Length / 2];
            Buffer.BlockCopy(pcmBytes, 0, pcmShorts, 0, pcmBytes.Length);
            return pcmShorts;
        }
        */

        /// <summary>
        /// converts a csv file to a double array
        /// </summary>
        /// <param name="filePath">file path to csv file</param>
        /// <returns>values of csv to a double array</returns>
        public static double[] LoadDoubleArrayCsv(string filePath)
        {
            var line = File.ReadLines(filePath).First();
            return line
                .Split(',')
                .Select(v => Convert.ToDouble(v.Trim(), CultureInfo.InvariantCulture))
                .ToArray();
        }

        /// <summary>
        /// saves pcm to a wav
        /// </summary>
        /// <param name="pcm">pcm input array</param>
        /// <param name="outputPath">path to output wav file</param>
        /// <param name="sampleRate">sample rate for wav</param>
        public static void SavePcmAsWav(short[] pcm, string outputPath, int sampleRate = 44100)
        {
            using var memoryStream = new MemoryStream();
            using var writer = new WaveFileWriter(outputPath, new WaveFormat(sampleRate, 16, 1));
            byte[] byteBuffer = new byte[pcm.Length * sizeof(short)];
            Buffer.BlockCopy(pcm, 0, byteBuffer, 0, byteBuffer.Length);
            writer.Write(byteBuffer, 0, byteBuffer.Length);
        }
    }
}