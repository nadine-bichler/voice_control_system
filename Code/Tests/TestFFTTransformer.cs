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
           

            Int16[] signal = new Int16[] { 0,1000,-1000,0};

            double[] result = sT.Transform(signal);
            Assert.IsTrue(result.Length > 0);

            Assert.IsTrue(Math.Abs(result[0]) < 0.05);
           

        }
    }
}
