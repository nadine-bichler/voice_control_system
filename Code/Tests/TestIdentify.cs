using Microsoft.VisualStudio.TestTools.UnitTesting;
using Spritzgusslogik;

namespace Tests
{
    /// <summary>
    /// Test class to verify the DTW based recognition in the <c>Identify</c> class.
    /// </summary>
    [TestClass]
    public class TestIdentify
    {
        /// <summary>
        /// Tests that the DTW distance of identical sequences is (almost) zero.
        /// </summary>
        [TestMethod]
        public void DtwDistance_IdenticalSequences_ReturnsZero()
        {
            double[][] seq = { new double[] { 1, 0 }, new double[] { 0, 1 }, new double[] { 1, 1 } };

            double result = Identify.DtwDistance(seq, seq);

            Assert.AreEqual(0.0, result, 1e-10);
        }

        /// <summary>
        /// Tests that the DTW distance is symmetric.
        /// </summary>
        [TestMethod]
        public void DtwDistance_IsSymmetric()
        {
            double[][] a = { new double[] { 1, 2 }, new double[] { 3, 4 }, new double[] { 5, 6 } };
            double[][] b = { new double[] { 1, 1 }, new double[] { 2, 2 } };

            double ab = Identify.DtwDistance(a, b);
            double ba = Identify.DtwDistance(b, a);

            Assert.AreEqual(ab, ba, 1e-10);
        }

        /// <summary>
        /// Tests that an empty sequence yields an infinite distance.
        /// </summary>
        [TestMethod]
        public void DtwDistance_EmptySequence_ReturnsInfinity()
        {
            double[][] a = { new double[] { 1, 2 } };
            double[][] empty = System.Array.Empty<double[]>();

            Assert.IsTrue(double.IsPositiveInfinity(Identify.DtwDistance(a, empty)));
            Assert.IsTrue(double.IsPositiveInfinity(Identify.DtwDistance(empty, a)));
        }

        /// <summary>
        /// Tests that passing null throws an exception.
        /// </summary>
        [TestMethod]
        public void RecognizeSequence_NullInput_ThrowsArgumentNullException()
        {
            var ex = Assert.ThrowsException<System.ArgumentNullException>(() =>
            {
                Identify.RecognizeSequence(null!, out _);
            });

            Assert.AreEqual("input", ex.ParamName);
        }

        /// <summary>
        /// Tests that a stored test sequence is recognized as its own digit.
        /// </summary>
        /// <param name="expectedDigit">The digit that is expected to be recognized.</param>
        [DataTestMethod]
        [DataRow(3)]
        [DataRow(5)]
        [DataRow(9)]
        public void RecognizeSequence_KnownSample_ReturnsDigit(int expectedDigit)
        {
            var basePath = System.IO.Path.Combine(System.AppContext.BaseDirectory, "..", "..", "..", "..",
                "SampleToFile", "WAV_sequences");
            var referencePath = System.IO.Path.Combine(basePath, $"Test{expectedDigit}.csv");
            var reference = audioConvert.LoadDoubleArray2DCsv(referencePath);

            Assert.IsNotNull(reference, "Referenzsequenz nicht gefunden: " + referencePath);

            var result = Identify.RecognizeSequence(reference!, out double similarity);

            Assert.AreEqual(expectedDigit, result,
                $"Recognition mismatch: expected {expectedDigit}, got {result} with similarity {similarity}");
        }
    }
}
