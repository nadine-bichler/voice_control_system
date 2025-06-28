using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Spritzgusslogik;
using NAudio.Wave;
using System.Reflection;
using System.Diagnostics;
using Accord.Diagnostics;
using Debug = System.Diagnostics.Debug;

namespace Tests
{
    [TestClass]
    public class Komponententest_1
    {
        /// <summary>
        /// This method tests Module Microphone. It simulates an random byte Array as Audio.
        /// And then checks if Microphone Module generates the right Int16[] values.
        /// </summary>
        [TestMethod]
        public void TestModuleMicrophone()
        {
            byte[] buffer = new byte[] { 0, 54, 145, 230, 170, 124, 34, 0 };

            Microphone mic = new Microphone();
            mic.Start();

            WaveInEventArgs eTest = new WaveInEventArgs(buffer, buffer.Length);

            Int16[] shortBuffer = new short[buffer.Length / 2];
            for (int i = 0; i < shortBuffer.Length; i++)
            {
                shortBuffer[i] = BitConverter.ToInt16(buffer, i * 2);
            }


            MethodInfo method = typeof(Microphone).GetMethod("WaveIn_DataAvailable", BindingFlags.NonPublic | BindingFlags.Instance);
            method.Invoke(mic, new object[] { mic, eTest });
            mic.Stop();

            var result = mic.GetValues();
            Assert.IsFalse(0 == result.Length, "Result länge ist  0");
            Assert.IsNotNull(result, "Result ist Null");
            Assert.AreEqual(shortBuffer[0], result[0], "Result und Buffer erste elemente sind nicht gleich");
            Assert.AreEqual(shortBuffer[1], result[1], "Result und Buffer sind nicht gleich Pos 1");
            Assert.AreEqual(shortBuffer[2], result[2], "Result und Buffer  sind nicht gleich Pos 2 ");
            Assert.AreEqual(shortBuffer[3], result[3], "Result und Buffer  sind nicht gleich Pos 3 ");



        }



        /// <summary>
        /// This method tests Module Microphone  and InputToMfcc and Identify together. It simulates an random byte Array as Audio.
        /// And then checks if Microphone Module generates the right Int16[] values. Then Converts those Int16[] Values to MFCC-Vectors of Length 13.
        /// And then identiies the 
        /// </summary>
        [TestMethod]
        public void TestMicrophone_MFCC_Identify()
        {

            //Microphone
            byte[] buffer = new byte[2500];

            for (int i = 0; i < buffer.Length; i++)
            {
                buffer[i] = (byte)((i * i) - i*10 + 100);
            }

            Microphone mic = new Microphone();
            mic.Start();

            WaveInEventArgs eTest = new WaveInEventArgs(buffer, buffer.Length);



            MethodInfo method = typeof(Microphone).GetMethod("WaveIn_DataAvailable", BindingFlags.NonPublic | BindingFlags.Instance);
            method.Invoke(mic, new object[] { mic, eTest });
            mic.Stop();

            var result = mic.GetValues();

           






            audioConvert.SavePcmAsWav(result, "input_temp1.wav");

            float[] floatInput = result.Select(x => x / 32768f).ToArray();


            string pythonScriptPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "Prototyp", "InputToMfcc.py");
            pythonScriptPath = Path.GetFullPath(pythonScriptPath); // ergibt z. B. .../Prototyp/InputToMfcc.py


            //Starte Python Skript
            var psi = new ProcessStartInfo
            {
                FileName = "python",
                Arguments = $"\"{pythonScriptPath}\"",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardError = true,
                WorkingDirectory = AppDomain.CurrentDomain.BaseDirectory  // Verzeichnis von .exe
            };

