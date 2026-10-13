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
    /// Class Transforms Signals with Fast Furier Transformation using Mathnet.
    /// </summary>
    public class SignalTransformer : IFFTTransformer
    {
        /// <summary>
        /// This method first converts that given signal array from type Int16 to Complex Array.
        /// Then uses Fourier.Forward() function from MathNet to execute the Fast Fourier Transformation.
        /// Then iterates using for-loop through FFTBuffer and saves the Magnitudes to signalsMagnitude double Array.
        /// Then returns that signalsMagnitude double Array.
        /// </summary>
        /// <param name="signal">signal values</param>
        /// <returns>double array of Magnitudes</returns>
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
