using System;
using System.Collections.Generic;
using System.Text;

namespace Arqus.Helpers
{
    public static class DataOperations
    {
        /// <summary>
        /// Transforms data from one range to another.
        /// </summary>              
        public static float ConvertRange(float originalStart, float originalEnd, // original range
                                      float newStart, float newEnd, // desired range
                                      float value) // value to convert
        {
            float scale = (newEnd - newStart) / (originalEnd - originalStart);
            return (newStart + ((value - originalStart) * scale));
        }


        public static bool IsPinching(ref int x1, ref int x2, ref int y1, ref int y2)
        {
            return (x1 * x2 < 0) || (y1 * y2 < 0) ? true : false;
        }

        public static double GetDistance2D(float x1, float x2, float y1, float y2)
        {
            float deltaX = Math.Abs(x1 - x2);
            float deltaY = Math.Abs(y1 - y2);

            return Math.Sqrt(Math.Pow(deltaX, 2.0f) + Math.Pow(deltaY, 2.0f));
        }
    }
}
