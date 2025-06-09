using SampleToFile;
using Spritzgusslogik;

namespace Tests;

[TestClass]
public class TestCompareMFCC
{
    [TestMethod]
    public void TestCompare1Digit()
    {
        var basePath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "SampleToFile", "WAV_samples");
        var referencePath = Path.Combine(basePath, "3.csv");
        var testPath = Path.Combine(basePath, "Test3.csv");

        var reference = audioConvert.LoadDoubleArrayCSV(referencePath);
        var test = audioConvert.LoadDoubleArrayCSV(testPath);

        var result = Identify.compare2(reference, test);

        Assert.IsTrue(result >= 0.75, $"Similarity is too low: {result}");
    }

    [TestMethod]
    public void TestCompareToAllDigits()
    {
        var basePath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "SampleToFile", "WAV_samples");


        var reference = audioConvert.LoadDoubleArrayCSV(Path.Combine(basePath, "3.csv"));

        var bestMatch = -1;
        var bestSimilarity = 0.0;

        for (int i = 1; i <= 9; i++)
        {
            var test = audioConvert.LoadDoubleArrayCSV(Path.Combine(basePath, $"Test{i}.csv"));
            var Sim = Identify.compare2(reference, test);
            if (Sim > bestSimilarity)
            {
                bestSimilarity = Sim;
                bestMatch = i;
            }
        }

        Assert.AreEqual(3, bestMatch, $"Vergleich stimmt nicht: {bestMatch}");
    }
}
