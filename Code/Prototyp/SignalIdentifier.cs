using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using Common;
using Spritzgusslogik;

namespace Prototyp
{
    internal class SignalIdentifier
    {
        private readonly SignalFFTSharpTransformer transformer = new SignalFFTSharpTransformer();
        private readonly double threshold = 0.75;

        //Event declaration
        public event EventHandler<DigitRecognizedEventArgs>? DigitRecognized;

        public void Process(Int16[] pcm)
        {
            if (pcm == null || pcm.Length == 0)
                return;

            var features = transformer.Transform(pcm);
            //float[] floatInput = features.Select(x => (float)x).ToArray();
            //var mfcc = audioConvert.MfccFromDouble(floatInput);
            int result = Identify.Recognize(features, out double sim);

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
