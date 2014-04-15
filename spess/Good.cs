using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace spess
{
    public class Good
    {
        string name;
        string description;
        int size;

        public string Name { get { return name; } }
        public string Description { get { return description; } }
        public int Size { get { return size; } }

        public Good(string name, string description, int size)
        {
            this.name = name; this.description = description; this.size = size;
        }
    }
}
