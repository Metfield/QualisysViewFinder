using System;
using System.Collections.Generic;
using System.Text;

namespace Arqus.Helpers
{
    public static class Constants
    {        
        /// <summary>
        /// Camera settings DRAWER string constants  
        /// </summary>        
        // Marker mode constants
        public static string MARKER_EXPOSURE_SLIDER_NAME = "Marker Exposure";
        public static string MARKER_THRESHOLD_SLIDER_NAME = "Marker Threshold";

        /*public static int MARKER_MIN_EXPOSURE = 5;
        public static int MARKER_MAX_EXPOSURE = 1000;

        public static int MARKER_MIN_THRESHOLD = 50;
        public static int MARKER_MAX_THRESHOLD = 900;*/

        // Video mode constants
        public static string VIDEO_EXPOSURE_SLIDER_NAME = "Video Exposure";
        public static string VIDEO_FLASH_SLIDER_NAME = "Video Flash";

       /* public static int VIDEO_MIN_EXPOSURE = 5;
        public static int VIDEO_MAX_EXPOSURE = 40000;

        public static int VIDEO_MIN_FLASH = 0;
        public static int VIDEO_MAX_FLASH = 1000;*/

        // Packet constants
        public static string MARKER_EXPOSURE_PACKET_STRING = "Marker_Exposure";
        public static string MARKER_THRESHOLD_PACKET_STRING = "Marker_Threshold";
        public static string VIDEO_EXPOSURE_PACKET_STRING = "Video_Exposure";
        public static string VIDEO_FLASH_PACKET_STRING = "Video_Flash_Time";
    }
}
