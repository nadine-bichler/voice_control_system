using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Spritzgusslogik;

namespace Prototyp
{
    /// <summary>
    /// Identifies spoken digits based on audio input.
    /// Converts PCM audio input to MFCC features using a Python script and compares with stored digit profiles.
    /// </summary>
    internal class SignalIdentifier
    {
        private static readonly double threshold = 0.60;   //threshold for signal similarity

        /// <summary>
        /// event triggered we´hen a digit is recognized
        /// </summary>
        public event EventHandler<DigitRecognizedEventArgs>? DigitRecognized;

        /// <summary>
        /// Converts PCM to WAV, runs MFCC extraction via Python, loads the resulting vector, and performs recognition.
        /// </summary>
        /// <param name="pcm">pcm audio signal as Int16 array</param>
        public void Recognition(Int16[] pcm)
        {
            if (pcm == null || pcm.Length == 0)
                return;

            audioConvert.SavePcmAsWav(pcm, "input_temp.wav");

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
            double[] mfcc = audioConvert.LoadDoubleArrayCsv("mfcc_vector.csv");

            int result = Identify.Recognize(mfcc, out double sim);

            if (sim > threshold)
                DigitRecognized?.Invoke(this, new DigitRecognizedEventArgs(result, sim));
        }
    }

    /// <summary>
    /// Event arguments for recognized digit results.
    /// </summary>
    internal class DigitRecognizedEventArgs : EventArgs
    {
        public int Digit { get; }
        public double Confidence { get; }

        /// <summary>
        /// Constructs a new DigitRecognizedEventArgs object.
        /// </summary>
        /// <param name="digit">Recognized digit from the input signal.</param>
        /// <param name="confidence">confidence Similarity score for the recognized digit.
        ///</param>
        public DigitRecognizedEventArgs(int digit, double confidence)
        {
            Digit = digit;
            Confidence = confidence;
        }
    }
}
