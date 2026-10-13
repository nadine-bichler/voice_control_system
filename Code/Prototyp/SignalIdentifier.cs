using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Spritzgusslogik;

namespace Prototyp
{
    /// <summary>
    /// Identifies spoken digits based on audio input.
    /// Converts PCM audio input to MFCC feature sequences using a persistent Python worker
    /// and compares them with stored digit profiles.
    /// </summary>
    internal class SignalIdentifier
    {
        private static readonly double threshold = 0.0; //threshold for the (scaled) margin confidence

        private Process? _pythonServer;
        private readonly object _pythonLock = new();

        /// <summary>
        /// Creates the identifier and makes sure the Python worker is stopped when the app exits.
        /// </summary>
        public SignalIdentifier()
        {
            AppDomain.CurrentDomain.ProcessExit += (object? s, EventArgs e) => Shutdown();
        }

        /// <summary>
        /// event triggered we´hen a digit is recognized
        /// </summary>
        public event EventHandler<DigitRecognizedEventArgs>? DigitRecognized;

        /// <summary>
        /// Sends the recorded PCM directly to the Python worker, receives the feature sequence
        /// in memory and performs the recognition. No temporary files are used.
        /// </summary>
        /// <param name="pcm">pcm audio signal as Int16 array</param>
        public void Recognition(Int16[] pcm)
        {
            if (pcm == null || pcm.Length == 0)
                return;

            if (pcm.Length < 200)  // zu kurzes Signal
            {
                Debug.WriteLine("Zu kurzes Signal");
                return;
            }

            // Projektverzeichnis mit dem Python-Skript. Funktioniert auch unter `dotnet run`,
            // unabhaengig vom aktuellen Arbeitsverzeichnis des Prozesses.
            string projectDir = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", ".."));
            string pythonScriptPath = Path.Combine(projectDir, "InputToMfcc.py");

            if (!EnsurePythonServer(projectDir, pythonScriptPath))
                return;

            if (!RequestFeaturesPcm(pcm, Microphone.SampleRate, out double[][] mfcc) || mfcc.Length == 0)
            {
                Debug.WriteLine("Python-Worker lieferte keine Feature-Sequenz.");
                return;
            }

            int result = Identify.RecognizeSequence(mfcc, out double sim);

            if (result > 0 && sim >= threshold)
                DigitRecognized?.Invoke(this, new DigitRecognizedEventArgs(result, sim));
        }

        /// <summary>
        /// Starts the persistent Python worker (which loads librosa/numba once) if needed.
        /// </summary>
        /// <returns><c>true</c> if the worker is running and ready.</returns>
        private bool EnsurePythonServer(string projectDir, string pythonScriptPath)
        {
            lock (_pythonLock)
            {
                if (_pythonServer != null && !_pythonServer.HasExited)
                    return true;

                string pythonExe = FindPythonExe() ?? "python";
                var psi = new ProcessStartInfo
                {
                    FileName = pythonExe,
                    Arguments = $"\"{pythonScriptPath}\" --server",
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    WorkingDirectory = projectDir
                };

                try
                {
                    var process = Process.Start(psi);
                    if (process == null)
                        return false;

                    // stderr asynchron weiterleiten, damit der Puffer nicht volllaeuft.
                    var stderr = process.StandardError;
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            string? line;
                            while ((line = await stderr.ReadLineAsync()) != null)
                                Debug.WriteLine("PYTHON: " + line);
                        }
                        catch { /* Worker beendet */ }
                    });

                    // Auf das einmalige "READY" warten (Import von librosa dauert ein paar Sekunden).
                    var stdout = process.StandardOutput.BaseStream;
                    string? ready = ReadLine(stdout);
                    if (ready == null)
                    {
                        process.Dispose();
                        return false;
                    }

