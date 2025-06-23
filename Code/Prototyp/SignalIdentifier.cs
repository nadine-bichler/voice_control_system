using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Common;
using Spritzgusslogik;

namespace Prototyp
{
    internal class SignalIdentifier
    {
        private readonly SignalFFTSharpTransformer transformer = new SignalFFTSharpTransformer();
        private readonly double threshold = 0.80;

        //Event declaration
        public event EventHandler<DigitRecognizedEventArgs>? DigitRecognized;

        public void Recognition(Int16[] pcm)
        {
            if (pcm == null || pcm.Length == 0)
                return;

            float[] floatInput = pcm.Select(x => x / 32768f).ToArray();


            if (floatInput.Length < 200)  // z. B. zu kurz
            {
                System.Diagnostics.Debug.WriteLine("Zu kurzes Signal nach dem Trimmen");
                return;
            }

            string pythonScriptPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "InputToMfcc.py");
            pythonScriptPath = Path.GetFullPath(pythonScriptPath); // ergibt z. B. .../Prototyp/InputToMfcc.py


            //Starte Python Skript
            var psi = new ProcessStartInfo
            {
                FileName = "python",
                Arguments = $"\"{pythonScriptPath}\"",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                WorkingDirectory = AppDomain.CurrentDomain.BaseDirectory  // Verzeichnis von .exe
            };

            psi.RedirectStandardError = true;

            using (var pythProcess = Process.Start(psi))
            {
                string error = pythProcess.StandardError.ReadToEnd();
                pythProcess.WaitForExit();
                System.Diagnostics.Debug.WriteLine("PYTHON-FEHLER:\n" + error);  // <-- zeigt dir Probleme z. B. in NumPy, Librosa etc.
            }

            //lade ergebnis aus csv datei
            double[] mfcc = audioConvert.LoadDoubleArrayCSV("mfcc_vector.csv");

            int result = Identify.Recognize(mfcc, out double sim);

            if (sim >= threshold)
                DigitRecognized?.Invoke(this, new DigitRecognizedEventArgs(result, sim));
        }
    }

    internal class DigitRecognizedEventArgs : EventArgs
    {
        public int Digit { get; }
        public double Confidence { get; }

        public DigitRecognizedEventArgs(int digit, double confidence)
        {
            Digit = digit;
            Confidence = confidence;
        }
    }
}
