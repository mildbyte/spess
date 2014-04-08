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
        int requiredTime;

        public Dictionary<Good, int> Input { get { return input; } }
        public Dictionary<Good, int> Output { get { return output; } }
        public int RequiredTime { get { return requiredTime; } }

        public ProductionRule(Dictionary<Good, int> input, Dictionary<Good, int> output, int requiredTime)
        {
            this.input = input; this.output = output;
            this.requiredTime = requiredTime;
        }

    }
}
