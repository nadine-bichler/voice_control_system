///
/// @file    IFFTTransformer
/// @brief   Interface for FFTTransformer
/// @author  Bazai Donart
/// @date    24.06.2025
///
///

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
        public double[] Transform(Int16[] signal);
    }
}
