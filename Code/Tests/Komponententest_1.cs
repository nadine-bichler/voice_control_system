using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Spritzgusslogik;
using NAudio.Wave;
using System.Reflection;

namespace Tests
{
    /// <summary>
    /// Komponententest für das <c>Microphone</c>-Modul.
    /// Testet, ob Audiodaten korrekt verarbeitet und interpretiert werden.
    /// </summary>
    [TestClass]
    public class Komponententest_1
    {
        /// <summary>
        /// Testet die Methode <c>WaveIn_DataAvailable</c> indirekt über Reflection,
        /// indem simulierte PCM-Daten übergeben werden.
        /// 
        /// Geprüft wird, ob:
        /// <list type="bullet">
        /// <item><description>PCM-Daten korrekt konvertiert werden</description></item>
        /// <item><description>Das Mikrofon die Daten übernimmt</description></item>
        /// <item><description>Die Längen und Inhalte korrekt zurückgegeben werden</description></item>
        /// </list>
        /// </summary>
        [TestMethod]
        public void TestModuleMicrophone()
        {
            // Beispielhafte PCM-Daten (8-Bit Darstellung, wird in 16-Bit konvertiert)
            byte[] buffer = new byte[] { 0, 54, 145, 230, 170, 124, 34, 0 };

            Microphone mic = new Microphone();
            mic.Start();

            // Simuliertes Ereignis mit Testdaten
            WaveInEventArgs eTest = new WaveInEventArgs(buffer, buffer.Length);

            // Erwartete konvertierte PCM-Daten (16-Bit)
            Int16[] shortBuffer = new short[buffer.Length / 2];
            for (int i = 0; i < shortBuffer.Length; i++)
            {
                shortBuffer[i] = BitConverter.ToInt16(buffer, i * 2);
            }

            // Zugriff auf private Methode per Reflection (WaveIn_DataAvailable)
            MethodInfo method = typeof(Microphone).GetMethod("WaveIn_DataAvailable", BindingFlags.NonPublic | BindingFlags.Instance);
            method.Invoke(mic, new object[] { mic, eTest });

            mic.Stop();

            // Auslesen der verarbeiteten Werte
            var result = mic.GetValues();

            // Prüfungen
            Assert.IsFalse(result.Length == 0, "Result Length is 0");
            Assert.IsNotNull(result, "Result is Null");
            Assert.AreEqual(shortBuffer[0], result[0], "Result and Buffer first element are not equal");
            Assert.AreEqual(shortBuffer[1], result[1], "Result and Buffer are not equal at position 1");
            Assert.AreEqual(shortBuffer[2], result[2], "Result and Buffer are not equal at position 2");
            Assert.AreEqual(shortBuffer[3], result[3], "Result and Buffer are not equal at position 3");
        }
    }
}
