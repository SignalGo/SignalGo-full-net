using SignalGo.Drawing.Shapes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignalGo.Drawing.Utilities
{
    public static class RectangleUtil
    {
        /// <summary>
        /// تقسیم یک مختصات عرضی
        /// </summary>
        /// <param name="rect"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static Rectangle[] DevidePoints(Rectangle rect, double length)
        {
            var hRec = rect.Width / length;
            var wRec = rect.Height / length;

            var xLen = Math.Ceiling(hRec);
            var yLen = Math.Ceiling(wRec);
            Rectangle[] smallerRects = new Rectangle[(int)Math.Ceiling(xLen * yLen)];
            for (int x = 0; x < xLen; x++)
            {
                for (int y = 0; y < yLen; y++)
                {
                    int i = (int)(x * yLen + y);
                    smallerRects[i] = new Rectangle(new Point(rect.X + x * length, rect.Y + y * length), new Point((rect.X + x * length) + length, (rect.Y + y * length) + length));
                }
            }
            return smallerRects;
        }

        public static int GetRectIndexByPoint(Point yourLocation, Rectangle baseRectangle, double baseLength)
        {
            var hRec = baseRectangle.Height / baseLength;

            var difX = (yourLocation.X - baseRectangle.X) / baseLength;
            var difY = (yourLocation.Y - baseRectangle.Y) / baseLength;

            var index = (int)difX * hRec + (int)difY;
            return (int)index;
        }


        public static List<int> GetListOfIndexes(int index, Rectangle baseRectangle, double baseLength)
        {
            List<int> items = new List<int>();

            var hRec = (baseRectangle.Width / baseLength);
            var wRec = (baseRectangle.Height / baseLength);
            var xLen = Math.Ceiling(hRec);
            var yLen = Math.Ceiling(wRec);
            var fullLength = (int)Math.Ceiling(xLen * yLen);

            items.Add(index);


            var top = GetTopOfIndex(index, baseRectangle, baseLength, (int)hRec, (int)wRec, fullLength);
            if (top.HasValue)
            {
                items.Add(top.Value);

                var topRight = GetRightOfIndex(top.Value, baseRectangle, baseLength, (int)hRec, (int)wRec, fullLength);
                if (topRight.HasValue)
                    items.Add(topRight.Value);

                var topLeft = GetLeftOfIndex(top.Value, baseRectangle, baseLength, (int)hRec, (int)wRec, fullLength);
                if (topLeft.HasValue)
                    items.Add(topLeft.Value);
            }

            var bot = GetBottomOfIndex(index, baseRectangle, baseLength, (int)hRec, (int)wRec, fullLength);
            if (bot.HasValue)
            {
                items.Add(bot.Value);

                var botRight = GetRightOfIndex(bot.Value, baseRectangle, baseLength, (int)hRec, (int)wRec, fullLength);
                if (botRight.HasValue)
                    items.Add(botRight.Value);

                var botLeft = GetLeftOfIndex(bot.Value, baseRectangle, baseLength, (int)hRec, (int)wRec, fullLength);
                if (botLeft.HasValue)
                    items.Add(botLeft.Value);
            }

            var left = GetLeftOfIndex(index, baseRectangle, baseLength, (int)hRec, (int)wRec, fullLength);
            if (left.HasValue)
                items.Add(left.Value);

            var right = GetRightOfIndex(index, baseRectangle, baseLength, (int)hRec, (int)wRec, fullLength);

            if (right.HasValue)
                items.Add(right.Value);

            return items;
        }

        static int? GetTopOfIndex(int index, Rectangle baseRectangle, double baseLength, int hRec, int wRec, int fullLength)
        {
            int top = index - 1;
            if (top >= 0 && index % hRec != 0)
                return top;
            return null;
        }

        static int? GetBottomOfIndex(int index, Rectangle baseRectangle, double baseLength, int hRec, int wRec, int fullLength)
        {
            int bot = index + 1;
            if (bot < fullLength && index % wRec != wRec - 1)
                return bot;
            return null;
        }

        static int? GetLeftOfIndex(int index, Rectangle baseRectangle, double baseLength, int hRec, int wRec, int fullLength)
        {
            int left = index - (int)hRec;
            if (left >= 0)
                return left;
            return null;
        }

        static int? GetRightOfIndex(int index, Rectangle baseRectangle, double baseLength, int hRec, int wRec, int fullLength)
        {
            int right = index + (int)hRec;
            if (right < fullLength)
                return right;
            return null;
        }
    }
}
