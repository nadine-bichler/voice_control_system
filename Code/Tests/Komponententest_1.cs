using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Spritzgusslogik;
using NAudio.Wave;

namespace Tests
{
    [TestClass]
    public class Komponententest_1
    {

        [TestMethod]
        public void TestModuleMicrophone()
        {
            byte[] buffer = new byte[] { 0,54, 145, 230, 170, 124,34, 0 };

            Microphone mic = new Microphone();
            mic.Start();

            WaveInEventArgs eTest = new WaveInEventArgs(buffer, buffer.Length);

            var privateObject = new PrivateObject(mic);

            privateObject.Invoke("WaveIn_DataAvailable",mic,eTest);

            mic.Stop();

            var result = mic.GetValues();
            Assert.IsNotNull(result);


        }
    }
}
