using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignalGo.Drawing.Shapes
{
    public struct Point
    {
        internal double _x;
        internal double _y;

        public static bool operator ==(Point point1, Point point2)
        {
            if (point1.X == point2.X)
                return point1.Y == point2.Y;
            return false;
        }

        public static bool operator !=(Point point1, Point point2)
        {
            return !(point1 == point2);
        }

        public static bool Equals(Point point1, Point point2)
        {
            if (point1.X.Equals(point2.X))
                return point1.Y.Equals(point2.Y);
            return false;
        }

        public override bool Equals(object o)
        {
            if (o == null || !(o is Point))
                return false;
            return Point.Equals(this, (Point)o);
        }

        public bool Equals(Point value)
        {
            return Point.Equals(this, value);
        }

        public override int GetHashCode()
        {
            double num = this.X;
            int hashCode1 = num.GetHashCode();
            num = this.Y;
            int hashCode2 = num.GetHashCode();
            return hashCode1 ^ hashCode2;
        }
        

        public double X
        {
            get
            {
                return this._x;
            }
            set
            {
                this._x = value;
            }
        }

        public double Y
        {
            get
            {
                return this._y;
            }
            set
            {
                this._y = value;
            }
        }
        

        public Point(double x, double y)
        {
            this._x = x;
            this._y = y;
        }

        public void Offset(double offsetX, double offsetY)
        {
            this._x = this._x + offsetX;
            this._y = this._y + offsetY;
        }

        public static Point operator +(Point point, Vector vector)
        {
            return new Point(point._x + vector._x, point._y + vector._y);
        }

        public static Point Add(Point point, Vector vector)
        {
            return new Point(point._x + vector._x, point._y + vector._y);
        }

        public static Point operator -(Point point, Vector vector)
        {
            return new Point(point._x - vector._x, point._y - vector._y);
        }

        public static Point Subtract(Point point, Vector vector)
        {
            return new Point(point._x - vector._x, point._y - vector._y);
        }

        public static Vector operator -(Point point1, Point point2)
        {
            return new Vector(point1._x - point2._x, point1._y - point2._y);
        }

        public static Vector Subtract(Point point1, Point point2)
        {
            return new Vector(point1._x - point2._x, point1._y - point2._y);
        }

        public static Point operator *(Point point, Matrix matrix)
        {
            return matrix.Transform(point);
        }

        public static Point Multiply(Point point, Matrix matrix)
        {
            return matrix.Transform(point);
        }

        public static explicit operator Size(Point point)
        {
            return new Size(Math.Abs(point._x), Math.Abs(point._y));
        }

        public static explicit operator Vector(Point point)
        {
            return new Vector(point._x, point._y);
        }
    }
}
