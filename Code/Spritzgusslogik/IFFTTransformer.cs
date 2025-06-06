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
