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
        public double[] Transform(Int16[] signal)
        {
           

            // Audiodaten im Byte-Array signal verarbeiten
            int bytesPerSample = 2; // 16-bit PCM = 2 Bytes
            int sampleCount = signal.Length / bytesPerSample;

            // In Complex-Werte umwandeln (normalisiert auf [-1, 1])
            Complex[] FFTBuffer = new Complex[sampleCount];
            for (int i = 0; i < sampleCount; i++)
            {
                // short sample = BitConverter.ToInt16(signal, i * bytesPerSample);
                double normalized = signal[i] / 32768.0;
                FFTBuffer[i] = new Complex(normalized, 0.0);
            }

            //tranform with FFT and get the Magnitude
            double[] fftMagnitude = FFT.Magnitude(FFTBuffer);

            return fftMagnitude;
        }
    }
}
