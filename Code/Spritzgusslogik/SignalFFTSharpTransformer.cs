///
/// @file    SignalFFTSharpTransformer
/// @brief   transforms signal with Sharp Fourier transformation
/// @author  Donart Bazai 
/// @date    24.06.2025
///
///

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using FftSharp;

namespace Spritzgusslogik
{
    public class SignalFFTSharpTransformer : IFFTTransformer
    {
        //PCM ideal für FFT, MFCC, Filter, etc.
        public double[] Transform(Int16[] signal)
        {

            if (signal == null) throw new ArgumentNullException("SignalTransformer: Signal is Null!");

            // Audiodaten im Byte-Array signal verarbeiten
            // int bytesPerSample = 2; // 16-bit PCM = 2 Bytes
            int sampleCount = signal.Length;// / bytesPerSample;

            // In Complex-Werte umwandeln (normalisiert auf [-1, 1])
            Complex[] FFTBuffer = new Complex[sampleCount];
            for (int i = 0; i < sampleCount; i++)
            {
                //short sample = BitConverter.ToInt16(signal, i * bytesPerSample);
                double normalized = signal[i] / 32768.0;
                //Hamming-Fenster reduziert Rundartefakte = sanfte ein- und ausblendung
                double window = 0.54 - 0.46 * Math.Cos(2 * Math.PI * i / (sampleCount - 1)); 
                FFTBuffer[i] = new Complex(normalized * window, 0.0);
            }

            //tranform with FFT and get the Magnitude
           // double[] fftMagnitude = FFT.Magnitude(FFTBuffer);

            FFT.Forward(FFTBuffer);

            double[] fftMagnitudes = new double[sampleCount];
            for (int j = 0; j < sampleCount; j++)
            {
                fftMagnitudes[j] = FFTBuffer[j].Magnitude;
            }



           /* double scale = Math.Sqrt(2.0 * sampleCount);
            for (int i = 0; i < fftMagnitudes.Length; i++)
                fftMagnitudes[i] *= scale;*/

            return fftMagnitudes;
        }
    }
}
