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

        public void setAlgorithm(IFFTTransformer newAlgo) { 
            TransformerAlgo = newAlgo;
        }

        public double[] Transform(Int16[] signal) { 
        
            if(TransformerAlgo == null)
            {
                throw new ArgumentNullException("Transformer Algorithm is NULL");
            }
            return TransformerAlgo.Transform(signal);
        }
    }
}
