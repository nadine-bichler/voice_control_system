using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Spritzgusslogik
{
    /// <summary>
    /// Interface for Fouriertransformation
    /// </summary>
    public  interface IFFTTransformer
    {
        public string Name { get; }
        /// <summary>
        /// This method transforms an Signal using FFT.
        /// </summary>
        /// <param name="signal">signal values</param>
        /// <returns>double-array of Magnitudes</returns>
        public double[] Transform(Int16[] signal);
    }
}
