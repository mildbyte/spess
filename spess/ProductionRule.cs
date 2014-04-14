using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace spess
{
    class ProductionRule
    {
        Dictionary<Good, int> input;
        Dictionary<Good, int> output;
        float requiredTime;

        public Dictionary<Good, int> Input { get { return input; } }
        public Dictionary<Good, int> Output { get { return output; } }
        public float RequiredTime { get { return requiredTime; } }

        public ProductionRule(Dictionary<Good, int> input, Dictionary<Good, int> output, float requiredTime)
        {
            this.input = input; this.output = output;
            this.requiredTime = requiredTime;
        }

    }
}
