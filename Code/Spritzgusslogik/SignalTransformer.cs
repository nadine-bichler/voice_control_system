///
/// @file    SignalTransformer
/// @brief   transforms signal with Fourier transformation
/// @author  Donart Bazai 
/// @date    24.06.2025
///

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.IntegralTransforms;
using MathNet.Numerics;
using NAudio.Dsp;
using Complex = System.Numerics.Complex;


namespace Spritzgusslogik
{
    /// <summary>
    /// 
    /// </summary>
    public class SignalTransformer : IFFTTransformer
    {
        /// <summary
        /// </summary>
        /// <param name="signal"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public double[] Transform(Int16[] signal)
        {
            if (signal == null) throw new ArgumentNullException("SignalTransformer: Signal is Null!");


            // Audiodaten im Byte-Array signal verarbeiten
           // int bytesPerSample = 2; // 16-bit PCM = 2 Bytes
            int sampleCount = signal.Length;// / bytesPerSample;

                //Brauchen wir nicht mehr?
                // In Complex-Werte umwandeln (normalisiert auf [-1, 1])
                Complex[] FFTBuffer = new Complex[sampleCount];
                for (int i = 0; i < sampleCount; i++)
                {
                   // short sample = BitConverter.ToInt16(signal, i * bytesPerSample);
                    double normalized = signal[i] / 32768.0;
                //Hamming-Fenster reduziert Rundartefakte = sanfte ein- und ausblendung
                double window = 0.54 - 0.46 * Math.Cos(2 * Math.PI * i / (sampleCount - 1));
                FFTBuffer[i] = new Complex(normalized * window, 0.0);
                }
              Fourier.Forward(FFTBuffer, FourierOptions.Matlab);

            
            double[] signalsMagnitude = new double[sampleCount];
              for(int j = 0; j<sampleCount; j++)
            {
                signalsMagnitude[j] = FFTBuffer[j].Magnitude;
            }

              return signalsMagnitude;
            
           

        } 

    }
}