                    _pythonServer = process;
                    return true;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Python-Worker konnte nicht gestartet werden (" + pythonExe + "): " + ex.Message);
                    return false;
                }
            }
        }

        /// <summary>
        /// Sends one PCM job to the running Python worker and reads the returned feature sequence.
        /// </summary>
        /// <returns><c>true</c> if the worker returned features.</returns>
        private bool RequestFeaturesPcm(short[] pcm, int sampleRate, out double[][] features)
        {
            features = Array.Empty<double[]>();

            lock (_pythonLock)
            {
                if (_pythonServer == null || _pythonServer.HasExited)
                    return false;

                try
                {
                    var stdin = _pythonServer.StandardInput.BaseStream;
                    var stdout = _pythonServer.StandardOutput.BaseStream;

                    WriteAscii(stdin, $"PCM\t{sampleRate}\t{pcm.Length}");

                    byte[] payload = new byte[pcm.Length * sizeof(short)];
                    Buffer.BlockCopy(pcm, 0, payload, 0, payload.Length);
                    stdin.Write(payload, 0, payload.Length);
                    stdin.Flush();

                    return TryReadFeatures(stdout, out features);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Python-Worker Fehler: " + ex.Message);
                    return false;
                }
            }
        }

        /// <summary>
        /// Sends one WAV -> CSV job to the running Python worker (used to build reference sequences).
        /// </summary>
        /// <returns><c>true</c> if the worker reported success.</returns>
        private bool RequestFeaturesFile(string wavPath, string csvPath)
        {
            lock (_pythonLock)
            {
                if (_pythonServer == null || _pythonServer.HasExited)
                    return false;

                try
                {
                    var stdin = _pythonServer.StandardInput.BaseStream;
                    var stdout = _pythonServer.StandardOutput.BaseStream;

                    WriteAscii(stdin, $"FILE\t{wavPath}\t{csvPath}");
                    stdin.Flush();

                    string? response = ReadLine(stdout);
                    if (response != null && response.StartsWith("OK", StringComparison.Ordinal))
                        return true;

                    Debug.WriteLine("PYTHON-WORKER: " + (response ?? "<keine Antwort>"));
                    return false;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Python-Worker Fehler: " + ex.Message);
                    return false;
                }
            }
        }

        /// <summary>
        /// Reads the "OK rows cols" answer followed by the feature rows from the worker.
        /// </summary>
        private static bool TryReadFeatures(Stream stdout, out double[][] features)
        {
            features = Array.Empty<double[]>();

            string? header = ReadLine(stdout);
            if (header == null)
                return false;

            var parts = header.Split(' ');
            if (parts.Length < 3 || parts[0] != "OK")
            {
                Debug.WriteLine("PYTHON-WORKER: " + header);
                return false;
            }

            int rows = int.Parse(parts[1], CultureInfo.InvariantCulture);
            var result = new double[rows][];

            for (int r = 0; r < rows; r++)
            {
                string? line = ReadLine(stdout);
                if (line == null)
                    return false;

                var values = line.Split(',');
                var row = new double[values.Length];
                for (int c = 0; c < values.Length; c++)
                    row[c] = double.Parse(values[c], CultureInfo.InvariantCulture);
                result[r] = row;
            }

            features = result;
            return true;
        }

        /// <summary>
        /// Writes a single ASCII line to the given stream (without flushing).
        /// </summary>
        private static void WriteAscii(Stream stream, string text)
        {
            byte[] bytes = Encoding.ASCII.GetBytes(text + "\n");
            stream.Write(bytes, 0, bytes.Length);
        }

        /// <summary>
        /// Reads a single ASCII line from the given stream.
        /// </summary>
        private static string? ReadLine(Stream stream)
        {
            var sb = new StringBuilder();
            int b;
            while ((b = stream.ReadByte()) != -1)
            {
                if (b == '\n')
                    return sb.ToString();
                if (b != '\r')
                    sb.Append((char)b);
            }
            return sb.Length > 0 ? sb.ToString() : null;
        }

        /// <summary>
        /// Stops the Python worker if it is still running.
        /// </summary>
        public void Shutdown()
        {
            lock (_pythonLock)
            {
                try
                {
                    if (_pythonServer != null && !_pythonServer.HasExited)
                    {
                        var stdin = _pythonServer.StandardInput.BaseStream;
                        WriteAscii(stdin, "QUIT");
                        stdin.Flush();
                        if (!_pythonServer.WaitForExit(2000))
                            _pythonServer.Kill();
                    }
                }
                catch { /* ignore */ }
                finally
                {
                    _pythonServer = null;
                }
            }
        }

        /// <summary>
        /// Recomputes all reference feature sequences (WAV_samples -> WAV_sequences) using
        /// the Python worker. Used after new reference recordings were added.
        /// </summary>
        /// <returns>The number of successfully generated sequences.</returns>
        public int RebuildReferenceSequences()
        {
            string projectDir = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", ".."));
            string sourceDir = Path.GetFullPath(Path.Combine(projectDir, "..", "SampleToFile", "WAV_samples"));
            string targetDir = Path.GetFullPath(Path.Combine(projectDir, "..", "SampleToFile", "WAV_sequences"));

            if (!Directory.Exists(sourceDir))
                return 0;

            Directory.CreateDirectory(targetDir);

            if (!EnsurePythonServer(projectDir, Path.Combine(projectDir, "InputToMfcc.py")))
                return 0;

            int count = 0;
            foreach (var wav in Directory.GetFiles(sourceDir, "*.wav"))
            {
                string outCsv = Path.Combine(targetDir, Path.GetFileNameWithoutExtension(wav) + ".csv");
                if (RequestFeaturesFile(wav, outCsv))
                    count++;
            }

            return count;
        }

        /// <summary>
        /// Finds the real Python interpreter (e.g. a per-user Python installation),
        /// avoiding the Microsoft Store alias stub that does not execute scripts.
        /// </summary>
        /// <returns>Full path to python.exe, or <c>null</c> if none was found.</returns>
        private static string? FindPythonExe()
        {
            var roots = new[]
            {
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Programs", "Python"),
                @"C:\Program Files\Python",
            };

            foreach (var root in roots)
            {
                if (!Directory.Exists(root))
                    continue;

                var exe = Directory.GetFiles(root, "python.exe", SearchOption.AllDirectories)
                    .Where(p => !p.Contains("WindowsApps", StringComparison.OrdinalIgnoreCase))
                    .OrderByDescending(p => p)
                    .FirstOrDefault();

                if (exe != null)
                    return exe;
            }

            return null;
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
