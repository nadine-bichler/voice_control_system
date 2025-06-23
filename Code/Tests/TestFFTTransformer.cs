using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Spritzgusslogik;


namespace Tests
{
    [TestClass]
    public class TestFFTTransformer
    {
        // FFTTransformer fftT = new FFTTransformer();

        [TestMethod]
        public void TestTransformerMathNet()
        {
            SignalTransformer sT = new SignalTransformer();

            


            Int16[] signal = new Int16[] { 0, 1000, -1000, 0 };

            double[] result = sT.Transform(signal);
            Assert.IsTrue(result.Length > 0);

            Assert.IsTrue(Math.Abs(result[0]) < 0.05);

        }

        [TestMethod]
        public void TestTransformerMathNet2()
        {
            SignalTransformer sT = new SignalTransformer();
            Int16[] signal = new Int16[] { 0, 100, -100, 0 };
            double[] result1 = sT.Transform(signal);
            double[] result2 = sT.Transform(signal);

            Assert.AreEqual(result1.Length, result2.Length);
            Assert.AreEqual(result1[0], result2[0]);
            Assert.AreEqual(result1[result1.Length - 1], result2[result2.Length - 1]);
        }

        [TestMethod]
        public void TestTransformerFFTSharp()
        {
        
            SignalFFTSharpTransformer ssharpT = new SignalFFTSharpTransformer();

            Int16[] signal = new Int16[] {0,1000,-1000,0 };

            double[] result = ssharpT.Transform(signal);

            Assert.IsTrue(result.Length > 0);

            Assert.IsTrue(Math.Abs(result[0]) < 0.05);

        }

        [TestMethod]
        public void TestTransformerFFTSharp2()
        {

            SignalFFTSharpTransformer ssharpT = new SignalFFTSharpTransformer();

            Int16[] signal = new Int16[] { 0, 1000, -1000, 0 };

            double[] result1 = ssharpT.Transform(signal);
            double[] result2 = ssharpT.Transform(signal);

            Assert.AreEqual(result1.Length, result2.Length);
            Assert.AreEqual(result1[0], result2[0]);
            Assert.AreEqual(result1[result1.Length-1], result2[result2.Length - 1]);

        }

        [TestMethod]
        public void TestBothTransformer() 
        {
            SignalFFTSharpTransformer ssharpT = new SignalFFTSharpTransformer();
            SignalTransformer sT = new SignalTransformer();



            Int16[] signal = { 0, 100, -100,0 };

            double[] result1 = sT.Transform(signal);
            double[] result2 = ssharpT.Transform(signal);

            Assert.AreEqual(result1.Length, result2.Length, "Array-Längen müssen übereinstimmen");

            const double epsilon = 1e-6;
            Assert.AreEqual(result1[1], result2[1], epsilon, "Wert an Index 1 stimmt nicht überein");
            Assert.AreEqual(
                result1[result1.Length - 1],
                result2[result2.Length - 1],
                epsilon,
                "Wert am letzten Index stimmt nicht überein"
            );

        }

        [TestMethod]
        public void TestBothTransformer2()   
        {
            SignalFFTSharpTransformer ssharpT = new SignalFFTSharpTransformer();
            SignalTransformer sT = new SignalTransformer();

            Int16[] signal = new Int16[] {0,0,0,0};

            double[] result1 = ssharpT.Transform(signal);
            double[] result2 = sT.Transform(signal);

            // Assert
            Assert.IsNotNull(result1, "Erstes Ergebnis darf nicht null sein");
            Assert.IsNotNull(result2, "Zweites Ergebnis darf nicht null sein");
            Assert.AreEqual(result1.Length, result2.Length, "Array-Längen müssen übereinstimmen");
            
           

        }

     
    }
}
