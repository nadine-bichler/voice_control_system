using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Spritzgusslogik;

namespace Tests
{
    /// <summary>
    /// Test class for verifying the functionality of the <c>History</c> and <c>HistoryEntry</c> classes.
    /// </summary>
    [TestClass]
    public class TestMicrophone
    {
        /// <summary>
        /// Tests if the microphone is unmuted after calling start and muted after calling stop.
        /// </summary>
        [TestMethod]
        public void Test_IsMuted()
        {
            Microphone microphone = new Microphone();
            Assert.IsTrue(microphone.IsMuted(), "Microphone should be muted initially.");

            microphone.Start();
            Assert.IsFalse(microphone.IsMuted(), "Microphone should not be muted after starting.");

            microphone.Stop();
            Assert.IsTrue(microphone.IsMuted(), "Microphone should be muted after stopping.");
        }

        [TestMethod]
        public void Test_GetStandardMicrophoneName()
        {
            Microphone microphone = new Microphone();
            string name = microphone.GetStandardMicrophoneName();
            Assert.IsFalse(string.IsNullOrEmpty(name), "Microphone name should not be empty.");
            Console.WriteLine("Microphone Name: " + name);
        }
    }
}