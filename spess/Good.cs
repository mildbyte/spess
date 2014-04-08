using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace spess
{
    class Good
    {
        string name;
        string description;

        public string Name { get { return name; } }
        public string Description { get { return description; } }

        public Good(string name, string description)
        {
            this.name = name; this.description = description;
        }
    }
}
