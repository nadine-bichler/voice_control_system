using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Spritzgusslogik;
using NAudio.Wave;
using System.Reflection;
using System.Diagnostics;

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
            byte[] buffer = new byte[] { 0,54, 145, 230, 170, 124,34, 0 };

            Microphone mic = new Microphone();
            mic.Start();

            WaveInEventArgs eTest = new WaveInEventArgs(buffer, buffer.Length);

            Int16[] shortBuffer = new short[buffer.Length/2];
            for(int i = 0; i < shortBuffer.Length; i++)
            {
                shortBuffer[i] = BitConverter.ToInt16(buffer, i*2);
            }

           
            MethodInfo method = typeof(Microphone).GetMethod("WaveIn_DataAvailable", BindingFlags.NonPublic | BindingFlags.Instance);
            method.Invoke( mic, new object[] { mic, eTest });
            mic.Stop();

            var result = mic.GetValues();
            Assert.IsFalse(0 == result.Length, "Result Length is 0");
            Assert.IsNotNull(result,"Result is Null");
            Assert.AreEqual(shortBuffer[0], result[0],"Result and Buffer first elemment are not Equal");
            Assert.AreEqual(shortBuffer[1], result[1], "Result and Buffer are not Equal Pos 1");
            Assert.AreEqual(shortBuffer[2], result[2], "Result and Buffer are not Equal Pos 2 ");
            Assert.AreEqual(shortBuffer[3], result[3], "Result and Buffer are not Equal Pos 3 ");



        }

       /*  [TestMethod]
         public void TestMicrophone_FFT_Identify() 
         {

             //Microphone
             byte[] buffer = new byte[] { 0, 54, 145, 230, 170, 124, 34, 0 };

             Microphone mic = new Microphone();
             mic.Start();

             WaveInEventArgs eTest = new WaveInEventArgs(buffer, buffer.Length);



             MethodInfo method = typeof(Microphone).GetMethod("WaveIn_DataAvailable", BindingFlags.NonPublic | BindingFlags.Instance);
             method.Invoke(mic, new object[] { mic, eTest });
             mic.Stop();

             var result = mic.GetValues();

           


            


            audioConvert.SavePcmAsWav(result, "kt_1_2.wav");

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
                WorkingDirectory = AppDomain.CurrentDomain.BaseDirectory  // Verzeichnis von .exe
            };



            //lade ergebnis aus csv datei
            double[] mfcc = audioConvert.LoadDoubleArrayCsv("mfcc_vector.csv");

            int cmd = Identify.Recognize(mfcc, out double sim);
            Assert.IsTrue(sim <= 0.003, "Command: "+cmd);

        }*/

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
            Assert.IsTrue(res.Length > 0," Die Array-Länge von Transformierte Signal ist 0 ");
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
    }
}
