using Microsoft.VisualStudio.TestTools.UnitTesting;
using NAudio.Wave;
using Spritzgusslogik;
using System;
using System.Collections;
using System.Globalization;
using System.IO;
using System.Linq;

namespace Tests
{
    /// <summary>
    /// Tests for audioConvert: CSV-Import and PCM-to-WAV functionality.
    /// </summary>
    [TestClass]
    public class TestMicrophoneFiles
    {
        private readonly string csvTestFolder = "TestCsvFiles";
        private readonly string wavOutputPath = "test_output.wav";
        private readonly string referenceFolder = "../../../../SampleToFile/wavTest";

        #region Setup & Cleanup

        [TestInitialize]
        public void Setup()
        {
            Directory.CreateDirectory(csvTestFolder);
        }

        [TestCleanup]
        public void Cleanup()
        {
            if (Directory.Exists(csvTestFolder))
                Directory.Delete(csvTestFolder, true);

            if (File.Exists(wavOutputPath))
                File.Delete(wavOutputPath);
        }

        #endregion

        #region CSV Tests

        [TestMethod]
        public void LoadDoubleArrayCsv_ValidReferenceFile_ReturnsArray()
        {
            double[] result = audioConvert.LoadDoubleArrayCsv(Path.Combine(referenceFolder, "test.csv"));
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Length > 0);
        }

        [TestMethod]
        public void LoadDoubleArrayCsv_UnknownReferenceFile_ReturnsNull()
        {
            double[] result = audioConvert.LoadDoubleArrayCsv(Path.Combine(referenceFolder, "unknown.csv"));
            Assert.IsNull(result);
        }

        [TestMethod]
        public void LoadDoubleArrayCsv_InvalidReferenceFile_ReturnsEmptyArray()
        {
            double[] result = audioConvert.LoadDoubleArrayCsv(Path.Combine(referenceFolder, "wrong.csv"));
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Length);
        }

        [TestMethod]
        public void LoadDoubleArrayCsv_MixedValidAndInvalid_ReturnsEmptyArray()
        {
            var filePath = Path.Combine("TestCsvFiles", "mixed.csv");
            File.WriteAllText(filePath, "1.0, abc, 2.0");

            double[] result = audioConvert.LoadDoubleArrayCsv(filePath);

            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Length);
        }

        [TestMethod]
        public void LoadDoubleArrayCsv_ValidTestFile_ReturnsArray()
        {
            var filePath = Path.Combine(csvTestFolder, "valid.csv");
            File.WriteAllText(filePath, "1.23, 4.56, 7.89");

            double[] result = audioConvert.LoadDoubleArrayCsv(filePath);

            Assert.IsNotNull(result);
            CollectionAssert.AreEqual(new[] { 1.23, 4.56, 7.89 }, result, new DoubleComparer());
        }

        [TestMethod]
        public void LoadDoubleArrayCsv_EmptyFile_ReturnsEmptyArray()
        {
            var filePath = Path.Combine(csvTestFolder, "empty.csv");
            File.WriteAllText(filePath, "");

            double[] result = audioConvert.LoadDoubleArrayCsv(filePath);

            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Length);
        }

        [TestMethod]
        public void LoadDoubleArrayCsv_InvalidValues_ReturnsEmptyArray()
        {
            var filePath = Path.Combine(csvTestFolder, "invalid.csv");
            File.WriteAllText(filePath, "abc, xyz, 123");

            double[] result = audioConvert.LoadDoubleArrayCsv(filePath);

            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Length);
        }

        [TestMethod]
        public void LoadDoubleArrayCsv_FileDoesNotExist_ReturnsNull()
        {
            var filePath = Path.Combine(csvTestFolder, "missing.csv");

            double[] result = audioConvert.LoadDoubleArrayCsv(filePath);

            Assert.IsNull(result);
        }

        [TestMethod]
        public void LoadDoubleArrayCsv_WhitespaceOrNullPath_ReturnsNull()
        {
            Assert.IsNull(audioConvert.LoadDoubleArrayCsv(null));
            Assert.IsNull(audioConvert.LoadDoubleArrayCsv(""));
            Assert.IsNull(audioConvert.LoadDoubleArrayCsv("   "));
        }

        [TestMethod]
        public void LoadDoubleArrayCsv_FileWithMalformedLine_ReturnsParsedArray()
        {
            var filePath = Path.Combine(csvTestFolder, "malformed.csv");
            File.WriteAllText(filePath, "1.0,2.0\nthis,is,not,valid");

            double[] result = audioConvert.LoadDoubleArrayCsv(filePath);

            Assert.IsNotNull(result);
            CollectionAssert.AreEqual(new[] { 1.0, 2.0 }, result.Take(2).ToArray(), new DoubleComparer());
        }

        [TestMethod]
        public void LoadDoubleArrayCsv_FileInUse_ThrowsHandled_ReturnsEmptyArray()
        {
            var filePath = Path.Combine(csvTestFolder, "locked.csv");
            File.WriteAllText(filePath, "1.0,2.0");

            using (var stream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.None))
            {
                double[] result = audioConvert.LoadDoubleArrayCsv(filePath);

                Assert.IsNotNull(result);
                Assert.AreEqual(0, result.Length);
            }
        }

        #endregion

        #region WAV Tests

        [TestMethod]
        public void SavePcmAsWav_ValidInput_CreatesWavFile()
        {
            short[] pcm = Enumerable.Range(0, 44100).Select(i =>
                (short)(Math.Sin(2 * Math.PI * 440 * i / 44100) * short.MaxValue)).ToArray();

            audioConvert.SavePcmAsWav(pcm, wavOutputPath);

            Assert.IsTrue(File.Exists(wavOutputPath), "WAV-Datei wurde nicht erstellt.");

            using var reader = new WaveFileReader(wavOutputPath);
            Assert.AreEqual(44100, reader.WaveFormat.SampleRate);
            Assert.AreEqual(1, reader.WaveFormat.Channels);
            Assert.AreEqual(16, reader.WaveFormat.BitsPerSample);
        }

        [TestMethod]
        public void SavePcmAsWav_EmptyInput_CreatesEmptyWavFile()
        {
            short[] pcm = Array.Empty<short>();

            audioConvert.SavePcmAsWav(pcm, wavOutputPath);

            Assert.IsTrue(File.Exists(wavOutputPath));
            using var reader = new WaveFileReader(wavOutputPath);
            Assert.AreEqual(0, reader.Length, "WAV-Datei sollte keine Audiodaten enthalten.");
        }

        [TestMethod]
        public void SavePcmAsWav_InvalidPath_ThrowsException()
        {
            short[] pcm = new short[] { 0, 1, 2 };

            Assert.ThrowsException<DirectoryNotFoundException>(() =>
            {
                audioConvert.SavePcmAsWav(pcm, @"Z:\nicht\existent\pfad\datei.wav");
            });
        }

        #endregion

        #region Helpers

        private class DoubleComparer : IComparer
        {
            private const double Tolerance = 1e-10;

            public int Compare(object x, object y)
            {
                double a = (double)x;
                double b = (double)y;

                if (Math.Abs(a - b) < Tolerance)
                    return 0;
                return a.CompareTo(b);
            }
        }

        #endregion
    }
}
