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

        public double Distance(Vector v)
        {
            return Math.Sqrt((x - v.x) * (x - v.x) + (y - v.y) * (y - v.y) + (z - v.z) * (z - v.z));
        }

        public static Vector operator +(Vector v1, Vector v2) { return new Vector(v1.x + v2.x, v1.y + v2.y, v1.z + v2.z); }

        public static Vector operator -(Vector v1, Vector v2) { return new Vector(v1.x - v2.x, v1.y - v2.y, v1.z - v2.z); }

        public static Vector operator *(double c, Vector v) { return new Vector(c * v.x, c * v.y, c * v.z); }

        public static Vector operator *(Vector v, double c) { return c * v; }

        public double Magnitude() { return Math.Sqrt(x * x + y * y + z * z); }
    }
}
