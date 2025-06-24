using Spritzgusslogik;
using System;
using System.Diagnostics;
using static System.Resources.ResXFileRef;

/// <summary>
/// @deprecated Dieser Block wurde durch das Python-Skript ersetzt.
/// Verwende stattdessen den automatisierten MFCC-Prozess.
/// </summary>
/*
[Obsolete("Deprecated: Manuelle Sample-Verarbeitung, ersetzt durch Python-Skript mfcss.py")]
// Dieses Programm ist für die Verarbeitung der Samples zu Files zuständig
SignalTransformer transformer = new SignalTransformer();

var files = Directory.GetFiles("../../../samples", "*.mp3");

foreach (var file in files)
{
    var pcm = audioConvert.ConvertMp3ToInt16Array(file);

    //audioConvert.SaveInt16ArrayAsWav(pcm,"../../../wavTest/test.wav");

    var fft = transformer.Transform(pcm);

    //audioConvert.SaveComplexArrayCsv(Path.ChangeExtension(file,".csv"),fft);
    audioConvert.SaveDoubleArrayCsv(Path.ChangeExtension(file, ".csv"), fft);
}
*/


/// <summary>
///Converts audio signals into csv as samples
/// </summary>
// Run Python code
// Absoluter Pfad basierend auf dem aktuellen Verzeichnis
string projectDir = Directory.GetParent(AppContext.BaseDirectory)!.Parent!.Parent!.Parent!.FullName; // geht 3 Ebenen hoch zur Projektwurzel
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