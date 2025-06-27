using System.Numerics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NAudio.Wave;
using Spritzgusslogik;

namespace Tests;


/// <summary>
/// Test class to verify the LaodDoubleArrayCsv method in audioConvert
/// </summary>
[TestClass]
public class TestMicrophoneFiles
{
    private String path = "../../../../SampleToFile/wavTest";

    /// <summary>
    /// Tests if valid reference file converts right
    /// </summary>
    [TestMethod]
    public void Test_Access_Reference_File()
    {
        double[] result = audioConvert.LoadDoubleArrayCsv(path + "/test.csv");
        Assert.IsTrue(result.Length > 0);
    }

    /// <summary>
    /// Tests if unknown file results in null
    /// </summary>
    [TestMethod]
    public void Test_Access_Unknown_File()
    {
        double[] result = audioConvert.LoadDoubleArrayCsv(path + "/unknown.csv");
        Assert.IsNull(result);
    }

    /// <summary>
    /// tests if invalid file has length of 0
    /// </summary>
    [TestMethod]
    public void Test_Invalid_File()
    {
        double[] result = audioConvert.LoadDoubleArrayCsv(path + "/wrong.csv");
        Assert.IsTrue(result.Length == 0);
    }


    private readonly string testFilePath = "test_output.wav";

    [TestCleanup]
    public void Cleanup()
    {
        if (File.Exists(testFilePath))
        {
            File.Delete(testFilePath);
        }
    }

    /// <summary>
    /// Testet, ob aus gültigem PCM korrekt eine .wav-Datei erzeugt wird
    /// </summary>
    [TestMethod]
    public void SavePcmAsWav_ValidInput_CreatesWavFile()
    {
        // Arrange
        short[] pcm = Enumerable.Range(0, 44100).Select(i => (short)(Math.Sin(2 * Math.PI * 440 * i / 44100) * short.MaxValue)).ToArray();

        // Act
        audioConvert.SavePcmAsWav(pcm, testFilePath);

        // Assert
        Assert.IsTrue(File.Exists(testFilePath), "WAV-Datei wurde nicht erstellt.");

        using var reader = new WaveFileReader(testFilePath);
        Assert.AreEqual(44100, reader.WaveFormat.SampleRate);
        Assert.AreEqual(1, reader.WaveFormat.Channels);
        Assert.AreEqual(16, reader.WaveFormat.BitsPerSample);
    }

    /// <summary>
    /// Testet, ob bei leerem PCM trotzdem eine gültige .wav-Datei erzeugt wird
    /// </summary>
    [TestMethod]
    public void SavePcmAsWav_EmptyInput_CreatesEmptyWavFile()
    {
        // Arrange
        short[] pcm = Array.Empty<short>();

        // Act
        audioConvert.SavePcmAsWav(pcm, testFilePath);

        // Assert
        Assert.IsTrue(File.Exists(testFilePath));
        using var reader = new WaveFileReader(testFilePath);
        Assert.AreEqual(0, reader.Length, "WAV-Datei sollte keine Audiodaten enthalten.");
    }

    /// <summary>
    /// Testet, ob bei ungültigem Pfad eine Exception geworfen wird
    /// </summary>
    [TestMethod]
    public void SavePcmAsWav_InvalidPath_ThrowsException()
    {
        // Arrange
        short[] pcm = new short[] { 0, 1, 2 };

        // Act & Assert
        Assert.ThrowsException<DirectoryNotFoundException>(() =>
        {
            audioConvert.SavePcmAsWav(pcm, @"Z:\nicht\existent\pfad\datei.wav");
        });
    }
}