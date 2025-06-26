using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Complex = System.Numerics.Complex;

namespace Spritzgusslogik
{
    /// <summary>
    /// sets the new Transform Algorithm and calls Transform Method.
    /// Part of Strategy Pattern
    /// </summary>
    public class FFTTransformer
    {
        public IFFTTransformer TransformerAlgo;


        /// <summary>
        /// This method sets the new Transform ALgorithm.
        /// </summary>
        /// <param name="newAlgo">IFFTTransformer Transformation Algorithm</param>
        /// <exception cref="ArgumentNullException"></exception>
        public void setAlgorithm(IFFTTransformer newAlgo) { 
            if(newAlgo == null) throw new ArgumentNullException(nameof(newAlgo));

            TransformerAlgo = newAlgo;
        }

        
        /// <summary>
        /// This method calls Transform() method of initialised TransformerAlgo.
        /// </summary>
        /// <param name="signal">signal values</param>
        /// <returns>double-array of Magnitudes</returns>
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
