using Microsoft.VisualStudio.CodeCoverage;
using Spritzgusslogik;

namespace Tests
{
    /// <summary>
    /// Test class to verify the <c>Recognize</c> method in the <c>Identify</c> class.
    /// </summary>
    [TestClass]
    public class TestIdentify
    {
        /*
        /// <summary>
        /// Deprecated: Tests cosine similarity between two audio files.
        /// </summary>
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
        /// Deprecated: Tests Pearson correlation between two audio files.
        /// </summary>
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

            Assert.IsTrue(result >= 0.75, $"Correlation is too low: {result}");
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