using Microsoft.VisualStudio.CodeCoverage;
using Spritzgusslogik;
using Common;

namespace Tests;

[TestClass]
public class TestIdentify
{
    [TestMethod]
    //vergleicht nur einen Wert(3) ob erkannt wird
    //Ergebnis von 0-1 (je höher desto besser)
    public void TestCosine()
    {
        var basePath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "SampleToFile", "WAV_samples");
        var referencePath = Path.Combine(basePath, "3.csv");
        var testPath = Path.Combine(basePath, "Test3.csv");

        var reference = audioConvert.LoadDoubleArrayCSV(referencePath);
        var test = audioConvert.LoadDoubleArrayCSV(testPath);

        var result = Identify.CosineSimilarity(reference, test);

        Assert.IsTrue(result >= 0.75, $"Similarity is too low: {result}");
    }

    [TestMethod]
    //vergleicht nur einen Wert(3) ob erkannt wird
    //Ergebnis mit Zahlen (je näher an 0 desto besser)
    public void TestEuclidean()
    {
        var basePath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "SampleToFile", "WAV_samples");
        var referencePath = Path.Combine(basePath, "3.csv");
        var testPath = Path.Combine(basePath, "Test3.csv");

        var reference = audioConvert.LoadDoubleArrayCSV(referencePath);
        var test = audioConvert.LoadDoubleArrayCSV(testPath);

        var result = Identify.EuclideanDistance(reference, test);

        Assert.IsTrue(result >= 50.0, $"Similarity is too low: {result}");
    }

    [TestMethod]
    //vergleicht nur einen Wert(3) ob erkannt wird
    //Ergebnis von -1 - 1 (Je näher an 1 desto besser)
    public void TestPearson()
    {
        var basePath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "SampleToFile", "WAV_samples");
        var referencePath = Path.Combine(basePath, "3.csv");
        var testPath = Path.Combine(basePath, "Test3.csv");

        var reference = audioConvert.LoadDoubleArrayCSV(referencePath);
        var test = audioConvert.LoadDoubleArrayCSV(testPath);

        var result = Identify.PearsonCorrelation(reference, test);

        Assert.IsTrue(result >= 0.75, $"Similarity is too low: {result}");
    }

    [TestMethod]
    public void TestCompare()
    {
        var basePath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "SampleToFile", "WAV_samples");
        var referencePath = Path.Combine(basePath, "3.csv");
        var testPath = Path.Combine(basePath, "Test3.csv");

        var reference = audioConvert.LoadDoubleArrayCSV(referencePath);
        var test = audioConvert.LoadDoubleArrayCSV(testPath);

        var result = Identify.Compare(reference, test);

        Assert.IsTrue(result >= 0.75, $"Combined similarity score is too low: {result}");
    }

    //Vergleicht 1 Ziffer mit allen Ziffern
    [DataTestMethod]
    [DataRow(3)]
    [DataRow(5)]
    [DataRow(9)]
    public void TestRecognize(int expectedDigit)
    {
        var basePath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "SampleToFile", "WAV_samples");   
        var referencePath = Path.Combine(basePath, $"{expectedDigit}.csv");
        var reference = audioConvert.LoadDoubleArrayCSV(referencePath);

        var result = Identify.Recognize(reference, out double similarity);

        Assert.AreEqual(expectedDigit, result,
            $"Vergleich stimmt nicht, statt: {expectedDigit} = {result} mit {similarity}");
    }
}
