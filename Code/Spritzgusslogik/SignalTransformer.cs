using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.IntegralTransforms;
using MathNet.Numerics;
using System.Numerics;



namespace Spritzgusslogik
{
    public class SignalTransformer
    {
     
        public SignalTransformer()
        {

        }

        // Auskommentiert, damit es compiled
        public Complex[] Transform(byte[] signal, int size)
        {
           

                // Audiodaten im Byte-Array signal verarbeiten
                int bytesPerSample = 2; // 16-bit PCM = 2 Bytes
                int sampleCount = size / bytesPerSample;

                // In Complex-Werte umwandeln (normalisiert auf [-1, 1])
                Complex[] FFTBuffer = new Complex[sampleCount];
                for (int i = 0; i < sampleCount; i++)
                {
                    short sample = BitConverter.ToInt16(signal, i * bytesPerSample);
                    double normalized = sample / 32768.0;
                    FFTBuffer[i] = new Complex(normalized, 0.0);
                }

                Console.Write("Transformed Signals: " + FFTBuffer);

                return FFTBuffer;
            
           

        } 

    }
}
