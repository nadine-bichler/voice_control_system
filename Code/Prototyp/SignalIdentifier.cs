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
        private readonly BlockingCollection<short[]> pcmQueue = new();
        private readonly Thread recognitionThread;
        private readonly SignalFFTSharpTransformer transformer = new();
        private readonly double threshold = 0.80;
        private readonly TimeSpan recognitionCooldown = TimeSpan.FromMilliseconds(500);

        private DateTime lastRecognitionTime = DateTime.MinValue;

        private volatile bool isRunning = true;

        public event EventHandler<DigitRecognizedEventArgs>? DigitRecognized;

        public SignalIdentifier()
        {
            recognitionThread = new Thread(ProcessQueue)
            {
                IsBackground = true
            };
            recognitionThread.Start();
        }

        public void Enqueue(short[] pcm)
        {
            if (!isRunning || pcm == null || pcm.Length == 0) return;

            // optional: Begrenze Queue-Länge, um Speicher zu sparen
            if (pcmQueue.Count < 100)
                pcmQueue.Add(pcm);
        }

        public void Stop()
        {
            isRunning = false;
            pcmQueue.CompleteAdding();
        }

        private void ProcessQueue()
        {
            while (isRunning)
            {
                try
                {
                    short[] pcm = pcmQueue.Take(); // Blockiert bei leerer Queue

                    if ((DateTime.Now - lastRecognitionTime) < recognitionCooldown)
                        continue;

                    lastRecognitionTime = DateTime.Now;

                    RunRecognition(pcm);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("Fehler im Recognition-Thread: " + ex.Message);
                }
            }
        }

        private void RunRecognition(short[] pcm)
        {
            float[] floatInput = pcm.Select(x => x / 32768f).ToArray();

            if (audioConvert.IsSilent(floatInput))
            {
                System.Diagnostics.Debug.WriteLine("Stille erkannt");
                return;
            }

            if (floatInput.Length < 200)
            {
                System.Diagnostics.Debug.WriteLine("Signal zu kurz");
                return;
            }

            // Schreibe CSV für Python
            string inputPath = "input_signal.csv";
            File.WriteAllText(inputPath, string.Join(",", floatInput));

            string pythonScriptPath = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "InputToMfcc.py"));

            var psi = new ProcessStartInfo
            {
                FileName = "python",
                Arguments = $"\"{pythonScriptPath}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                WorkingDirectory = AppDomain.CurrentDomain.BaseDirectory
            };

            using (var pythProcess = new Process())
            {
                pythProcess.StartInfo = psi;
                pythProcess.OutputDataReceived += (s, e) => System.Diagnostics.Debug.WriteLine("PYTHON OUT: " + e.Data);
                pythProcess.ErrorDataReceived += (s, e) => System.Diagnostics.Debug.WriteLine("PYTHON ERR: " + e.Data);

                pythProcess.Start();
                pythProcess.BeginOutputReadLine();
                pythProcess.BeginErrorReadLine();

                pythProcess.WaitForExit(5000); // max. 5 Sek. warten
                if (!pythProcess.HasExited)
                {
                    System.Diagnostics.Debug.WriteLine("Python-Skript hängt! Beende Prozess.");
                    pythProcess.Kill();
                }
            }

            if (!File.Exists("mfcc_vector.csv"))
            {
                System.Diagnostics.Debug.WriteLine("MFCC-Datei nicht gefunden.");
                return;
            }

            double[] mfcc = audioConvert.LoadDoubleArrayCSV("mfcc_vector.csv");

            System.Diagnostics.Debug.WriteLine("MFCC-Länge: " + mfcc.Length);
            System.Diagnostics.Debug.WriteLine("MFCC-Daten: " + string.Join(", ", mfcc.Take(10)) + "...");

            int result = Identify.Recognize(mfcc, out double sim);

            if (sim >= threshold)
            {
                DigitRecognized?.Invoke(this, new DigitRecognizedEventArgs(result, sim));
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("Keine gültige Ziffer erkannt.");
            }
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
