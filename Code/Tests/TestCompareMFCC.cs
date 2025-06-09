using Microsoft.VisualStudio.CodeCoverage;
using SampleToFile;
using Spritzgusslogik;

namespace Tests;

[TestClass]
public class TestCompareMFCC
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

        Assert.IsTrue(result <= 50, $"Similarity is too low: {result}");
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


    [DataTestMethod]
    [DataRow(3)]
    [DataRow(5)]
    [DataRow(9)]
    public void TestCompareToAllDigits(int expectedDigit)
    {
        var basePath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "SampleToFile", "WAV_samples");
        var referencePath = Path.Combine(basePath, $"Test{expectedDigit}.csv");
        var reference = audioConvert.LoadDoubleArrayCSV(referencePath);

        var bestMatch = -1;
        var bestSimilarity = 0.0;
        var simExpected = 0.0;

        for (int i = 1; i <= 9; i++)
        {
            var scores = new List<double>();

            var files = Directory.GetFiles(basePath, $"*{i}.csv");

            foreach (var file in files)
            {
                var test = audioConvert.LoadDoubleArrayCSV(file);
                var sim = Identify.compare(reference, test);
                scores.Add(sim);
            }

            if (scores.Count == 0)
                continue;

            var avgScore = scores.Average();

            if (i == expectedDigit)
                simExpected = avgScore;

            if (avgScore > bestSimilarity)
            {
                bestSimilarity = avgScore;
                bestMatch = i;
            }
        }

        Assert.AreEqual(expectedDigit, bestMatch,
            $"Vergleich stimmt nicht: {expectedDigit} = {simExpected:F3}, {bestMatch} = {bestSimilarity:F3}");
    }
}
