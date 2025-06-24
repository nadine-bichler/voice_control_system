/** 
* @file IFFTTransformer.cs 
* @brief File is interface.
* @author Donart Bazaj 
* @date 16.05.2025 
* 
* Additional Infos: none
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Spritzgusslogik
{
    public  interface IFFTTransformer
    {

        /** 
       * @fn double[] Transform(Int16[] signal)
       * @brief Transforms that signal using FFT. 
       * @param signal , signal Values 
       * @return double [] , double-array of Magnitudes 
       * @author Donart Bazaj
       * @date 09.05.2025 
       * 
       * This method transforms an Signal using FFT.
       * 
       */
        public double[] Transform(Int16[] signal);
    }
}
