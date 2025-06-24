/** 
* @file FFTTransformer.cs 
* @brief File is a Program that sets the new Transform Algorithm and calls Transform Method.
* @author Donart Bazaj 
* @date 09.05.2025 
* 
* Additional Infos: part of Strattegy Pattern
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Complex = System.Numerics.Complex;

namespace Spritzgusslogik
{
    public class FFTTransformer
    {
        public IFFTTransformer TransformerAlgo;

        /** 
        * @fn void setAlgorithm(IFFTTransformer newAlgo)
        * @brief Sets an Transform Algorithm.
        * @param newAlgo ,  IFFTTransformer
        * @return void
        * @author Donart Bazaj
        * @date 09.05.2025 
        * 
        * This method sets the new Transform ALgorithm.
        */
        public void setAlgorithm(IFFTTransformer newAlgo) { 
            if(newAlgo == null) throw new ArgumentNullException(nameof(newAlgo));

            TransformerAlgo = newAlgo;
        }
        /** 
        * @fn double[] Transform(Int16[] signal)
        * @brief Transforms that signal using FFT. 
        * @param signal , signal werte 
        * @return double [] , double-array of Magnitudes 
        * @author Donart Bazaj
        * @date 09.05.2025 
        * 
        * This method calls Transform() method of initialised TransformerAlgo.
        */
        public double[] Transform(Int16[] signal) { 
        
            if(TransformerAlgo == null)
            {
                throw new ArgumentNullException("Transformer Algorithm is NULL");
            }
            return TransformerAlgo.Transform(signal);
        }
    }
}
