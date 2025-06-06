using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using FftSharp;

namespace Spritzgusslogik
{
    public class SignalFFTSharpTransformer : IFFTransformer
    {
        public Complex[] Transform(Int16[] signal)
        {
            float[] floatSignal = new float[signal.Length];

            for(int i= 0; i < signal.Length; i++)
            {
                floatSignal[i] = (float)signal[i];
            }

            FFT.ForwardReal(signal, FFTWindowType.Hanning);
        }
    }
}
