using System;
using System.IO;
using NAudio.Wave;
using Spritzgusslogik;
using Complex = System.Numerics.Complex;
/*
SignalTransformer transformer = new SignalTransformer();



var files = Directory.GetFiles("../../../samples", "*.mp3");

foreach (var file in files)
{

    var pcm = audioConvert.ConvertMp3ToInt16Array(file);

    //audioConvert.SaveInt16ArrayAsWav(pcm,"../../../wavTest/test.wav");

    var fft = transformer.Transform(pcm);



    audioConvert.SaveComplexArrayCsv(Path.ChangeExtension(file,".csv"),fft);

}





class audioConvert
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

    public static void SaveComplexArrayCsv(string filePath, Complex[] data)
    {
        using (var writer = new StreamWriter(filePath))
        {
            foreach (var c in data)
            {
                writer.WriteLine($"{c.Real},{c.Imaginary}");
            }
        }
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
}*/