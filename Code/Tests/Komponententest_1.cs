using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Spritzgusslogik;
using NAudio.Wave;
using System.Reflection;

namespace Tests
{
    /// <summary>
    /// tests 
    /// </summary>
    [TestClass]
    public class Komponententest_1
    {

        /// <summary>
        /// 
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
    }
}
