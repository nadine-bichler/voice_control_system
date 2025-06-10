using NAudio.Wave;
using Spritzgusslogik;
using SampleToFile;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using Complex = System.Numerics.Complex;


SignalTransformer transformer = new SignalTransformer();

//brauchen wir nicht mehr dank Tests
var array = audioConvert.LoadDoubleArrayCSV("../../../WAV_samples/3.csv");
var array2 = audioConvert.LoadDoubleArrayCSV("../../../WAV_samples/Test3.csv");
Console.WriteLine(array.Length);
//Console.WriteLine(array);



var result = Identify.compare(array, array2);

Console.WriteLine(result);


var files = Directory.GetFiles("../../../samples", "*.mp3");

foreach (var file in files)
{

    var pcm = audioConvert.ConvertMp3ToInt16Array(file);

    //audioConvert.SaveInt16ArrayAsWav(pcm,"../../../wavTest/test.wav");

    var fft = transformer.Transform(pcm);



    //audioConvert.SaveComplexArrayCsv(Path.ChangeExtension(file,".csv"),fft);
    audioConvert.SaveDoubleArrayCsv(Path.ChangeExtension(file, ".csv"),fft);

}


//run python code
// Absoluter Pfad basierend auf dem aktuellen Verzeichnis
string projectDir = Directory.GetParent(AppContext.BaseDirectory)!.Parent!.Parent!.Parent!.FullName;            //geht 3 Ebenen hoch zur Projektwurzel
string scriptPath = Path.Combine(projectDir, "mfcss.py");


var psi = new ProcessStartInfo
{
    FileName = "python",  // oder vollständiger Pfad zur python.exe
    Arguments = $"\"{scriptPath}\"",
    UseShellExecute = false,
    RedirectStandardOutput = true,
    RedirectStandardError = true,
    CreateNoWindow = true
};

using (var process = Process.Start(psi))
{
    string output = process.StandardOutput.ReadToEnd();
    string errors = process.StandardError.ReadToEnd();
    process.WaitForExit();

    Console.WriteLine("Python-Ausgabe:\n" + output);
    if (!string.IsNullOrWhiteSpace(errors))
    {
        Console.WriteLine("Fehler:\n" + errors);
    }
}


namespace SampleToFile
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
    }
}