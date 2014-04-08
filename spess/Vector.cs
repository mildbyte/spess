using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace spess
{
    class Vector
    {
        double x, y, z;
        public double X { get { return x; } }
        public double Y { get { return y; } }
        public double Z { get { return z; } }

        public Vector(double x, double y, double z)
        {
            this.x = x; this.y = y; this.z = z;
        }
    }
}
