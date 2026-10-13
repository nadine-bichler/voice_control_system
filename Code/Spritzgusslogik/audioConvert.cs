using System.Globalization;
using System.IO;
using NAudio.Wave;

namespace Spritzgusslogik
{
    /// <summary>
    ///Provides methods for converting audio input to and from file formats such as WAV and CSV.around 
    /// </summary>
    public class audioConvert
    {
        /// <summary>
        /// Converts a CSV file with one feature vector per line into a jagged double array.
        /// Used for MFCC feature sequences (one frame per row).
        /// </summary>
        /// <param name="filePath">Path to the CSV file.</param>
        /// <returns>A jagged array (rows x columns), or null if the file is not found. Returns an empty array on parse errors.</returns>
        public static double[][]? LoadDoubleArray2DCsv(string? filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
                return null;

            try
            {
                var rows = new List<double[]>();
                foreach (var line in File.ReadLines(filePath))
                {
                    if (string.IsNullOrWhiteSpace(line))
                        continue;

                    var parts = line.Split(',');
                    var values = new double[parts.Length];
                    for (int i = 0; i < parts.Length; i++)
                    {
                        if (!double.TryParse(parts[i].Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out values[i]))
                            return Array.Empty<double[]>();
                    }
                    rows.Add(values);
                }
                return rows.ToArray();
            }
            catch
            {
                return Array.Empty<double[]>();
            }
        }

        /// <summary>
        /// Saves a PCM short[] array as a .wav file.
        /// </summary>
        /// <param name="pcm">PCM audio data as short array.</param>
        /// <param name="outputPath">Destination file path for WAV file</param>
        /// <param name="sampleRate">Sampling rate in Hz (default: 44100)</param>
        public static void SavePcmAsWav(short[] pcm, string outputPath, int sampleRate = 44100)
        {
            using var writer = new WaveFileWriter(outputPath, new WaveFormat(sampleRate, 16, 1));
            byte[] byteBuffer = new byte[pcm.Length * sizeof(short)];
            Buffer.BlockCopy(pcm, 0, byteBuffer, 0, byteBuffer.Length);
            writer.Write(byteBuffer, 0, byteBuffer.Length);
        }
    }
}
