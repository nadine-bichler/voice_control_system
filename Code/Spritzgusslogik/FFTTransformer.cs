///
/// @file    FFTTransformer
/// @brief   class for Fouriertransformation
/// @author  Bazai Donart
/// @date    24.06.2025
///
///

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Complex = System.Numerics.Complex;

namespace Spritzgusslogik
{
    /// <summary>
    /// 
    /// </summary>
    public class FFTTransformer
    {
        /// <summary>
        /// 
        /// </summary>
        public IFFTTransformer TransformerAlgo;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="newAlgo"></param>
        public void setAlgorithm(IFFTTransformer newAlgo) { 
            TransformerAlgo = newAlgo;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="signal"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public double[] Transform(Int16[] signal) { 
        
            if(TransformerAlgo == null)
            {
                throw new ArgumentNullException("Transformer Algorithm is NULL");
            }
            return TransformerAlgo.Transform(signal);
        }
    }
}
