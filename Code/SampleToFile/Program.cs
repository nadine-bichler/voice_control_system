///
/// @file    Program
/// @brief   Converts audio signals into csv as samples
/// @author  Anton Armin, Bichler Nadine
/// @date    24.06.2025
///

using Spritzgusslogik;
using Common;
using System;
using System.Diagnostics;
using NAudio.Wave;
using System.Globalization;
using MathNet.Numerics.LinearRegression;



// Diese Klasse ist für die Verarbeitung der Samples zu Files zuständig



SignalTransformer transformer = new SignalTransformer();


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

    System.Diagnostics.Debug.WriteLine("Python-Ausgabe:\n" + output);
    if (!string.IsNullOrWhiteSpace(errors))
    {
        System.Diagnostics.Debug.WriteLine("Fehler:\n" + errors);
    }
}

//brauchen wir nicht mehr dank Tests
var array = audioConvert.LoadDoubleArrayCSV("../../../WAV_samples/3.csv");
var array2 = audioConvert.LoadDoubleArrayCSV("../../../WAV_samples/Test3.csv");
Console.WriteLine(array.Length);
Console.WriteLine(array2.Length);



var result = Identify.Compare(array, array2);

Console.WriteLine(result);