            using (var proc = Process.Start(psi))
            {
                Assert.IsNotNull(proc, "Python-Prozess konnte nicht gestartet werden");

                // Warte und lese Ausgaben
                string stdout = proc.StandardOutput.ReadToEnd();
                string stderr = proc.StandardError.ReadToEnd();
                proc.WaitForExit();

                // Ausgabe im Test-Log
                Debug.WriteLine("=== Python stdout ===");
                Debug.WriteLine(stdout);
                Debug.WriteLine("=== Python stderr ===");
                Debug.WriteLine(stderr);

                //  Abbruch wenn ExitCode != 0
                Assert.AreEqual(0, proc.ExitCode,
                    $"Python-Skript ist mit Exit-Code {proc.ExitCode} beendet.{Environment.NewLine}Stderr:{Environment.NewLine}{stderr}");
            }

            //lade ergebnis aus csv datei
            double[] mfcc = audioConvert.LoadDoubleArrayCsv("mfcc_vector.csv");

            int cmd = Identify.Recognize(mfcc, out double sim);
            Assert.IsTrue(mfcc.Length == 26, "Mfcc Länge: " + mfcc.Length);
            Assert.IsTrue(sim >= 0.7, " Similarityi ist nicht größer als 0.7. Similarity: " + sim);
            Assert.IsTrue(cmd == 7, "Befehl 7 wurde nicht erkannt! Stattdessen wurde " + cmd + " erkannt.");

        }

        /// <summary>
        /// This Test Method tests the function of Startegy-Pattern in FFT-Transformer.
        /// It simulates a random sinus signal and it sets the right algorithm and then checks if 
        /// it worked.
        /// </summary>
        [TestMethod]
        public void TestStrategyPatternFFT()
        {
            Int16[] signal = new Int16[] { 0, 100, -100, 0 };

            FFTTransformer fFTTransformer = new FFTTransformer();
            IFFTTransformer fftT1 = new SignalTransformer();
            fFTTransformer.setAlgorithm(fftT1);
            var res = fFTTransformer.Transform(signal);
            Assert.IsTrue(res.Length > 0, " Die Array-Länge von Transformierte Signal ist 0 ");
            Assert.AreEqual("SignalTransformer", fFTTransformer.TransformerAlgo.Name);
        }


        /// <summary>
        /// This Test Method tests the function of Startegy-Pattern in FFT-Transformer.
        /// It simulates a random sinus signal and it sets the right algorithm and then checks if 
        /// it worked.
        /// </summary>
        [TestMethod]
        public void TestStrategyPatternFFT2()
        {
            Int16[] signal = new Int16[] { 0, 100, -100, 0 };

            FFTTransformer fFTTransformer = new FFTTransformer();
            IFFTTransformer fftT1 = new SignalFFTSharpTransformer();
            fFTTransformer.setAlgorithm(fftT1);
            var res = fFTTransformer.Transform(signal);
            Assert.IsTrue(res.Length > 0, " Die Array-Länge von Transformierte Signal ist 0 ");
            Assert.AreEqual("SignalFFTSharpTransformer", fFTTransformer.TransformerAlgo.Name);
        }

        /// <summary>
        /// This Test Method tests the function converting an short Array to MFCC-Vectors.
        /// InputToMfcc will take a random sinus Array and convert it to MFCC-Vectors.
        /// It checks if the Length is 26 and if converted array is not null.
        /// </summary>
        [TestMethod]
        public void TestMFCCConvert()
        {


            Int16[] pcm = new Int16[] { 0, 100, -100, 0 };

            audioConvert.SavePcmAsWav(pcm, "input_temp2.wav");

            float[] floatInput = pcm.Select(x => x / 32768f).ToArray();


            string pythonScriptPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "Prototyp", "InputToMfcc.py");
            pythonScriptPath = Path.GetFullPath(pythonScriptPath); // ergibt z. B. .../Prototyp/InputToMfcc.py


            //Starte Python Skript
            var psi = new ProcessStartInfo
            {
                FileName = "python",
                Arguments = $"\"{pythonScriptPath}\"",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardError = true,
                WorkingDirectory = AppDomain.CurrentDomain.BaseDirectory  // Verzeichnis von .exe
            };

