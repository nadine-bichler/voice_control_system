using System.Numerics;
using Spritzgusslogik;

namespace Tests;

[TestClass]
public class TestReferenceFiles
{


    [TestMethod]
    public void Test_Access_Reference_File()
    {
        double[] result = audioConvert.LoadDoubleArrayCsv("wavTest/test.csv");
        Assert.IsTrue(result.Length > 0);
    }

    [TestMethod]
    public void Test_Access_Unknown_File()
    {
        double[] result = audioConvert.LoadDoubleArrayCsv("wavTest/unknown.csv");
        Assert.IsNull(result);
    }

    [TestMethod]
    public void Test_Invalid_File()
    {
        double[] result = audioConvert.LoadDoubleArrayCsv("wavTest/wrong.csv");
        Assert.IsTrue(result.Length == 0);
    }
}