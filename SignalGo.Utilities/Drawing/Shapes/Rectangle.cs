using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignalGo.Drawing.Shapes
{
    public struct Rectangle
    {
        private static readonly Rectangle s_empty = Rectangle.CreateEmptyRect();
        internal double _x;
        internal double _y;
        internal double _width;
        internal double _height;

        public static bool operator ==(Rectangle rect1, Rectangle rect2)
        {
            if (rect1.X == rect2.X && rect1.Y == rect2.Y && rect1.Width == rect2.Width)
                return rect1.Height == rect2.Height;
            return false;
        }

        public static bool operator !=(Rectangle rect1, Rectangle rect2)
        {
            return !(rect1 == rect2);
        }

        public static bool Equals(Rectangle rect1, Rectangle rect2)
        {
            if (rect1.IsEmpty)
                return rect2.IsEmpty;
            if (rect1.X.Equals(rect2.X) && rect1.Y.Equals(rect2.Y) && rect1.Width.Equals(rect2.Width))
                return rect1.Height.Equals(rect2.Height);
            return false;
        }

        public override bool Equals(object o)
        {
            if (o == null || !(o is Rectangle))
                return false;
            return Rectangle.Equals(this, (Rectangle)o);
        }

        public bool Equals(Rectangle value)
        {
            return Rectangle.Equals(this, value);
        }

        public override int GetHashCode()
        {
            if (this.IsEmpty)
                return 0;
            int hashCode1 = this.X.GetHashCode();
            double num1 = this.Y;
            int hashCode2 = num1.GetHashCode();
            int num2 = hashCode1 ^ hashCode2;
            num1 = this.Width;
            int hashCode3 = num1.GetHashCode();
            int num3 = num2 ^ hashCode3;
            num1 = this.Height;
            int hashCode4 = num1.GetHashCode();
            return num3 ^ hashCode4;
        }
        
        public Rectangle(Point location, Size size)
        {
            if (size.IsEmpty)
            {
                this = Rectangle.s_empty;
            }
            else
            {
                this._x = location._x;
                this._y = location._y;
                this._width = size._width;
                this._height = size._height;
            }
        }

        public Rectangle(double x, double y, double width, double height)
        {
            if (width < 0.0 || height < 0.0)
                throw new ArgumentException("Size_WidthAndHeightCannotBeNegative");
            this._x = x;
            this._y = y;
            this._width = width;
            this._height = height;
        }

        public Rectangle(Point point1, Point point2)
        {
            this._x = Math.Min(point1._x, point2._x);
            this._y = Math.Min(point1._y, point2._y);
            this._width = Math.Max(Math.Max(point1._x, point2._x) - this._x, 0.0);
            this._height = Math.Max(Math.Max(point1._y, point2._y) - this._y, 0.0);
        }

        public Rectangle(Point point, Vector vector)
        {
            this = new Rectangle(point, point + vector);
        }

        public Rectangle(Size size)
        {
            if (size.IsEmpty)
            {
                this = Rectangle.s_empty;
            }
            else
            {
                this._x = this._y = 0.0;
                this._width = size.Width;
                this._height = size.Height;
            }
        }

        public static Rectangle Empty
        {
            get
            {
                return Rectangle.s_empty;
            }
        }

        public bool IsEmpty
        {
            get
            {
                return this._width < 0.0;
            }
        }

        public Point Location
        {
            get
            {
                return new Point(this._x, this._y);
            }
            set
            {
                if (this.IsEmpty)
                    throw new InvalidOperationException("Rect_CannotModifyEmptyRect");
                this._x = value._x;
                this._y = value._y;
            }
        }

        public Size Size
        {
            get
            {
                if (this.IsEmpty)
                    return Size.Empty;
                return new Size(this._width, this._height);
            }
            set
            {
                if (value.IsEmpty)
                {
                    this = Rectangle.s_empty;
                }
                else
                {
                    if (this.IsEmpty)
                        throw new InvalidOperationException("Rect_CannotModifyEmptyRect");
                    this._width = value._width;
                    this._height = value._height;
                }
            }
        }

        public double X
        {
            get
            {
                return this._x;
            }
            set
            {
                if (this.IsEmpty)
                    throw new InvalidOperationException("Rect_CannotModifyEmptyRect");
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
                if (this.IsEmpty)
                    throw new InvalidOperationException("Rect_CannotModifyEmptyRect");
                this._y = value;
            }
        }

        public double Width
        {
            get
            {
                return this._width;
            }
            set
            {
                if (this.IsEmpty)
                    throw new InvalidOperationException("Rect_CannotModifyEmptyRect");
                if (value < 0.0)
                    throw new ArgumentException("Size_WidthCannotBeNegative");
                this._width = value;
            }
        }

        public double Height
        {
            get
            {
                return this._height;
            }
            set
            {
                if (this.IsEmpty)
                    throw new InvalidOperationException("Rect_CannotModifyEmptyRect");
                if (value < 0.0)
                    throw new ArgumentException("Size_HeightCannotBeNegative");
                this._height = value;
            }
        }

        public double Left
        {
            get
            {
                return this._x;
            }
        }

        public double Top
        {
            get
            {
                return this._y;
            }
        }

        public double Right
        {
            get
            {
                if (this.IsEmpty)
                    return double.NegativeInfinity;
                return this._x + this._width;
            }
        }

        public double Bottom
        {
            get
            {
                if (this.IsEmpty)
                    return double.NegativeInfinity;
                return this._y + this._height;
            }
        }

        public Point TopLeft
        {
            get
            {
                return new Point(this.Left, this.Top);
            }
        }

        public Point TopRight
        {
            get
            {
                return new Point(this.Right, this.Top);
            }
        }

        public Point BottomLeft
        {
            get
            {
                return new Point(this.Left, this.Bottom);
            }
        }

        public Point BottomRight
        {
            get
            {
                return new Point(this.Right, this.Bottom);
            }
        }

        public bool Contains(Point point)
        {
            return this.Contains(point._x, point._y);
        }

        public bool Contains(double x, double y)
        {
            if (this.IsEmpty)
                return false;
            return this.ContainsInternal(x, y);
        }

        public bool Contains(Rectangle rect)
        {
            if (this.IsEmpty || rect.IsEmpty || (this._x > rect._x || this._y > rect._y) || this._x + this._width < rect._x + rect._width)
                return false;
            return this._y + this._height >= rect._y + rect._height;
        }

        public bool IntersectsWith(Rectangle rect)
        {
            if (this.IsEmpty || rect.IsEmpty || (rect.Left > this.Right || rect.Right < this.Left) || rect.Top > this.Bottom)
                return false;
            return rect.Bottom >= this.Top;
        }

        public void Intersect(Rectangle rect)
        {
            if (!this.IntersectsWith(rect))
            {
                this = Rectangle.Empty;
            }
            else
            {
                double num1 = Math.Max(this.Left, rect.Left);
                double num2 = Math.Max(this.Top, rect.Top);
                this._width = Math.Max(Math.Min(this.Right, rect.Right) - num1, 0.0);
                this._height = Math.Max(Math.Min(this.Bottom, rect.Bottom) - num2, 0.0);
                this._x = num1;
                this._y = num2;
            }
        }

        public static Rectangle Intersect(Rectangle rect1, Rectangle rect2)
        {
            rect1.Intersect(rect2);
            return rect1;
        }

        public void Union(Rectangle rect)
        {
            if (this.IsEmpty)
            {
                this = rect;
            }
            else
            {
                if (rect.IsEmpty)
                    return;
                double num1 = Math.Min(this.Left, rect.Left);
                double num2 = Math.Min(this.Top, rect.Top);
                this._width = rect.Width == double.PositiveInfinity || this.Width == double.PositiveInfinity ? double.PositiveInfinity : Math.Max(Math.Max(this.Right, rect.Right) - num1, 0.0);
                this._height = rect.Height == double.PositiveInfinity || this.Height == double.PositiveInfinity ? double.PositiveInfinity : Math.Max(Math.Max(this.Bottom, rect.Bottom) - num2, 0.0);
                this._x = num1;
                this._y = num2;
            }
        }

        public static Rectangle Union(Rectangle rect1, Rectangle rect2)
        {
            rect1.Union(rect2);
            return rect1;
        }

        public void Union(Point point)
        {
            this.Union(new Rectangle(point, point));
        }

        public static Rectangle Union(Rectangle rect, Point point)
        {
            rect.Union(new Rectangle(point, point));
            return rect;
        }

        public void Offset(Vector offsetVector)
        {
            if (this.IsEmpty)
                throw new InvalidOperationException("Rect_CannotCallMethod");
            this._x = this._x + offsetVector._x;
            this._y = this._y + offsetVector._y;
        }

        public void Offset(double offsetX, double offsetY)
        {
            if (this.IsEmpty)
                throw new InvalidOperationException("Rect_CannotCallMethod");
            this._x = this._x + offsetX;
            this._y = this._y + offsetY;
        }

        public static Rectangle Offset(Rectangle rect, Vector offsetVector)
        {
            rect.Offset(offsetVector.X, offsetVector.Y);
            return rect;
        }

        public static Rectangle Offset(Rectangle rect, double offsetX, double offsetY)
        {
            rect.Offset(offsetX, offsetY);
            return rect;
        }

        public void Inflate(Size size)
        {
            this.Inflate(size._width, size._height);
        }

        public void Inflate(double width, double height)
        {
            if (this.IsEmpty)
                throw new InvalidOperationException("Rect_CannotCallMethod");
            this._x = this._x - width;
            this._y = this._y - height;
            this._width = this._width + width;
            this._width = this._width + width;
            this._height = this._height + height;
            this._height = this._height + height;
            if (this._width >= 0.0 && this._height >= 0.0)
                return;
            this = Rectangle.s_empty;
        }

        public static Rectangle Inflate(Rectangle rect, Size size)
        {
            rect.Inflate(size._width, size._height);
            return rect;
        }

        public static Rectangle Inflate(Rectangle rect, double width, double height)
        {
            rect.Inflate(width, height);
            return rect;
        }

        public static Rectangle Transform(Rectangle rect, Matrix matrix)
        {
            MatrixUtil.TransformRect(ref rect, ref matrix);
            return rect;
        }

        public void Transform(Matrix matrix)
        {
            MatrixUtil.TransformRect(ref this, ref matrix);
        }

        public void Scale(double scaleX, double scaleY)
        {
            if (this.IsEmpty)
                return;
            this._x = this._x * scaleX;
            this._y = this._y * scaleY;
            this._width = this._width * scaleX;
            this._height = this._height * scaleY;
            if (scaleX < 0.0)
            {
                this._x = this._x + this._width;
                this._width = this._width * -1.0;
            }
            if (scaleY >= 0.0)
                return;
            this._y = this._y + this._height;
            this._height = this._height * -1.0;
        }

        private bool ContainsInternal(double x, double y)
        {
            if (x >= this._x && x - this._width <= this._x && y >= this._y)
                return y - this._height <= this._y;
            return false;
        }

        private static Rectangle CreateEmptyRect()
        {
            return new Rectangle()
            {
                _x = double.PositiveInfinity,
                _y = double.PositiveInfinity,
                _width = double.NegativeInfinity,
                _height = double.NegativeInfinity
            };
        }
    }
}
