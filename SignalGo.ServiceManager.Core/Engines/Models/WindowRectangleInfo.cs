using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace SignalGo.ServiceManager.Core.Engines.Models
{
    public class WindowRectangleInfo //: IDisposable
    {
        #region WindowsApiImports
        [DllImport("gdi32.dll", ExactSpelling = true, PreserveSig = true, SetLastError = true)]
        static extern IntPtr SelectObject(IntPtr hdc, IntPtr hgdiobj);

        [DllImport("gdi32.dll")]
        private static extern IntPtr CreateCompatibleBitmap(IntPtr hdc, int nWidth, int nHeight);

        [DllImport("gdi32.dll", SetLastError = true)]
        private static extern IntPtr CreateCompatibleDC(IntPtr hdc);

        [DllImport("gdi32.dll")]
        private static extern bool DeleteObject(IntPtr hObject);

        [DllImport("gdi32.dll")]
        private static extern IntPtr CreateBitmap(int nWidth, int nHeight, uint cPlanes, uint cBitsPerPel, IntPtr lpvBits);

        [DllImport("user32.dll")]
        private static extern IntPtr GetDC(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

        [DllImport("gdi32.dll")]
        private static extern bool BitBlt(IntPtr hdc, int nXDest, int nYDest, int nWidth, int nHeight, IntPtr hdcSrc, int nXSrc, int nYSrc, TernaryRasterOperationsEnum dwRop);

        [DllImport("user32.dll")]
        public static extern bool GetWindowRect(IntPtr hWnd, out WindowRectangleStruct lpRect);
        [DllImport("user32.dll")]
        public static extern bool PrintWindow(IntPtr hWnd, IntPtr hdcBlt, int nFlags);
        #endregion


        public static Bitmap CaptureWindowImage(IntPtr hWnd, Rectangle wndRect)
        {
            IntPtr hWndDc = GetDC(hWnd);
            IntPtr hMemDc = CreateCompatibleDC(hWndDc);
            IntPtr hBitmap = CreateCompatibleBitmap(hWndDc, wndRect.Width, wndRect.Height);
            SelectObject(hMemDc, hBitmap);

            BitBlt(hMemDc, 0, 0, wndRect.Width, wndRect.Height, hWndDc, 0, 0, TernaryRasterOperationsEnum.SRCPAINT);
            Bitmap bitmap = Bitmap.FromHbitmap(hBitmap);

            DeleteObject(hBitmap);
            ReleaseDC(hWnd, hWndDc);
            ReleaseDC(IntPtr.Zero, hMemDc);
            return bitmap;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct WindowRectangleStruct
        {
            private int _Left;
            private int _Top;
            private int _Right;
            private int _Bottom;

            public WindowRectangleStruct(WindowRectangleStruct Rectangle) : this(
                Rectangle.Left,
                Rectangle.Top,
                Rectangle.Right,
                Rectangle.Bottom)
            {

            }
            public WindowRectangleStruct(int Left, int Top, int Right, int Bottom)
            {
                _Left = Left;
                _Top = Top;
                _Right = Right;
                _Bottom = Bottom;
            }

            public int X
            {
                get { return _Left; }
                set { _Left = value; }
            }
            public int Y
            {
                get { return _Top; }
                set { _Top = value; }
            }
            public int Left
            {
                get { return _Left; }
                set { _Left = value; }
            }
            public int Top
            {
                get { return _Top; }
                set { _Top = value; }
            }
            public int Right
            {
                get { return _Right; }
                set { _Right = value; }
            }
            public int Bottom
            {
                get { return _Bottom; }
                set { _Bottom = value; }
            }
            public int Height
            {
                get { return _Bottom - _Top; }
                set { _Bottom = value + _Top; }
            }
            public int Width
            {
                get { return _Right - _Left; }
                set { _Right = value + _Left; }
            }
            public Point Location
            {
                get { return new Point(Left, Top); }
                set
                {
                    _Left = value.X;
                    _Top = value.Y;
                }
            }
            public Size Size
            {
                get { return new Size(Width, Height); }
                set
                {
                    _Right = value.Width + _Left;
                    _Bottom = value.Height + _Top;
                }
            }
            public static implicit operator Rectangle(WindowRectangleStruct Rectangle)
            {
                return new Rectangle(Rectangle.Left, Rectangle.Top, Rectangle.Width, Rectangle.Height);
            }
            public static implicit operator WindowRectangleStruct(Rectangle Rectangle)
            {
                return new WindowRectangleStruct(Rectangle.Left, Rectangle.Top, Rectangle.Right, Rectangle.Bottom);
            }
            public static bool operator ==(WindowRectangleStruct Rectangle1, WindowRectangleStruct Rectangle2)
            {
                return Rectangle1.Equals(Rectangle2);
            }
            public static bool operator !=(WindowRectangleStruct Rectangle1, WindowRectangleStruct Rectangle2)
            {
                return !Rectangle1.Equals(Rectangle2);
            }

            public override string ToString()
            {
                return "{Left: " + _Left + "; " + "Top: " + _Top + "; Right: " + _Right + "; Bottom: " + _Bottom + "}";
            }

            public override int GetHashCode()
            {
                return ToString().GetHashCode();
            }

            public bool Equals(WindowRectangleStruct Rectangle)
            {
                return Rectangle.Left == _Left && Rectangle.Top == _Top && Rectangle.Right == _Right && Rectangle.Bottom == _Bottom;
            }

            public override bool Equals(object Object)
            {
                if (Object is WindowRectangleStruct)
                {
                    return Equals((WindowRectangleStruct)Object);
                }
                else if (Object is Rectangle)
                {
                    return Equals(new WindowRectangleStruct((Rectangle)Object));
                }

                return false;
            }
        }


        #region Disposing
        //private bool _disposed;
        //protected virtual void Dispose(bool disposing)
        //{
        //    if (!_disposed)
        //    {
        //        if (disposing)
        //        {
        //        }
        //        _disposed = true;
        //    }
        //}

        //~WindowRectangleInfo()
        //{
        //    Dispose(false);
        //}

        //public void Dispose()
        //{
        //    Dispose(true);
        //GC.SuppressFinalize(this);
        //}
        #endregion
    }
}