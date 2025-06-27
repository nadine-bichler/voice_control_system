using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Moq;
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

        /// <summary>
        /// Tests for the standard microphone name retrieval functionality.
        /// </summary>
        [TestMethod]
        public void Test_GetStandardMicrophoneName()
        {
            Microphone microphone = new Microphone();
            string name = microphone.GetStandardMicrophoneName();
            Assert.IsFalse(string.IsNullOrEmpty(name), "Microphone name should not be empty.");
            Console.WriteLine("Microphone Name: " + name);
        }

        /// <summary>
        /// Tests for the standard microphone name retrieval when no devices are available.
        /// </summary>
        [TestMethod]
        public void Test_GetStandardMicrophoneName_ReturnsNoMicrophoneFoundWhenNoDevices()
        {
            var mockManager = new Mock<IAudioDeviceManager>();
            mockManager.Setup(m => m.DeviceCount).Returns(0);

            var microphone = new Microphone(mockManager.Object, null);
            string name = microphone.GetStandardMicrophoneName();

            Assert.AreEqual("No microphone found", name, "Should return 'No microphone found' when no devices are available.");
        }

        /// <summary>
        /// Tests if start calls the recorder's StartRecording method and sets muted to false.
        /// </summary>
        [TestMethod]
        public void Start_CallsRecorder_StartRecording_SetsMutedFalse()
        {
            var mockDeviceManager = new Mock<IAudioDeviceManager>();
            var mockRecorder = new Mock<IAudioRecorder>();
            var mic = new Microphone(mockDeviceManager.Object, mockRecorder.Object);

            mic.Start();

            mockRecorder.Verify(r => r.StartRecording(), Times.Once());
            Assert.IsFalse(mic.IsMuted());
        }

        /// <summary>
        /// Tests if start handles exceptions from the recorder and prints an error message while setting muted to false.
        /// </summary>
        [TestMethod]
        public void Start_WhenExceptionThrown_PrintsErrorAndSetsMutedFalse()
        {
            var mockDeviceManager = new Mock<IAudioDeviceManager>();
            var mockRecorder = new Mock<IAudioRecorder>();
            mockRecorder.Setup(r => r.StartRecording()).Throws(new InvalidOperationException("BOOM"));

            var mic = new Microphone(mockDeviceManager.Object, mockRecorder.Object);

            using var sw = new StringWriter();
            Console.SetOut(sw);

            mic.Start();

            var output = sw.ToString();
            StringAssert.Contains(output, "Error while starting recording: BOOM");
            Assert.IsFalse(mic.IsMuted());
        }

        private class TestableMicrophone : Microphone
        {
            public float InvokeCalculateRMS(short[] samples)
            {
                return base.CalculateRMS(samples);
            }
        }

        /// <summary>
        /// Tests the CalculateRMS method with an empty array to ensure it returns zero.
        /// </summary>
        [TestMethod]
        public void CalculateRMS_EmptyArray_ReturnsZero()
        {
            var mic = new TestableMicrophone();
            var samples = new short[0];

            float rms = mic.InvokeCalculateRMS(samples);

            Assert.AreEqual(0f, rms);
        }

        /// <summary>
        /// Tests the CalculateRMS method with a non-empty array to ensure it returns the expected RMS value.
        /// </summary>
        [TestMethod]
        public void CalculateRMS_NonEmptyArray_ReturnsExpectedRMS()
        {
            var mic = new TestableMicrophone();
            var samples = new short[] { 32767, 0, -32768 };

            float expected = (float)Math.Sqrt((Math.Pow(32767 / 32768f, 2) + 0 + Math.Pow(-32768 / 32768f, 2)) / 3);

            float rms = mic.InvokeCalculateRMS(samples);
            Assert.AreEqual(expected, rms, 0.0001f);
        }
    }
}