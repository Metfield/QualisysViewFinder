using System;
using System.Collections.Generic;
using System.Text;

namespace Arqus.Helpers
{
    public static class DataOperations
    {
        /// <summary>
        /// Transforms data from one range to another
        /// </summary>
        /// <param name="originalStart">original range start</param>
        /// <param name="originalEnd">original range end</param>
        /// <param name="newStart">new range start</param>
        /// <param name="newEnd">new range end</param>
        /// <param name="value">the value to be converted into a new range</param>
        /// <returns></returns>
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

        const double DEGREE_TO_RADIANS = Math.PI / 180;

        /// <summary>
        /// Calculates the distance away from the camera where the frustrum has a given width
        /// </summary>
        /// <param name="width">width of the frustrum at a certain distance</param>
        /// <param name="aspectRatio">aspect ratio of the frustrum</param>
        /// <param name="fov">field of view of the frustrum</param>
        /// <returns></returns>
        public static double GetDistanceForFrustrumWidth(double width, double aspectRatio, double fov)
        {
            return width / aspectRatio * 0.5f / Math.Tan(fov * DEGREE_TO_RADIANS * 0.5f );
        }
        
        public static double GetDistanceForFrustrumHeight(double height, double fov)
        {
            return height * 0.5f / Math.Tan(fov * DEGREE_TO_RADIANS * 0.5f);
        }

    }
}
