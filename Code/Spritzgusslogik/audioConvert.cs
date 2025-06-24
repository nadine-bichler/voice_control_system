///
/// @file    audioConvert
/// @brief   Converting input to file or other way around
/// @author  Anton Armin, Bichler Nadine
/// @date    24.06.2025
///


using System.Globalization;
using System.IO;
using Complex = System.Numerics.Complex;
using NAudio.Wave;


namespace Common
{
    /// <summary>
    /// Converting input to file or other way around 
    /// </summary>
    public class audioConvert
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="mp3FilePath"></param>
        /// <returns></returns>
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
        /// <summary>
        /// 
        /// </summary>
        /// <param name="samples"></param>
        /// <param name="outputFilePath"></param>
        /// <param name="sampleRate"></param>
        /// <param name="channels"></param>
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
        /// <summary>
        /// 
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="data"></param>
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

        /// <summary>
        /// saves an Double Array into an csv
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="data"></param>
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
        /// <summary>
        /// 
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        /// <exception cref="FormatException"></exception>
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
        /// <summary>
        /// 
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static Double[] LoadDoubleArrayCsv(string filePath)
        {
            var result = new List<Double>();

            foreach (var line in File.ReadLines(filePath))
            {
                result.Add(Convert.ToDouble(line));
            }

            return result.ToArray();
        }

        /// <summary>
        /// converts a csv file to a double array
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns>double array</returns>
        public static double[] LoadDoubleArrayCSV(string filePath)
        {
            var line = File.ReadLines(filePath).First(); // erste Zeile mit allen Werten
            return line
                .Split(',')                              // Trenne an Kommas
                .Select(v => Convert.ToDouble(v.Trim(), CultureInfo.InvariantCulture))  // Umwandeln in double
                .ToArray();
        }

        /// <summary>
        /// saves pcm to an wav
        /// </summary>
        /// <param name="pcm"></param>
        /// <param name="outputPath"></param>
        /// <param name="sampleRate"></param>
        public static void SavePcmAsWav(short[] pcm, string outputPath, int sampleRate = 44100)
        {
            using (var memoryStream = new MemoryStream())
            {
                // WAV-Datei schreiben
                using (var writer = new WaveFileWriter(outputPath, new WaveFormat(sampleRate, 16, 1)))
                {
                    byte[] byteBuffer = new byte[pcm.Length * sizeof(short)];
                    Buffer.BlockCopy(pcm, 0, byteBuffer, 0, byteBuffer.Length);
                    writer.Write(byteBuffer, 0, byteBuffer.Length);
                }
            }
        }
    }
}

