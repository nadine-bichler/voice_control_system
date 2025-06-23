using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using Common;
using Spritzgusslogik;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Prototyp
{
    internal class SignalIdentifier
    {
        private readonly SignalFFTSharpTransformer transformer = new SignalFFTSharpTransformer();
        private readonly double threshold = 0.70;//0.80;
        private Identify2 identify2 = new Identify2();
        //Event declaration
        public event EventHandler<DigitRecognizedEventArgs>? DigitRecognized;


        public void Recognition(Int16[] pcm)
        {
            if (pcm == null || pcm.Length == 0)
                return;

            
            float[] floatInput = pcm.Select(x => x / 32768f).ToArray();

            // Debug WAV speichern
            short[] debugPcm = floatInput.Select(x => (short)(x * 32767)).ToArray();
            audioConvert.SavePcmAsWav(debugPcm, "input_temp.wav");

            //schreibe input
            //File.WriteAllText("input_signal.csv", string.Join(",", floatInput));

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
            // double[] fft = transformer.Transform(pcm);
            //int result = identify2.Recognize(mfcc, out double sim);

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
