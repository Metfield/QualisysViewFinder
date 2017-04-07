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
    }
}
