using Microsoft.VisualStudio.TestTools.UnitTesting;
using Spritzgusslogik;

namespace Tests
{
    /// <summary>
    /// Test class to verify the <c>Recognize</c> method in the <c>Identify</c> class.
    /// </summary>
    [TestClass]
    public class TestIdentify
    {
        /// <summary>
        /// Tests cosine similarity on valid input
        /// </summary>
        [TestMethod]
        public void TestCosineSimilarity_ValidInput()
        {
            double[] a = { 1, 0 };
            double[] b = { 0, 1 };
            var result = Identify.CosineSimilarity(a, b);
            Assert.AreEqual(0, result, 1e-10);

            double[] c = { 1, 1 };
            double[] d = { 1, 1 };
            result = Identify.CosineSimilarity(c, d);
            Assert.AreEqual(1, result, 1e-10);
        }

        /// <summary>
        /// Tests exception for cosine similarity on inputs with different lengths
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestCosineSimilarity_DifferentLengths_Throws()
        {
            double[] a = { 1, 2 };
            double[] b = { 1 };
            Identify.CosineSimilarity(a, b);
        }


        /// <summary>
        /// Tests the PearsonCorrelation for valid input
        /// </summary>
        [TestMethod]
        public void PearsonCorrelation_ValidInput_ReturnsExpectedValue()
        { 
            double[] a = { 1, 2, 3, 4, 5 };
            double[] b = { 2, 4, 6, 8, 10 }; 

            double result = Identify.PearsonCorrelation(a, b);

            Assert.AreEqual(1.0, result, 1e-10, "Pearson correlation should be 1 for perfectly linear data.");
        }

        /// <summary>
        /// Tests the PearsonCorrelation for perfectly inversely correlated data, which should return -1.
        /// </summary>
        [TestMethod]
        public void PearsonCorrelation_InverseInput_ReturnsMinusOne()
        {
            double[] a = { 1, 2, 3, 4, 5 };
            double[] b = { 10, 8, 6, 4, 2 }; 

            double result = Identify.PearsonCorrelation(a, b);

            Assert.AreEqual(-1.0, result, 1e-10, "Pearson correlation should be -1 for perfectly inversely correlated data.");
        }

        /// <summary>
        /// Tests the PearsonCorrelation for different lengths of input arrays, which should throw an exception.
        /// </summary>
        [TestMethod]
        public void PearsonCorrelation_DifferentLengths_ThrowsArgumentException()
        {
            double[] a = { 1, 2, 3 };
            double[] b = { 1, 2 };

            Assert.ThrowsException<ArgumentException>(() => Identify.PearsonCorrelation(a, b));
        }

        /*
        /// <summary>
        /// Deprecated: Tests Euclidean distance between two audio files.
        /// </summary>
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

            Assert.IsTrue(result >= 25.0, $"Distance is too small: {result}");
        }

        /// <summary>
        /// Deprecated: Tests the combined similarity function using cosine, Pearson, and Euclidean.
        /// </summary>
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
        /// Tests the <c>Recognize</c> method by comparing one digit sample against all stored sample digits.
        /// </summary>
        /// <param name="expectedDigit">The digit that is expected to be recognized.</param>
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
                $"Recognition mismatch: expected {expectedDigit}, got {result} with similarity {similarity}");
        }

        /// <summary>
        /// Tests if no input results in Exception
        /// </summary>
        [TestMethod]
        public void TestNoInput()
        {
            var ex = Assert.ThrowsException<ArgumentNullException>(() =>
            {
                Spritzgusslogik.Identify.Recognize(null, out _);
            });

            Assert.AreEqual("input", ex.ParamName);
        }

        /// <summary>
        /// Tests if invalid digit is recognized
        /// </summary>
        [TestMethod]
        public void TestInvalidCommand()
        {
            var basePath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "SampleToFile", "wavTest");
            var referencePath = Path.Combine(basePath, "0.csv");
            var reference = audioConvert.LoadDoubleArrayCsv(referencePath);

            var result = Identify.Recognize(reference, out double similarity);
            Assert.IsTrue(similarity < 0.8, "Similarity to high, 0 is recognized");

        }

        /// <summary>
        /// Tests if different sized inputs are throwing an exception
        /// </summary>
        [TestMethod]
        public void TestDifferentSizes()
        {
            var input = new double[] { 1.0, 2.0, 3.0 };

            Assert.ThrowsException<ArgumentException>(() =>
            {
                var result = Identify.Recognize(input, out _);
            });
        }
    }
}