            using (var proc = Process.Start(psi))
            {
                Assert.IsNotNull(proc, "Python-Prozess konnte nicht gestartet werden");

                // Warte und lese Ausgaben
                string stdout = proc.StandardOutput.ReadToEnd();
                string stderr = proc.StandardError.ReadToEnd();
                proc.WaitForExit();

                // Ausgabe im Test-Log
                Debug.WriteLine("=== Python stdout ===");
                Debug.WriteLine(stdout);
                Debug.WriteLine("=== Python stderr ===");
                Debug.WriteLine(stderr);

                //  Abbruch wenn ExitCode != 0
                Assert.AreEqual(0, proc.ExitCode,
                    $"Python-Skript ist mit Exit-Code {proc.ExitCode} beendet.{Environment.NewLine}Stderr:{Environment.NewLine}{stderr}");
            }

            //lade ergebnis aus csv datei
            double[] mfcc = audioConvert.LoadDoubleArrayCsv("mfcc_vector.csv");

            Assert.IsFalse(mfcc == null);
            Assert.IsTrue(mfcc.Length == 26, "Die mfcc ergbnis Länge ist nicht 26. MFCC-Array Länge: " + mfcc.Length);

        }

        /// <summary>
        /// This Test Method tests the function of Startegy-Pattern in FFT-Transformer.
        /// Transform Algorithm is null here.
        /// It checks if the Method throws an Exception.
        /// </summary>
        [TestMethod]
        public void TestStrattegyPatternFFTNull()
        {
            Int16[] signal = new Int16[] { 0, 100, -100, 0 };

            FFTTransformer fFTTransformer = new FFTTransformer();


            Assert.ThrowsExactly<ArgumentNullException>(() =>
            {
                fFTTransformer.setAlgorithm(null);
                var res = fFTTransformer.Transform(signal);
            });
        }

        /// <summary>
        /// This Test Method tests the Microphone and InputToMfcc together.
        /// Python returns an error here because script can not be found.
        /// </summary>
        [TestMethod]
        public void TestMicrophone_MFCCScriptNotFound()
        {
            //Microphone
            byte[] buffer = new byte[2000];

            for (int i = 0; i < buffer.Length; i++)
            {
                buffer[i] = (byte)(1);
            }

            Microphone mic = new Microphone();
            mic.Start();

            WaveInEventArgs eTest = new WaveInEventArgs(buffer, buffer.Length);


            //übergebe die daten an mikrofon
            MethodInfo method = typeof(Microphone).GetMethod("WaveIn_DataAvailable", BindingFlags.NonPublic | BindingFlags.Instance);
            method.Invoke(mic, new object[] { mic, eTest });
            mic.Stop();

            var result = mic.GetValues();

            






            audioConvert.SavePcmAsWav(result, "input_temp3.wav");

            float[] floatInput = result.Select(x => x / 32768f).ToArray();


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
                RedirectStandardError = true,
                WorkingDirectory = AppDomain.CurrentDomain.BaseDirectory  // Verzeichnis von .exe
            };

            using (var proc = Process.Start(psi))
            {
                Assert.IsNotNull(proc, "Python-Prozess konnte nicht gestartet werden");

                // Warte und lese Ausgaben
                string stdout = proc.StandardOutput.ReadToEnd();
                string stderr = proc.StandardError.ReadToEnd();
                proc.WaitForExit();

                // Ausgabe im Test-Log
                Debug.WriteLine("=== Python stdout ===");
                Debug.WriteLine(stdout);
                Debug.WriteLine("=== Python stderr ===");
                Debug.WriteLine(stderr);

              //es ist nicht null weil script konnte nicht gefunden werden
                Assert.AreNotEqual(0, proc.ExitCode,
                    $"Python-Skript ist mit Exit-Code {proc.ExitCode} beendet.{Environment.NewLine}Stderr:{Environment.NewLine}{stderr}");
            }

         
           

        }
    }
}
