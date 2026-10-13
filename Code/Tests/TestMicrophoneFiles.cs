using Microsoft.VisualStudio.TestTools.UnitTesting;
using NAudio.Wave;
using Spritzgusslogik;
using System;
using System.IO;
using System.Linq;

namespace Tests
{
    /// <summary>
    /// Tests for audioConvert: 2D CSV import and PCM-to-WAV functionality.
    /// Uses unique temporary paths so the tests stay independent under the
    /// assembly-wide method-level parallelization.
    /// </summary>
    [TestClass]
    public class TestMicrophoneFiles
    {
        private static string UniquePath(string extension)
        {
            return Path.Combine(Path.GetTempPath(), $"prototyp_test_{Guid.NewGuid():N}{extension}");
        }

        #region CSV Tests

        [TestMethod]
        public void LoadDoubleArray2DCsv_ValidFile_ReturnsJaggedArray()
        {
            var filePath = UniquePath(".csv");
            File.WriteAllText(filePath, "1,2,3\n4,5,6");

            try
            {
                var result = audioConvert.LoadDoubleArray2DCsv(filePath);

                Assert.IsNotNull(result);
                Assert.AreEqual(2, result!.Length);
                Assert.AreEqual(3, result[0].Length);
                Assert.AreEqual(4.0, result[1][0], 1e-10);
            }
            finally
            {
                File.Delete(filePath);
            }
        }

        [TestMethod]
        public void LoadDoubleArray2DCsv_FileDoesNotExist_ReturnsNull()
        {
            var filePath = UniquePath(".csv");

            Assert.IsNull(audioConvert.LoadDoubleArray2DCsv(filePath));
        }

        [TestMethod]
        public void LoadDoubleArray2DCsv_WhitespaceOrNullPath_ReturnsNull()
        {
            Assert.IsNull(audioConvert.LoadDoubleArray2DCsv(null));
            Assert.IsNull(audioConvert.LoadDoubleArray2DCsv(""));
            Assert.IsNull(audioConvert.LoadDoubleArray2DCsv("   "));
        }

        [TestMethod]
        public void LoadDoubleArray2DCsv_InvalidValues_ReturnsEmptyArray()
        {
            var filePath = UniquePath(".csv");
            File.WriteAllText(filePath, "1,abc,3");

            try
            {
                var result = audioConvert.LoadDoubleArray2DCsv(filePath);

                Assert.IsNotNull(result);
                Assert.AreEqual(0, result!.Length);
            }
            finally
            {
                File.Delete(filePath);
            }
        }

        #endregion

        #region WAV Tests

        [TestMethod]
        public void SavePcmAsWav_ValidInput_CreatesWavFile()
        {
            var wavOutputPath = UniquePath(".wav");
            short[] pcm = Enumerable.Range(0, 44100).Select(i =>
                (short)(Math.Sin(2 * Math.PI * 440 * i / 44100) * short.MaxValue)).ToArray();

            try
            {
                audioConvert.SavePcmAsWav(pcm, wavOutputPath);

                Assert.IsTrue(File.Exists(wavOutputPath), "WAV-Datei wurde nicht erstellt.");

                using var reader = new WaveFileReader(wavOutputPath);
                Assert.AreEqual(44100, reader.WaveFormat.SampleRate);
                Assert.AreEqual(1, reader.WaveFormat.Channels);
                Assert.AreEqual(16, reader.WaveFormat.BitsPerSample);
            }
            finally
            {
                File.Delete(wavOutputPath);
            }
        }

        [TestMethod]
        public void SavePcmAsWav_EmptyInput_CreatesEmptyWavFile()
        {
            var wavOutputPath = UniquePath(".wav");
            short[] pcm = Array.Empty<short>();

            try
            {
                audioConvert.SavePcmAsWav(pcm, wavOutputPath);

                Assert.IsTrue(File.Exists(wavOutputPath));
                using var reader = new WaveFileReader(wavOutputPath);
                Assert.AreEqual(0, reader.Length, "WAV-Datei sollte keine Audiodaten enthalten.");
            }
            finally
            {
                File.Delete(wavOutputPath);
            }
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
    }
}
