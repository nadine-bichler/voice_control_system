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
            Assert.AreEqual(signal.Length, result.Length);

            Assert.AreEqual(0.0, result[0],0.001);
            Assert.AreEqual(1000.0 / 32768.0, result[1], 0.001);

        }
    }
}
