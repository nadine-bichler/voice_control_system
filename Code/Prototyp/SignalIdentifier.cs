using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using Common;
using NWaves.Utils;
using Spritzgusslogik;

namespace Prototyp
{
    internal class SignalIdentifier
    {
        private readonly SignalFFTSharpTransformer transformer = new SignalFFTSharpTransformer();
        private readonly double threshold = 0.75;
        private Identify2 identify2 = new Identify2();

        //Event declaration
        public event EventHandler<DigitRecognizedEventArgs>? DigitRecognized;

        public void Process(Int16[] pcm)
        {
            if (pcm == null || pcm.Length == 0)
                return;
           

           //  float[] floatInput = pcm.Select(x => x / 32768f).ToArray();
            // var mfcc = audioConvert.MfccFromFloat(floatInput);
            // int result = Identify.Recognize(mfcc, out double sim);
            double[] fft = transformer.Transform(pcm);
            int result = identify2.Recognize(fft, out double sim);

            if (sim >= threshold)
                DigitRecognized?.Invoke(this, new DigitRecognizedEventArgs(result, sim));
        }
    }

    internal class DigitRecognizedEventArgs : EventArgs
    {
        public int Digit { get; }
        public double Confidence { get; }

        public DigitRecognizedEventArgs(int digit, double confidence)
        { 
            Digit = digit;
            Confidence = confidence;
        }
    }
}
