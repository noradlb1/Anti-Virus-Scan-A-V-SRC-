using System;
using System.Windows;

namespace CircularProgressBar.Helpers
{
    public static class Utils
    {
        /// <summary>
        /// Converts a coordinate from the polar coordinate system to the cartesian coordinate system.
        /// </summary>
        /// <param name="angle"></param>
        /// <param name="radius"></param>
        /// <returns></returns>
        public static Point ComputeCartesianCoordinate(double angle, double radius)
        {
            // convert to radians
            double angleRad = (Math.PI / 180.0) * (angle - 90);

            double x = radius * Math.Cos(angleRad);
            double y = radius * Math.Sin(angleRad);
            Point p = new Point(x, y);
            return p;
        }

        public static Point Offset(this Point point, double X, double Y)
        {
            return new System.Windows.Point(point.X + X, point.Y + Y);
        }
    }
}
