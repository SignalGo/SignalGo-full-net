using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignalGo.Drawing.Shapes
{
    [Flags]
    internal enum MatrixTypes
    {
        TRANSFORM_IS_IDENTITY = 0,
        TRANSFORM_IS_TRANSLATION = 1,
        TRANSFORM_IS_SCALING = 2,
        TRANSFORM_IS_UNKNOWN = 4,
    }
    public struct Matrix
    {
        private static Matrix s_identity = Matrix.CreateIdentity();
        private const int c_identityHashCode = 0;
        internal double _m11;
        internal double _m12;
        internal double _m21;
        internal double _m22;
        internal double _offsetX;
        internal double _offsetY;
        internal MatrixTypes _type;
        internal int _padding;

        public Matrix(double m11, double m12, double m21, double m22, double offsetX, double offsetY)
        {
            this._m11 = m11;
            this._m12 = m12;
            this._m21 = m21;
            this._m22 = m22;
            this._offsetX = offsetX;
            this._offsetY = offsetY;
            this._type = MatrixTypes.TRANSFORM_IS_UNKNOWN;
            this._padding = 0;
            this.DeriveMatrixType();
        }

        public static Matrix Identity
        {
            get
            {
                return Matrix.s_identity;
            }
        }

        public void SetIdentity()
        {
            this._type = MatrixTypes.TRANSFORM_IS_IDENTITY;
        }

        public bool IsIdentity
        {
            get
            {
                if (this._type == MatrixTypes.TRANSFORM_IS_IDENTITY)
                    return true;
                if (this._m11 == 1.0 && this._m12 == 0.0 && (this._m21 == 0.0 && this._m22 == 1.0) && this._offsetX == 0.0)
                    return this._offsetY == 0.0;
                return false;
            }
        }

        public static Matrix operator *(Matrix trans1, Matrix trans2)
        {
            MatrixUtil.MultiplyMatrix(ref trans1, ref trans2);
            return trans1;
        }

        public static Matrix Multiply(Matrix trans1, Matrix trans2)
        {
            MatrixUtil.MultiplyMatrix(ref trans1, ref trans2);
            return trans1;
        }

        public void Append(Matrix matrix)
        {
            this = this * matrix;
        }

        public void Prepend(Matrix matrix)
        {
            this = matrix * this;
        }

        public void Rotate(double angle)
        {
            angle %= 360.0;
            this = this * Matrix.CreateRotationRadians(angle * (Math.PI / 180.0));
        }

        public void RotatePrepend(double angle)
        {
            angle %= 360.0;
            this = Matrix.CreateRotationRadians(angle * (Math.PI / 180.0)) * this;
        }

        public void RotateAt(double angle, double centerX, double centerY)
        {
            angle %= 360.0;
            this = this * Matrix.CreateRotationRadians(angle * (Math.PI / 180.0), centerX, centerY);
        }

        public void RotateAtPrepend(double angle, double centerX, double centerY)
        {
            angle %= 360.0;
            this = Matrix.CreateRotationRadians(angle * (Math.PI / 180.0), centerX, centerY) * this;
        }

        public void Scale(double scaleX, double scaleY)
        {
            this = this * Matrix.CreateScaling(scaleX, scaleY);
        }

        public void ScalePrepend(double scaleX, double scaleY)
        {
            this = Matrix.CreateScaling(scaleX, scaleY) * this;
        }

        public void ScaleAt(double scaleX, double scaleY, double centerX, double centerY)
        {
            this = this * Matrix.CreateScaling(scaleX, scaleY, centerX, centerY);
        }

        public void ScaleAtPrepend(double scaleX, double scaleY, double centerX, double centerY)
        {
            this = Matrix.CreateScaling(scaleX, scaleY, centerX, centerY) * this;
        }

        public void Skew(double skewX, double skewY)
        {
            skewX %= 360.0;
            skewY %= 360.0;
            this = this * Matrix.CreateSkewRadians(skewX * (Math.PI / 180.0), skewY * (Math.PI / 180.0));
        }

        public void SkewPrepend(double skewX, double skewY)
        {
            skewX %= 360.0;
            skewY %= 360.0;
            this = Matrix.CreateSkewRadians(skewX * (Math.PI / 180.0), skewY * (Math.PI / 180.0)) * this;
        }

        public void Translate(double offsetX, double offsetY)
        {
            if (this._type == MatrixTypes.TRANSFORM_IS_IDENTITY)
                this.SetMatrix(1.0, 0.0, 0.0, 1.0, offsetX, offsetY, MatrixTypes.TRANSFORM_IS_TRANSLATION);
            else if (this._type == MatrixTypes.TRANSFORM_IS_UNKNOWN)
            {
                this._offsetX = this._offsetX + offsetX;
                this._offsetY = this._offsetY + offsetY;
            }
            else
            {
                this._offsetX = this._offsetX + offsetX;
                this._offsetY = this._offsetY + offsetY;
                this._type = this._type | MatrixTypes.TRANSFORM_IS_TRANSLATION;
            }
        }

        public void TranslatePrepend(double offsetX, double offsetY)
        {
            this = Matrix.CreateTranslation(offsetX, offsetY) * this;
        }

        public Point Transform(Point point)
        {
            Point point1 = point;
            this.MultiplyPoint(ref point1._x, ref point1._y);
            return point1;
        }

        public void Transform(Point[] points)
        {
            if (points == null)
                return;
            for (int index = 0; index < points.Length; ++index)
                this.MultiplyPoint(ref points[index]._x, ref points[index]._y);
        }

        public Vector Transform(Vector vector)
        {
            Vector vector1 = vector;
            this.MultiplyVector(ref vector1._x, ref vector1._y);
            return vector1;
        }

        public void Transform(Vector[] vectors)
        {
            if (vectors == null)
                return;
            for (int index = 0; index < vectors.Length; ++index)
                this.MultiplyVector(ref vectors[index]._x, ref vectors[index]._y);
        }

        public double Determinant
        {
            get
            {
                switch (this._type)
                {
                    case MatrixTypes.TRANSFORM_IS_IDENTITY:
                    case MatrixTypes.TRANSFORM_IS_TRANSLATION:
                        return 1.0;
                    case MatrixTypes.TRANSFORM_IS_SCALING:
                    case MatrixTypes.TRANSFORM_IS_TRANSLATION | MatrixTypes.TRANSFORM_IS_SCALING:
                        return this._m11 * this._m22;
                    default:
                        return this._m11 * this._m22 - this._m12 * this._m21;
                }
            }
        }

        public bool HasInverse
        {
            get
            {
                return !DoubleUtil.IsZero(this.Determinant);
            }
        }

        public void Invert()
        {
            double determinant = this.Determinant;
            if (DoubleUtil.IsZero(determinant))
                throw new InvalidOperationException("Transform_NotInvertible");
            switch (this._type)
            {
                case MatrixTypes.TRANSFORM_IS_IDENTITY:
                    break;
                case MatrixTypes.TRANSFORM_IS_TRANSLATION:
                    this._offsetX = -this._offsetX;
                    this._offsetY = -this._offsetY;
                    break;
                case MatrixTypes.TRANSFORM_IS_SCALING:
                    this._m11 = 1.0 / this._m11;
                    this._m22 = 1.0 / this._m22;
                    break;
                case MatrixTypes.TRANSFORM_IS_TRANSLATION | MatrixTypes.TRANSFORM_IS_SCALING:
                    this._m11 = 1.0 / this._m11;
                    this._m22 = 1.0 / this._m22;
                    this._offsetX = -this._offsetX * this._m11;
                    this._offsetY = -this._offsetY * this._m22;
                    break;
                default:
                    double num = 1.0 / determinant;
                    this.SetMatrix(this._m22 * num, -this._m12 * num, -this._m21 * num, this._m11 * num, (this._m21 * this._offsetY - this._offsetX * this._m22) * num, (this._offsetX * this._m12 - this._m11 * this._offsetY) * num, MatrixTypes.TRANSFORM_IS_UNKNOWN);
                    break;
            }
        }

        public double M11
        {
            get
            {
                if (this._type == MatrixTypes.TRANSFORM_IS_IDENTITY)
                    return 1.0;
                return this._m11;
            }
            set
            {
                if (this._type == MatrixTypes.TRANSFORM_IS_IDENTITY)
                {
                    this.SetMatrix(value, 0.0, 0.0, 1.0, 0.0, 0.0, MatrixTypes.TRANSFORM_IS_SCALING);
                }
                else
                {
                    this._m11 = value;
                    if (this._type == MatrixTypes.TRANSFORM_IS_UNKNOWN)
                        return;
                    this._type = this._type | MatrixTypes.TRANSFORM_IS_SCALING;
                }
            }
        }

        public double M12
        {
            get
            {
                if (this._type == MatrixTypes.TRANSFORM_IS_IDENTITY)
                    return 0.0;
                return this._m12;
            }
            set
            {
                if (this._type == MatrixTypes.TRANSFORM_IS_IDENTITY)
                {
                    this.SetMatrix(1.0, value, 0.0, 1.0, 0.0, 0.0, MatrixTypes.TRANSFORM_IS_UNKNOWN);
                }
                else
                {
                    this._m12 = value;
                    this._type = MatrixTypes.TRANSFORM_IS_UNKNOWN;
                }
            }
        }

        public double M21
        {
            get
            {
                if (this._type == MatrixTypes.TRANSFORM_IS_IDENTITY)
                    return 0.0;
                return this._m21;
            }
            set
            {
                if (this._type == MatrixTypes.TRANSFORM_IS_IDENTITY)
                {
                    this.SetMatrix(1.0, 0.0, value, 1.0, 0.0, 0.0, MatrixTypes.TRANSFORM_IS_UNKNOWN);
                }
                else
                {
                    this._m21 = value;
                    this._type = MatrixTypes.TRANSFORM_IS_UNKNOWN;
                }
            }
        }

        public double M22
        {
            get
            {
                if (this._type == MatrixTypes.TRANSFORM_IS_IDENTITY)
                    return 1.0;
                return this._m22;
            }
            set
            {
                if (this._type == MatrixTypes.TRANSFORM_IS_IDENTITY)
                {
                    this.SetMatrix(1.0, 0.0, 0.0, value, 0.0, 0.0, MatrixTypes.TRANSFORM_IS_SCALING);
                }
                else
                {
                    this._m22 = value;
                    if (this._type == MatrixTypes.TRANSFORM_IS_UNKNOWN)
                        return;
                    this._type = this._type | MatrixTypes.TRANSFORM_IS_SCALING;
                }
            }
        }

        public double OffsetX
        {
            get
            {
                if (this._type == MatrixTypes.TRANSFORM_IS_IDENTITY)
                    return 0.0;
                return this._offsetX;
            }
            set
            {
                if (this._type == MatrixTypes.TRANSFORM_IS_IDENTITY)
                {
                    this.SetMatrix(1.0, 0.0, 0.0, 1.0, value, 0.0, MatrixTypes.TRANSFORM_IS_TRANSLATION);
                }
                else
                {
                    this._offsetX = value;
                    if (this._type == MatrixTypes.TRANSFORM_IS_UNKNOWN)
                        return;
                    this._type = this._type | MatrixTypes.TRANSFORM_IS_TRANSLATION;
                }
            }
        }

        public double OffsetY
        {
            get
            {
                if (this._type == MatrixTypes.TRANSFORM_IS_IDENTITY)
                    return 0.0;
                return this._offsetY;
            }
            set
            {
                if (this._type == MatrixTypes.TRANSFORM_IS_IDENTITY)
                {
                    this.SetMatrix(1.0, 0.0, 0.0, 1.0, 0.0, value, MatrixTypes.TRANSFORM_IS_TRANSLATION);
                }
                else
                {
                    this._offsetY = value;
                    if (this._type == MatrixTypes.TRANSFORM_IS_UNKNOWN)
                        return;
                    this._type = this._type | MatrixTypes.TRANSFORM_IS_TRANSLATION;
                }
            }
        }

        internal void MultiplyVector(ref double x, ref double y)
        {
            switch (this._type)
            {
                case MatrixTypes.TRANSFORM_IS_IDENTITY:
                    break;
                case MatrixTypes.TRANSFORM_IS_TRANSLATION:
                    break;
                case MatrixTypes.TRANSFORM_IS_SCALING:
                case MatrixTypes.TRANSFORM_IS_TRANSLATION | MatrixTypes.TRANSFORM_IS_SCALING:
                    x = x * this._m11;
                    y = y * this._m22;
                    break;
                default:
                    double num1 = y * this._m21;
                    double num2 = x * this._m12;
                    x = x * this._m11;
                    x = x + num1;
                    y = y * this._m22;
                    y = y + num2;
                    break;
            }
        }

        internal void MultiplyPoint(ref double x, ref double y)
        {
            switch (this._type)
            {
                case MatrixTypes.TRANSFORM_IS_IDENTITY:
                    break;
                case MatrixTypes.TRANSFORM_IS_TRANSLATION:
                    x = x + this._offsetX;
                    y = y + this._offsetY;
                    break;
                case MatrixTypes.TRANSFORM_IS_SCALING:
                    x = x * this._m11;
                    y = y * this._m22;
                    break;
                case MatrixTypes.TRANSFORM_IS_TRANSLATION | MatrixTypes.TRANSFORM_IS_SCALING:
                    x = x * this._m11;
                    x = x + this._offsetX;
                    y = y * this._m22;
                    y = y + this._offsetY;
                    break;
                default:
                    double num1 = y * this._m21 + this._offsetX;
                    double num2 = x * this._m12 + this._offsetY;
                    x = x * this._m11;
                    x = x + num1;
                    y = y * this._m22;
                    y = y + num2;
                    break;
            }
        }

        internal static Matrix CreateRotationRadians(double angle)
        {
            return Matrix.CreateRotationRadians(angle, 0.0, 0.0);
        }

        internal static Matrix CreateRotationRadians(double angle, double centerX, double centerY)
        {
            Matrix matrix = new Matrix();
            double m12 = Math.Sin(angle);
            double num = Math.Cos(angle);
            double offsetX = centerX * (1.0 - num) + centerY * m12;
            double offsetY = centerY * (1.0 - num) - centerX * m12;
            matrix.SetMatrix(num, m12, -m12, num, offsetX, offsetY, MatrixTypes.TRANSFORM_IS_UNKNOWN);
            return matrix;
        }

        internal static Matrix CreateScaling(double scaleX, double scaleY, double centerX, double centerY)
        {
            Matrix matrix = new Matrix();
            matrix.SetMatrix(scaleX, 0.0, 0.0, scaleY, centerX - scaleX * centerX, centerY - scaleY * centerY, MatrixTypes.TRANSFORM_IS_TRANSLATION | MatrixTypes.TRANSFORM_IS_SCALING);
            return matrix;
        }

        internal static Matrix CreateScaling(double scaleX, double scaleY)
        {
            Matrix matrix = new Matrix();
            matrix.SetMatrix(scaleX, 0.0, 0.0, scaleY, 0.0, 0.0, MatrixTypes.TRANSFORM_IS_SCALING);
            return matrix;
        }

        internal static Matrix CreateSkewRadians(double skewX, double skewY)
        {
            Matrix matrix = new Matrix();
            matrix.SetMatrix(1.0, Math.Tan(skewY), Math.Tan(skewX), 1.0, 0.0, 0.0, MatrixTypes.TRANSFORM_IS_UNKNOWN);
            return matrix;
        }

        internal static Matrix CreateTranslation(double offsetX, double offsetY)
        {
            Matrix matrix = new Matrix();
            matrix.SetMatrix(1.0, 0.0, 0.0, 1.0, offsetX, offsetY, MatrixTypes.TRANSFORM_IS_TRANSLATION);
            return matrix;
        }

        private static Matrix CreateIdentity()
        {
            Matrix matrix = new Matrix();
            matrix.SetMatrix(1.0, 0.0, 0.0, 1.0, 0.0, 0.0, MatrixTypes.TRANSFORM_IS_IDENTITY);
            return matrix;
        }

        private void SetMatrix(double m11, double m12, double m21, double m22, double offsetX, double offsetY, MatrixTypes type)
        {
            this._m11 = m11;
            this._m12 = m12;
            this._m21 = m21;
            this._m22 = m22;
            this._offsetX = offsetX;
            this._offsetY = offsetY;
            this._type = type;
        }

        private void DeriveMatrixType()
        {
            this._type = MatrixTypes.TRANSFORM_IS_IDENTITY;
            if (this._m21 != 0.0 || this._m12 != 0.0)
            {
                this._type = MatrixTypes.TRANSFORM_IS_UNKNOWN;
            }
            else
            {
                if (this._m11 != 1.0 || this._m22 != 1.0)
                    this._type = MatrixTypes.TRANSFORM_IS_SCALING;
                if (this._offsetX != 0.0 || this._offsetY != 0.0)
                    this._type = this._type | MatrixTypes.TRANSFORM_IS_TRANSLATION;
                if ((this._type & (MatrixTypes.TRANSFORM_IS_TRANSLATION | MatrixTypes.TRANSFORM_IS_SCALING)) != MatrixTypes.TRANSFORM_IS_IDENTITY)
                    return;
                this._type = MatrixTypes.TRANSFORM_IS_IDENTITY;
            }
        }
        
        private void Debug_CheckType()
        {
            switch (this._type)
            {
            }
        }

        private bool IsDistinguishedIdentity
        {
            get
            {
                return this._type == MatrixTypes.TRANSFORM_IS_IDENTITY;
            }
        }

        public static bool operator ==(Matrix matrix1, Matrix matrix2)
        {
            if (matrix1.IsDistinguishedIdentity || matrix2.IsDistinguishedIdentity)
                return matrix1.IsIdentity == matrix2.IsIdentity;
            if (matrix1.M11 == matrix2.M11 && matrix1.M12 == matrix2.M12 && (matrix1.M21 == matrix2.M21 && matrix1.M22 == matrix2.M22) && matrix1.OffsetX == matrix2.OffsetX)
                return matrix1.OffsetY == matrix2.OffsetY;
            return false;
        }

        public static bool operator !=(Matrix matrix1, Matrix matrix2)
        {
            return !(matrix1 == matrix2);
        }

        public static bool Equals(Matrix matrix1, Matrix matrix2)
        {
            if (matrix1.IsDistinguishedIdentity || matrix2.IsDistinguishedIdentity)
                return matrix1.IsIdentity == matrix2.IsIdentity;
            if (matrix1.M11.Equals(matrix2.M11) && matrix1.M12.Equals(matrix2.M12) && (matrix1.M21.Equals(matrix2.M21) && matrix1.M22.Equals(matrix2.M22)) && matrix1.OffsetX.Equals(matrix2.OffsetX))
                return matrix1.OffsetY.Equals(matrix2.OffsetY);
            return false;
        }

        public override bool Equals(object o)
        {
            if (o == null || !(o is Matrix))
                return false;
            return Matrix.Equals(this, (Matrix)o);
        }

        public bool Equals(Matrix value)
        {
            return Matrix.Equals(this, value);
        }

        public override int GetHashCode()
        {
            if (this.IsDistinguishedIdentity)
                return 0;
            int hashCode1 = this.M11.GetHashCode();
            double num1 = this.M12;
            int hashCode2 = num1.GetHashCode();
            int num2 = hashCode1 ^ hashCode2;
            num1 = this.M21;
            int hashCode3 = num1.GetHashCode();
            int num3 = num2 ^ hashCode3;
            num1 = this.M22;
            int hashCode4 = num1.GetHashCode();
            int num4 = num3 ^ hashCode4;
            num1 = this.OffsetX;
            int hashCode5 = num1.GetHashCode();
            int num5 = num4 ^ hashCode5;
            num1 = this.OffsetY;
            int hashCode6 = num1.GetHashCode();
            return num5 ^ hashCode6;
        }
        
    }
}
