using System;
using System.Collections.Generic;
using System.Windows;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SeamlessRepeater.Helper
{
    public static class GeometryUtilities
    {
        public static double Distance(this Point point0, Point point1)
        {
            return Math.Sqrt((point0.X - point1.X).Square() + (point0.Y - point1.Y).Square());
        }

        public static double Square(this double number)
        {
            return number * number;
        }
    }
}
