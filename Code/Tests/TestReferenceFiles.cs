using System.Numerics;
using Spritzgusslogik;

namespace Tests;

[TestClass]
public class TestReferenceFiles
{


    [TestMethod]
    public void Test_Access_Reference_File()
    {
        Complex[] result = Identify.LoadComplexArrayCsv("wavTest/test.csv");
        Assert.IsTrue(result.Length > 0);
    }

    [TestMethod]
    public void Test_Access_Unknown_File()
    {
        Complex[] result = Identify.LoadComplexArrayCsv("wavTest/unknown.csv");
        Assert.IsNull(result);
    }

    [TestMethod]
    public void Test_Invalid_File()
    {
        Complex[] result = Identify.LoadComplexArrayCsv("wavTest/wrong.csv");
        Assert.IsTrue(result.Length == 0);
    }
}