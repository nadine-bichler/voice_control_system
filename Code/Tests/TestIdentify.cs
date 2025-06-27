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

        /// <summary>
        /// Tests the EuclideanDistance with two identical vectors (should be 0).
        /// </summary>
        [TestMethod]
        public void EuclideanDistance_SameVectors_ReturnsZero()
        {
            double[] a = { 1, 2, 3 };
            double[] b = { 1, 2, 3 };

            double result = Identify.EuclideanDistance(a, b);

            Assert.AreEqual(0.0, result, 1e-10);
        }

        /// <summary>
        /// Tests the EuclideanDistance with positive values.
        /// </summary>
        [TestMethod]
        public void EuclideanDistance_KnownVectors_ReturnsCorrectDistance()
        {
            double[] a = { 1, 2, 3 };
            double[] b = { 4, 6, 8 };

            double expected = Math.Sqrt(50);
            double result = Identify.EuclideanDistance(a, b);

            Assert.AreEqual(expected, result, 1e-10);
        }

        /// <summary>
        /// Tests that an ArgumentException is thrown for arrays of different lengths.
        /// </summary>
        [TestMethod]
        public void EuclideanDistance_DifferentLengths_ThrowsArgumentException()
        {
            double[] a = { 1, 2 };
            double[] b = { 1, 2, 3 };

            Assert.ThrowsException<ArgumentException>(() =>
            {
                Identify.EuclideanDistance(a, b);
            });
        }

        /// <summary>
        /// Tests that passing null throws an exception.
        /// </summary>
        [TestMethod]
        public void EuclideanDistance_NullInput_ThrowsArgumentException()
        {
            double[] a = null!;
            double[] b = { 1, 2, 3 };

            Assert.ThrowsException<ArgumentException>(() =>
            {
                Identify.EuclideanDistance(a, b);
            });
        }

        /// <summary>
        /// Tests Euclidean distance with negative values.
        /// </summary>
        [TestMethod]
        public void EuclideanDistance_NegativeValues_CalculatesCorrectly()
        {
            double[] a = { -1, -2, -3 };
            double[] b = { 1, 2, 3 };

            double expected = Math.Sqrt(56);
            double result = Identify.EuclideanDistance(a, b);

            Assert.AreEqual(expected, result, 1e-10);
        }


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
        public void RecognizeTestNoInput()
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
        public void RecognizeTestInvalidCommand()
        {
            var basePath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "SampleToFile", "wavTest");
            var referencePath = Path.Combine(basePath, "0.csv");
            var reference = audioConvert.LoadDoubleArrayCsv(referencePath);

            var result = Identify.Recognize(reference, out double similarity);
            Assert.IsTrue(similarity < 0.8, "Similarity to high, 0 is recognized");

        }

        /// <summary>
        /// Tests if Recognition is skipped if there are no matching files
        /// </summary>
        [TestMethod]
        public void RecognizeTest_NoMatchingFiles_SkipsDigit()
        {
            // Arrange
            // Lege eine Datei mit falscher Extension an, damit sie ignoriert wird
            var basePath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "SampleToFile", "WAV_samples");
            var fakeFile = Path.Combine(basePath, "1.txt"); // Wird nicht als *.csv erkannt

            File.WriteAllText(fakeFile, "1.0, 2.0, 3.0");

            try
            {
                var input = new double[] { 1.0, 2.0, 3.0 };

                // Act
                var result = Identify.Recognize(input, out double similarity);

                // Assert
                Assert.AreEqual(-1, result); // Kein Treffer, kein gültiger Vergleich
                Assert.AreEqual(0.0, similarity);
            }
            finally
            {
                if (File.Exists(fakeFile))
                    File.Delete(fakeFile);
            }
        }

    }
}