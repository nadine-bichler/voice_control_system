using System.Numerics;
using Spritzgusslogik;

namespace Tests;


/// <summary>
/// Test class to verify the LaodDoubleArrayCsv method in audioConvert
/// </summary>
[TestClass]
public class TestReferenceFiles
{

    /// <summary>
    /// Tests if valid reference file converts right
    /// </summary>
    [TestMethod]
    public void Test_Access_Reference_File()
    {
        double[] result = audioConvert.LoadDoubleArrayCsv("wavTest/test.csv");
        Assert.IsTrue(result.Length > 0);
    }

    /// <summary>
    /// Tests if unknown file results in null
    /// </summary>
    [TestMethod]
    public void Test_Access_Unknown_File()
    {
        double[] result = audioConvert.LoadDoubleArrayCsv("wavTest/unknown.csv");
        Assert.IsNull(result);
    }

    /// <summary>
    /// tests if invalid file has length of 0
    /// </summary>
    [TestMethod]
    public void Test_Invalid_File()
    {
        double[] result = audioConvert.LoadDoubleArrayCsv("wavTest/wrong.csv");
        Assert.IsTrue(result.Length == 0);
    }
}