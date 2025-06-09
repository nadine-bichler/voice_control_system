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
    public class SignalTransformer : IFFTTransformer
    {
     
     

        // 
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
              Fourier.Forward(FFTBuffer, FourierOptions.Matlab);

              //Console.Write("Transformed Signals: ");
              for (int i = 0; i < sampleCount; i++)
              {
                //Console.Write(FFTBuffer[i].ToString());
              }
            double[] signalsMagnitude = new double[sampleCount];
              for(int j = 0; j<sampleCount; j++)
            {
                signalsMagnitude[j] = FFTBuffer[j].Magnitude;
            }

              return signalsMagnitude;
            
           

        } 

    }
}
