using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Moq;
using Spritzgusslogik;
using NAudio.Wave;
using System.Reflection;

namespace Tests
{
    /// <summary>
    /// Test class for verifying the functionality of the Microphone Methods
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

        /// <summary>
        /// Tests if CalculateRMS works
        /// </summary>
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


    /// <summary>
    /// Test class for verifying the functionality of the VoiceRecognition and Extraction.
    /// </summary>
    [TestClass]
    public class WaveInDataAvailableTests
    {
        private Microphone mic;
        private Mock<IAudioDeviceManager> mockDeviceManager;
        private List<short[]> audioReceivedCalls;

        /// <summary>
        /// Initializes the test environment, creating a mock device manager and a microphone instance.
        /// </summary>
        [TestInitialize]
        public void Setup()
        {
            mockDeviceManager = new Mock<IAudioDeviceManager>();
            mic = new Microphone(mockDeviceManager.Object, null);

            audioReceivedCalls = new();
            mic.AudioReceived += (samples) => audioReceivedCalls.Add(samples);
        }

        /// <summary>
        /// Creates a <c>WaveInEventArgs</c> instance from an array of short samples.
        /// </summary>
        private WaveInEventArgs CreateBuffer(params short[] samples)
        {
            byte[] buffer = new byte[samples.Length * 2];
            Buffer.BlockCopy(samples, 0, buffer, 0, buffer.Length);
            return new WaveInEventArgs(buffer, buffer.Length);
        }

        /// <summary>
        /// Tests that the microphone does nothing when the RMS value is below the start threshold.
        /// </summary>
        [TestMethod]
        public void DoesNothing_WhenBelowStartThreshold()
        {
            var args = CreateBuffer(Enumerable.Repeat((short)100, 100).ToArray()); // RMS < 0.03
            mic.WaveIn_DataAvailable(null, args);

            Assert.IsFalse(audioReceivedCalls.Any());
        }

        /// <summary>
        /// Tests that the microphone starts recording when the RMS value is above the start threshold.
        /// </summary>
        [TestMethod]
        public void StartsRecording_WhenAboveStartThreshold()
        {
            var args = CreateBuffer(Enumerable.Repeat(short.MaxValue, 100).ToArray()); // RMS ~1.0
            mic.WaveIn_DataAvailable(null, args);

            Assert.IsFalse(audioReceivedCalls.Any());
        }

        /// <summary>
        /// Tests that the microphone continues recording when the RMS value is above the start threshold.
        /// </summary>
        [TestMethod]
        public void StopsRecording_AndInvokesAudioReceived_WhenSilenceExceedsThreshold()
        {
            var loudSamples = Enumerable.Repeat(short.MaxValue, 2000).ToArray(); // RMS ~1.0 → Start
            var silentSamples = new short[2000]; // RMS = 0.0 → Silence

            mic.WaveIn_DataAvailable(null, CreateBuffer(loudSamples));

            typeof(Microphone)
                .GetField("lastVoiceTime", BindingFlags.NonPublic | BindingFlags.Instance)!
                .SetValue(mic, DateTime.Now - TimeSpan.FromMilliseconds(600));

            mic.WaveIn_DataAvailable(null, CreateBuffer(silentSamples));

            Assert.AreEqual(1, audioReceivedCalls.Count);
            Assert.IsTrue(audioReceivedCalls[0].Length >= 2000);
        }

        /// <summary>
        /// Tests that the microphone stops recording without invoking audio received when the silence is too short.
        /// </summary>
        [TestMethod]
        public void StopsRecording_WithoutInvoke_WhenBufferIsTooShort()
        {
            var loudSamples = Enumerable.Repeat(short.MaxValue, 500).ToArray(); // Start
            var silentSamples = new short[500];

            mic.WaveIn_DataAvailable(null, CreateBuffer(loudSamples));

            typeof(Microphone)
                .GetField("lastVoiceTime", BindingFlags.NonPublic | BindingFlags.Instance)!
                .SetValue(mic, DateTime.Now - TimeSpan.FromMilliseconds(600));

            mic.WaveIn_DataAvailable(null, CreateBuffer(silentSamples));

            Assert.AreEqual(0, audioReceivedCalls.Count);
        }

        /// <summary>
        /// Tests that the microphone updates the last voice time when the RMS value is above the end threshold during recording.
        /// </summary>
        [TestMethod]
        public void UpdatesLastVoiceTime_WhenAboveEndThreshold_DuringRecording()
        {
            var loudSamples = Enumerable.Repeat(short.MaxValue, 1000).ToArray();

            mic.WaveIn_DataAvailable(null, CreateBuffer(loudSamples)); // Start recording
            var before = DateTime.Now;

            mic.WaveIn_DataAvailable(null, CreateBuffer(loudSamples)); // Sustain recording

            var lastVoiceTime = (DateTime)typeof(Microphone)
                .GetField("lastVoiceTime", BindingFlags.NonPublic | BindingFlags.Instance)!
                .GetValue(mic)!;

            Assert.IsTrue(lastVoiceTime >= before);
        }
    }
}