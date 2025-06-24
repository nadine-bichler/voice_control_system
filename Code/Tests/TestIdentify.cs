using Microsoft.VisualStudio.CodeCoverage;
using Spritzgusslogik;

namespace Tests;

/// <summary>
/// this class tests the recognize method
/// </summary>
[TestClass]
public class TestIdentify
{
    /*
    [TestMethod]
    [Obsolete("Deprecated test, use TestRecognize instead")]
    public void TestCosine()
    {
        var basePath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "SampleToFile", "WAV_samples");
        var referencePath = Path.Combine(basePath, "3.csv");
        var testPath = Path.Combine(basePath, "Test3.csv");

        var reference = audioConvert.LoadDoubleArrayCsv(referencePath);
        var test = audioConvert.LoadDoubleArrayCsv(testPath);

        var result = Identify.CosineSimilarity(reference, test);

        Assert.IsTrue(result >= 0.75, $"Similarity is too low: {result}");
    }

    [TestMethod]
    [Obsolete("Deprecated test, use TestRecognize instead")]
    public void TestEuclidean()
    {
        var basePath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "SampleToFile", "WAV_samples");
        var referencePath = Path.Combine(basePath, "3.csv");
        var testPath = Path.Combine(basePath, "Test3.csv");

        var reference = audioConvert.LoadDoubleArrayCsv(referencePath);
        var test = audioConvert.LoadDoubleArrayCsv(testPath);

        var result = Identify.EuclideanDistance(reference, test);

        Assert.IsTrue(result >= 25.0, $"Similarity is too low: {result}");
    }

    [TestMethod]
    [Obsolete("Deprecated test, use TestRecognize instead")]
    public void TestPearson()
    {
        var basePath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "SampleToFile", "WAV_samples");
        var referencePath = Path.Combine(basePath, "3.csv");
        var testPath = Path.Combine(basePath, "Test3.csv");

        var reference = audioConvert.LoadDoubleArrayCsv(referencePath);
        var test = audioConvert.LoadDoubleArrayCsv(testPath);

        var result = Identify.PearsonCorrelation(reference, test);

        Assert.IsTrue(result >= 0.75, $"Similarity is too low: {result}");
    }

    [TestMethod]
    [Obsolete("Deprecated test, use TestRecognize instead")]
    public void TestCompare()
    {
        var basePath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "SampleToFile", "WAV_samples");
        var referencePath = Path.Combine(basePath, "3.csv");
        var testPath = Path.Combine(basePath, "Test3.csv");

        var reference = audioConvert.LoadDoubleArrayCsv(referencePath);
        var test = audioConvert.LoadDoubleArrayCsv(testPath);

        var result = Identify.Compare(reference, test);

        Assert.IsTrue(result >= 0.75, $"Combined similarity score is too low: {result}");
    }
    */


    /// <summary>
    /// this method compares one digit with all samples to determine which digit is being said
    /// </summary>
    /// <param name="expectedDigit">expected digit</param>
    [DataTestMethod]
    [DataRow(3)]
    [DataRow(5)]
    [DataRow(9)]
    public void TestRecognize(int expectedDigit)
    {
        var basePath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "SampleToFile", "WAV_samples");
        var referencePath = Path.Combine(basePath, $"{expectedDigit}.csv");
        var reference = audioConvert.LoadDoubleArrayCsv(referencePath);

        var result = Identify.Recognize(reference, out double similarity);

        Assert.AreEqual(expectedDigit, result,
            $"Vergleich stimmt nicht, statt: {expectedDigit} = {result} mit {similarity}");
    }
}